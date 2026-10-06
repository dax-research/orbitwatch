using Microsoft.Extensions.Logging;
using Moq;
using OrbitWatch.Models.Api;
using OrbitWatch.Services.Orbit;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrbitWatch.Tests
{
    public class Sgp4OrbitPropagationServiceTests
    {
        private readonly ILogger<Sgp4OrbitPropagationService> _logger;
        private readonly Sgp4OrbitPropagationService _service;

        public Sgp4OrbitPropagationServiceTests()
        {
            var loggerMock = new Mock<ILogger<Sgp4OrbitPropagationService>>();
            _logger = loggerMock.Object;
            _service = new Sgp4OrbitPropagationService(_logger);
        }

        private CelesTrakGpData GetIssTestData()
        {
            // Real-ish test data for NORAD 25544 (ISS)
            return new CelesTrakGpData
            {
                ObjectName = "ISS (ZARYA)",
                ObjectId = "1998-067A",
                NoradCatalogId = 25544,
                ClassificationType = "U",
                Epoch = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                MeanMotion = 15.49560532,
                Eccentricity = 0.0006703,
                Inclination = 51.6400,
                RightAscensionOfAscendingNode = 208.9163,
                ArgumentOfPericenter = 30.1107,
                MeanAnomaly = 330.0289,
                EphemerisType = 0,
                BStar = 0.0001027,
                MeanMotionDot = 0.00016717,
                MeanMotionDdot = 0.0,
                ElementSetNumber = 900,
                RevolutionNumberAtEpoch = 1
            };
        }

        [Fact]
        public void GetPosition_WithValidData_ReturnsSensiblePosition()
        {
            // Arrange
            var data = GetIssTestData();
            var utcTime = data.Epoch!.Value;

            // Act
            var position = _service.GetPosition(data, utcTime);

            // Assert
            Assert.NotNull(position);
            Assert.Equal(25544, position.NoradCatalogId);
            Assert.Equal(utcTime, position.TimestampUtc);
            Assert.InRange(position.LatitudeDegrees, -90, 90);
            Assert.InRange(position.LongitudeDegrees, -180, 180);
            Assert.True(position.AltitudeKm > 0);
            Assert.True(position.AltitudeKm < 2000); // LEO
            Assert.True(position.VelocityKmPerSec > 5 && position.VelocityKmPerSec < 10);
            
            // Cartesian position check
            Assert.NotEqual(0, position.CartesianXKm);
            Assert.NotEqual(0, position.CartesianYKm);
            Assert.NotEqual(0, position.CartesianZKm);
            
            // Check that the orbital radius is roughly Earth Radius + Altitude
            double orbitalRadius = Math.Sqrt(Math.Pow(position.CartesianXKm, 2) + Math.Pow(position.CartesianYKm, 2) + Math.Pow(position.CartesianZKm, 2));
            Assert.True(orbitalRadius > 6371);
            Assert.True(orbitalRadius < 8000);
        }

        [Fact]
        public void GetPositions_WithMultipleTimestamps_ReturnsValidPositions()
        {
            // Arrange
            var data = GetIssTestData();
            var baseTime = data.Epoch!.Value;
            var times = new List<DateTime>
            {
                baseTime,
                baseTime.AddSeconds(60),
                baseTime.AddSeconds(180),
                baseTime.AddSeconds(300)
            };

            // Act
            var positions = _service.GetPositions(data, times);

            // Assert
            Assert.Equal(4, positions.Count);
            
            // Check they are distinct over time (satellite moves)
            Assert.NotEqual(positions[0].LatitudeDegrees, positions[1].LatitudeDegrees);
            Assert.NotEqual(positions[0].LongitudeDegrees, positions[1].LongitudeDegrees);

            foreach (var pos in positions)
            {
                Assert.InRange(pos.LatitudeDegrees, -90, 90);
                Assert.InRange(pos.LongitudeDegrees, -180, 180);
                Assert.True(pos.AltitudeKm > 0);
            }
        }
        
        [Fact]
        public void GetPosition_WithInvalidData_ReturnsNull()
        {
            // Arrange
            var data = GetIssTestData();
            data.MeanMotion = -1; // Invalid
            var utcTime = data.Epoch!.Value;

            // Act
            var position = _service.GetPosition(data, utcTime);

            // Assert
            Assert.Null(position);
        }
    }
}
