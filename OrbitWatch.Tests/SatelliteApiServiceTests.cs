using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using OrbitWatch.Models;
using OrbitWatch.Models.Api;
using OrbitWatch.Repositories;
using OrbitWatch.Services;
using Xunit;

namespace OrbitWatch.Tests
{
    public class SatelliteApiServiceTests
    {
        private readonly Mock<ICelesTrakOrbitalDataCacheRepository> _repoMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly SatelliteApiService _service;

        public SatelliteApiServiceTests()
        {
            _repoMock = new Mock<ICelesTrakOrbitalDataCacheRepository>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://celestrak.org/")
            };

            _service = new SatelliteApiService(
                _httpClient,
                _memoryCache,
                _repoMock.Object,
                NullLogger<SatelliteApiService>.Instance);
        }

        [Fact]
        public async Task GetSatelliteDataAsync_FreshMemoryCacheHit_ReturnsDataWithoutDbOrApiCall()
        {
            // Arrange
            var noradId = "25544";
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544 };
            
            // Prime memory cache with reflection/dynamic or just a wrapper since CacheEntry is private.
            // But since CacheEntry is private, we can't easily seed it directly without using the service.
            // Let's seed it via a mock DB hit first to populate memory cache.
            
            var dbRecord = new CelesTrakOrbitalDataCache
            {
                NoradId = noradId,
                PayloadJson = JsonSerializer.Serialize(new List<CelesTrakGpData> { gpData }),
                FetchedAtUtc = DateTime.UtcNow
            };
            
            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync(dbRecord);
            
            // Act 1 - Misses memory, hits DB, populates memory
            var result1 = await _service.GetSatelliteDataAsync(noradId);
            
            // Clear mock invocations to track calls for Act 2
            _repoMock.Invocations.Clear();

            // Act 2 - Should hit memory cache directly
            var result2 = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.NotNull(result2.Data);
            Assert.False(result2.IsStale);
            Assert.Equal(25544, result2.Data.NoradCatalogId);
            
            _repoMock.Verify(r => r.GetByNoradIdAsync(It.IsAny<string>()), Times.Never);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetSatelliteDataAsync_PersistentCacheFresh_ReturnsDataWithoutApiCall()
        {
            // Arrange
            var noradId = "25544";
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544 };
            var dbRecord = new CelesTrakOrbitalDataCache
            {
                NoradId = noradId,
                PayloadJson = JsonSerializer.Serialize(new List<CelesTrakGpData> { gpData }),
                FetchedAtUtc = DateTime.UtcNow.AddMinutes(-30) // Fresh
            };

            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync(dbRecord);

            // Act
            var result = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.NotNull(result.Data);
            Assert.False(result.IsStale);
            Assert.Equal(25544, result.Data.NoradCatalogId);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetSatelliteDataAsync_PersistentCacheStale_ApiSucceeds_ReturnsFreshDataAndUpdate()
        {
            // Arrange
            var noradId = "25544";
            var oldGpData = new CelesTrakGpData { NoradCatalogId = 25544, ObjectName = "OLD" };
            var newGpData = new CelesTrakGpData { NoradCatalogId = 25544, ObjectName = "NEW" };
            
            var dbRecord = new CelesTrakOrbitalDataCache
            {
                NoradId = noradId,
                PayloadJson = JsonSerializer.Serialize(new List<CelesTrakGpData> { oldGpData }),
                FetchedAtUtc = DateTime.UtcNow.AddHours(-10) // Stale
            };

            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync(dbRecord);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<CelesTrakGpData> { newGpData }))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("CATNR=25544")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.NotNull(result.Data);
            Assert.False(result.IsStale);
            Assert.Equal("NEW", result.Data.ObjectName);

            _repoMock.Verify(r => r.UpsertAsync(It.Is<CelesTrakOrbitalDataCache>(c => c.NoradId == noradId && c.PayloadJson.Contains("NEW"))), Times.Once);
        }

        [Fact]
        public async Task GetSatelliteDataAsync_PersistentCacheStale_ApiFails_ReturnsStaleDataWithout502()
        {
            // Arrange
            var noradId = "25544";
            var oldGpData = new CelesTrakGpData { NoradCatalogId = 25544, ObjectName = "OLD" };
            
            var dbRecord = new CelesTrakOrbitalDataCache
            {
                NoradId = noradId,
                PayloadJson = JsonSerializer.Serialize(new List<CelesTrakGpData> { oldGpData }),
                FetchedAtUtc = DateTime.UtcNow.AddHours(-10) // Stale but valid for fallback (<= 24h)
            };

            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync(dbRecord);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable); // 503

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.NotNull(result.Data);
            Assert.True(result.IsStale);
            Assert.Equal("OLD", result.Data.ObjectName);
        }

        [Fact]
        public async Task GetSatelliteDataAsync_NoCaches_ApiFails_ReturnsNull()
        {
            // Arrange
            var noradId = "25544";
            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync((CelesTrakOrbitalDataCache?)null);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable); // 503

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.Null(result.Data);
            // In OrbitController, this null Data translates to a 502 response
        }

        [Fact]
        public async Task GetSatelliteDataAsync_PersistedMalformedJson_GracefullyFetchesFromApi()
        {
            // Arrange
            var noradId = "25544";
            var dbRecord = new CelesTrakOrbitalDataCache
            {
                NoradId = noradId,
                PayloadJson = "{ INVALID JSON ]",
                FetchedAtUtc = DateTime.UtcNow
            };

            _repoMock.Setup(r => r.GetByNoradIdAsync(noradId)).ReturnsAsync(dbRecord);

            var newGpData = new CelesTrakGpData { NoradCatalogId = 25544, ObjectName = "NEW" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<CelesTrakGpData> { newGpData }))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.GetSatelliteDataAsync(noradId);

            // Assert
            Assert.NotNull(result.Data);
            Assert.False(result.IsStale);
            Assert.Equal("NEW", result.Data.ObjectName);
        }
    }
}
