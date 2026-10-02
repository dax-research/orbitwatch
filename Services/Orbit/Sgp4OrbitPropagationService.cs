using Microsoft.Extensions.Logging;
using OrbitWatch.Models.Api;
using OrbitWatch.Models.Orbit;
using SGPdotNET.Observation;
using SGPdotNET.Parsers;

namespace OrbitWatch.Services.Orbit
{
    /// <summary>
    /// SGP4-based orbit propagation service.
    /// Converts CelesTrak GP orbital elements to SGP.NET's OmmData format,
    /// then uses the SGP4 algorithm to calculate satellite positions at
    /// specified UTC timestamps.
    ///
    /// No additional HTTP requests are made per calculation — all propagation
    /// is performed locally from the provided orbital elements.
    /// </summary>
    public class Sgp4OrbitPropagationService : IOrbitPropagationService
    {
        private readonly ILogger<Sgp4OrbitPropagationService> _logger;

        public Sgp4OrbitPropagationService(ILogger<Sgp4OrbitPropagationService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public SatellitePosition? GetPosition(CelesTrakGpData gpData, DateTime utcTime)
        {
            if (!ValidateGpData(gpData))
            {
                _logger.LogWarning(
                    "Invalid or incomplete GP data for NORAD {NoradId}. " +
                    "Cannot propagate orbit.",
                    gpData.NoradCatalogId);
                return null;
            }

            try
            {
                var satellite = CreateSatellite(gpData);
                return Propagate(satellite, gpData.NoradCatalogId!.Value, utcTime);
            }
            catch (SGPdotNET.Exception.DecayedException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Satellite NORAD {NoradId} has decayed. Cannot propagate.",
                    gpData.NoradCatalogId);
                return null;
            }
            catch (SGPdotNET.Exception.SatellitePropagationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "SGP4 propagation failed for NORAD {NoradId} at {Time:u}.",
                    gpData.NoradCatalogId, utcTime);
                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error propagating orbit for NORAD {NoradId} at {Time:u}.",
                    gpData.NoradCatalogId, utcTime);
                return null;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<SatellitePosition> GetPositions(
            CelesTrakGpData gpData,
            IEnumerable<DateTime> utcTimes)
        {
            if (!ValidateGpData(gpData))
            {
                _logger.LogWarning(
                    "Invalid or incomplete GP data for NORAD {NoradId}. " +
                    "Cannot propagate orbit for multiple timestamps.",
                    gpData.NoradCatalogId);
                return [];
            }

            var results = new List<SatellitePosition>();

            try
            {
                // Create the satellite object once and reuse for all timestamps.
                // This is the key design point: no additional HTTP requests needed.
                var satellite = CreateSatellite(gpData);
                var noradId = gpData.NoradCatalogId!.Value;

                foreach (var time in utcTimes)
                {
                    try
                    {
                        var position = Propagate(satellite, noradId, time);
                        if (position != null)
                        {
                            results.Add(position);
                        }
                    }
                    catch (SGPdotNET.Exception.DecayedException)
                    {
                        _logger.LogWarning(
                            "Satellite NORAD {NoradId} decayed at {Time:u}. " +
                            "Skipping this timestamp.",
                            noradId, time);
                    }
                    catch (SGPdotNET.Exception.SatellitePropagationException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "SGP4 propagation failed for NORAD {NoradId} at {Time:u}. " +
                            "Skipping this timestamp.",
                            noradId, time);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error creating satellite for NORAD {NoradId}.",
                    gpData.NoradCatalogId);
            }

            return results;
        }

        /// <summary>
        /// Validates that the CelesTrak GP data contains all required orbital elements
        /// for SGP4 propagation.
        /// </summary>
        private bool ValidateGpData(CelesTrakGpData gpData)
        {
            if (gpData == null) return false;
            if (gpData.NoradCatalogId == null) return false;
            if (gpData.Epoch == null) return false;
            if (gpData.MeanMotion == null) return false;
            if (gpData.Eccentricity == null) return false;
            if (gpData.Inclination == null) return false;
            if (gpData.RightAscensionOfAscendingNode == null) return false;
            if (gpData.ArgumentOfPericenter == null) return false;
            if (gpData.MeanAnomaly == null) return false;

            // Mean motion must be positive (revolutions per day)
            if (gpData.MeanMotion <= 0) return false;

            // Eccentricity must be in [0, 1) for bound orbits
            if (gpData.Eccentricity < 0 || gpData.Eccentricity >= 1) return false;

            return true;
        }

        /// <summary>
        /// Creates an SGP.NET Satellite object from CelesTrak GP data
        /// by mapping the GP fields to the OMM (Orbit Mean-Elements Message) format
        /// that SGP.NET can consume directly.
        /// </summary>
        private static Satellite CreateSatellite(CelesTrakGpData gpData)
        {
            var ommData = new OmmData
            {
                ObjectName = gpData.ObjectName ?? $"NORAD {gpData.NoradCatalogId}",
                ObjectID = gpData.ObjectId ?? string.Empty,
                NoradCatID = (uint)gpData.NoradCatalogId!.Value,
                ClassificationType = gpData.ClassificationType ?? "U",
                Epoch = DateTime.SpecifyKind(gpData.Epoch!.Value, DateTimeKind.Utc),
                MeanMotion = gpData.MeanMotion!.Value,
                Eccentricity = gpData.Eccentricity!.Value,
                Inclination = gpData.Inclination!.Value,
                RAOfAscNode = gpData.RightAscensionOfAscendingNode!.Value,
                ArgOfPericenter = gpData.ArgumentOfPericenter!.Value,
                MeanAnomaly = gpData.MeanAnomaly!.Value,
                EphemerisType = gpData.EphemerisType ?? 0,
                BStar = gpData.BStar ?? 0.0,
                MeanMotionDot = gpData.MeanMotionDot ?? 0.0,
                MeanMotionDDot = gpData.MeanMotionDdot ?? 0.0,
                ElementSetNo = (uint)(gpData.ElementSetNumber ?? 0),
                RevAtEpoch = (uint)(gpData.RevolutionNumberAtEpoch ?? 0)
            };

            return new Satellite(ommData);
        }

        /// <summary>
        /// Runs SGP4 propagation for a single timestamp and converts
        /// the result to a <see cref="SatellitePosition"/>.
        /// </summary>
        private static SatellitePosition? Propagate(
            Satellite satellite,
            int noradCatalogId,
            DateTime utcTime)
        {
            var utc = utcTime.Kind == DateTimeKind.Utc
                ? utcTime
                : DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            var eci = satellite.Predict(utc);
            var geodetic = eci.ToGeodetic();

            return new SatellitePosition
            {
                NoradCatalogId = noradCatalogId,
                TimestampUtc = utc,
                LatitudeDegrees = geodetic.Latitude.Degrees,
                LongitudeDegrees = geodetic.Longitude.Degrees,
                AltitudeKm = geodetic.Altitude,
                VelocityKmPerSec = eci.Velocity.Length
            };
        }
    }
}
