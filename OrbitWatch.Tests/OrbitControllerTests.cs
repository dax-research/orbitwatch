using Microsoft.AspNetCore.Mvc;
using Moq;
using OrbitWatch.Controllers.Api;
using OrbitWatch.Models;
using OrbitWatch.Models.Api;
using OrbitWatch.Models.Orbit;
using OrbitWatch.Repositories;
using OrbitWatch.Services;
using OrbitWatch.Services.Orbit;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OrbitWatch.Tests
{
    public class OrbitControllerTests
    {
        private readonly Mock<ISatelliteRepository> _repoMock;
        private readonly Mock<ISatelliteApiService> _apiServiceMock;
        private readonly Mock<IOrbitPropagationService> _propagationServiceMock;
        private readonly OrbitController _controller;

        public OrbitControllerTests()
        {
            _repoMock = new Mock<ISatelliteRepository>();
            _apiServiceMock = new Mock<ISatelliteApiService>();
            _propagationServiceMock = new Mock<IOrbitPropagationService>();

            _controller = new OrbitController(
                _repoMock.Object,
                _apiServiceMock.Object,
                _propagationServiceMock.Object
            );
        }

        [Fact]
        public async Task GetPosition_ValidRequest_ReturnsOkWithPosition()
        {
            // Arrange
            var satelliteId = 1;
            var noradId = "25544";
            var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            var satellite = new Satellite { Id = satelliteId, NoradId = noradId };
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544 };
            var apiResult = new CelesTrakResult { Data = gpData, IsStale = false };
            var expectedPosition = new SatellitePosition
            {
                NoradCatalogId = 25544,
                TimestampUtc = timestamp,
                LatitudeDegrees = 45.0,
                LongitudeDegrees = -90.0,
                AltitudeKm = 400.0,
                VelocityKmPerSec = 7.5
            };

            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync(satellite);
            _apiServiceMock.Setup(s => s.GetSatelliteDataAsync(noradId)).ReturnsAsync(apiResult);
            _propagationServiceMock.Setup(p => p.GetPosition(gpData, timestamp)).Returns(expectedPosition);

            // Act
            var result = await _controller.GetPosition(satelliteId, timestamp);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var position = Assert.IsType<SatellitePosition>(okResult.Value);
            
            Assert.Equal(45.0, position.LatitudeDegrees);
            Assert.Equal(-90.0, position.LongitudeDegrees);
            Assert.Equal(400.0, position.AltitudeKm);
            Assert.Equal(7.5, position.VelocityKmPerSec);
            Assert.False(position.IsStaleOrbitalData);
        }

        [Fact]
        public async Task GetPosition_StaleData_ReturnsPositionWithStaleFlag()
        {
            // Arrange
            var satelliteId = 1;
            var noradId = "25544";
            var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            var satellite = new Satellite { Id = satelliteId, NoradId = noradId };
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544 };
            var apiResult = new CelesTrakResult { Data = gpData, IsStale = true };
            var expectedPosition = new SatellitePosition
            {
                NoradCatalogId = 25544,
                TimestampUtc = timestamp,
                LatitudeDegrees = 45.0,
                LongitudeDegrees = -90.0,
                AltitudeKm = 400.0,
                VelocityKmPerSec = 7.5
            };

            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync(satellite);
            _apiServiceMock.Setup(s => s.GetSatelliteDataAsync(noradId)).ReturnsAsync(apiResult);
            _propagationServiceMock.Setup(p => p.GetPosition(gpData, timestamp)).Returns(expectedPosition);

            // Act
            var result = await _controller.GetPosition(satelliteId, timestamp);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var position = Assert.IsType<SatellitePosition>(okResult.Value);
            
            Assert.True(position.IsStaleOrbitalData);
        }

        [Fact]
        public async Task GetPosition_SatelliteNotFound_ReturnsNotFound()
        {
            // Arrange
            var satelliteId = 999;
            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync((Satellite?)null);

            // Act
            var result = await _controller.GetPosition(satelliteId, null);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetPosition_CelesTrakDataUnavailable_Returns502()
        {
            // Arrange
            var satelliteId = 1;
            var noradId = "25544";
            var satellite = new Satellite { Id = satelliteId, NoradId = noradId };
            var apiResult = new CelesTrakResult { Data = null, IsStale = false };

            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync(satellite);
            _apiServiceMock.Setup(s => s.GetSatelliteDataAsync(noradId)).ReturnsAsync(apiResult);

            // Act
            var result = await _controller.GetPosition(satelliteId, null);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetPosition_PropagationFails_Returns500()
        {
            // Arrange
            var satelliteId = 1;
            var noradId = "25544";
            var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            var satellite = new Satellite { Id = satelliteId, NoradId = noradId };
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544 };
            var apiResult = new CelesTrakResult { Data = gpData, IsStale = false };

            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync(satellite);
            _apiServiceMock.Setup(s => s.GetSatelliteDataAsync(noradId)).ReturnsAsync(apiResult);
            _propagationServiceMock.Setup(p => p.GetPosition(gpData, timestamp)).Returns((SatellitePosition?)null);

            // Act
            var result = await _controller.GetPosition(satelliteId, timestamp);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetTrajectory_GeneratesFiveMinutePath_Correctly()
        {
            // Arrange
            var satelliteId = 1;
            var noradId = "25544";
            var satellite = new Satellite { Id = satelliteId, NoradId = noradId };
            var gpData = new CelesTrakGpData { NoradCatalogId = 25544, MeanMotion = 15.5 };
            var apiResult = new CelesTrakResult { Data = gpData, IsStale = false };

            _repoMock.Setup(r => r.GetByIdAsync(satelliteId)).ReturnsAsync(satellite);
            _apiServiceMock.Setup(s => s.GetSatelliteDataAsync(noradId)).ReturnsAsync(apiResult);

            _propagationServiceMock.Setup(p => p.GetPosition(gpData, It.IsAny<DateTime>()))
                .Returns((CelesTrakGpData data, DateTime t) => new SatellitePosition { TimestampUtc = t, CartesianXKm = 1000 });
            
            var pathList = new List<SatellitePosition>();
            var next5MinList = new List<SatellitePosition>();
            
            // Mock propagation to return list of requested times
            _propagationServiceMock.Setup(p => p.GetPositions(gpData, It.IsAny<IEnumerable<DateTime>>()))
                .Returns((CelesTrakGpData data, IEnumerable<DateTime> times) => 
                {
                    var list = new List<SatellitePosition>();
                    foreach(var t in times)
                    {
                        list.Add(new SatellitePosition { TimestampUtc = t, CartesianXKm = 1000 });
                    }
                    return list;
                });

            // Act
            var result = await _controller.GetTrajectory(satelliteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObj = okResult.Value;
            
            var next5Minutes = responseObj.GetType().GetProperty("next5Minutes").GetValue(responseObj, null) as IReadOnlyList<SatellitePosition>;
            var currentPosition = responseObj.GetType().GetProperty("currentPosition").GetValue(responseObj, null) as SatellitePosition;

            Assert.NotNull(next5Minutes);
            // From s=0 to s=300 in 30 sec steps -> 11 points
            Assert.Equal(11, next5Minutes.Count);
            
            // Starts exactly at current position (t + 0)
            Assert.Equal(next5Minutes[0].TimestampUtc, currentPosition.TimestampUtc);
            
            // Monotonically increasing by 30 seconds
            for (int i = 1; i < next5Minutes.Count; i++)
            {
                var diff = next5Minutes[i].TimestampUtc - next5Minutes[i-1].TimestampUtc;
                Assert.Equal(TimeSpan.FromSeconds(30), diff);
            }

            // Total duration is 300 seconds
            var totalDuration = next5Minutes[^1].TimestampUtc - next5Minutes[0].TimestampUtc;
            Assert.Equal(TimeSpan.FromMinutes(5), totalDuration);
        }
    }
}
