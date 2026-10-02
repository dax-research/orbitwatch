using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrbitWatch.Models.Api;

namespace OrbitWatch.Services
{
    public class SatelliteApiService : ISatelliteApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SatelliteApiService> _logger;

        // Cache freshness/stale policy as per requirements
        private static readonly TimeSpan FreshDuration = TimeSpan.FromHours(2);
        private static readonly TimeSpan StaleDuration = TimeSpan.FromHours(24);

        private class CacheEntry
        {
            public CelesTrakGpData Data { get; set; } = null!;
            public DateTime FetchedAtUtc { get; set; }
        }

        public SatelliteApiService(HttpClient httpClient, IMemoryCache cache, ILogger<SatelliteApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CelesTrakResult> GetSatelliteDataAsync(string noradId)
        {
            if (string.IsNullOrWhiteSpace(noradId))
            {
                return new CelesTrakResult { Data = null };
            }

            var cacheKey = $"celestrak:{noradId}";
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(cacheKey, out CacheEntry? cachedEntry) && cachedEntry != null)
            {
                var age = now - cachedEntry.FetchedAtUtc;
                if (age <= FreshDuration)
                {
                    _logger.LogInformation("Cache hit for NORAD {NoradId}. Data is fresh (age: {Age}).", noradId, age);
                    return new CelesTrakResult { Data = cachedEntry.Data, IsStale = false };
                }

                _logger.LogInformation("Cache for NORAD {NoradId} is stale (age: {Age}). Attempting to refresh...", noradId, age);
                // Attempt to refresh, if fail use stale
                var freshData = await FetchFromApiAsync(noradId);
                if (freshData != null)
                {
                    _logger.LogInformation("Successfully refreshed CelesTrak data for NORAD {NoradId}.", noradId);
                    UpdateCache(cacheKey, freshData, now);
                    return new CelesTrakResult { Data = freshData, IsStale = false };
                }
                
                // Fallback to stale
                _logger.LogWarning("Failed to refresh CelesTrak data for NORAD {NoradId}. Falling back to stale data.", noradId);
                return new CelesTrakResult { Data = cachedEntry.Data, IsStale = true };
            }

            _logger.LogInformation("Cache miss for NORAD {NoradId}. Fetching from CelesTrak...", noradId);
            var data = await FetchFromApiAsync(noradId);
            if (data != null)
            {
                UpdateCache(cacheKey, data, now);
            }
            return new CelesTrakResult { Data = data, IsStale = false };
        }

        private async Task<CelesTrakGpData?> FetchFromApiAsync(string noradId)
        {
            try
            {
                var url = $"NORAD/elements/gp.php?CATNR={Uri.EscapeDataString(noradId)}&FORMAT=JSON";
                using var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<List<CelesTrakGpData>>();
                return data?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching from CelesTrak for NORAD {NoradId}", noradId);
                return null;
            }
        }

        private void UpdateCache(string cacheKey, CelesTrakGpData data, DateTime fetchedAt)
        {
            var entry = new CacheEntry { Data = data, FetchedAtUtc = fetchedAt };
            _cache.Set(cacheKey, entry, StaleDuration);
        }
    }
}