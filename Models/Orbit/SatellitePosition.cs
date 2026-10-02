namespace OrbitWatch.Models.Orbit
{
    /// <summary>
    /// Represents a satellite's propagated/predicted position at a specific UTC timestamp.
    /// Calculated locally from CelesTrak orbital elements using SGP4 propagation.
    /// This is runtime data, not persisted to the database.
    /// </summary>
    public class SatellitePosition
    {
        /// <summary>
        /// NORAD catalog ID of the satellite.
        /// </summary>
        public int NoradCatalogId { get; set; }

        /// <summary>
        /// The UTC timestamp for which this position was calculated.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Geodetic latitude in degrees (-90 to +90).
        /// </summary>
        public double LatitudeDegrees { get; set; }

        /// <summary>
        /// Geodetic longitude in degrees (-180 to +180).
        /// </summary>
        public double LongitudeDegrees { get; set; }

        /// <summary>
        /// Altitude above the WGS84 reference ellipsoid in kilometers.
        /// </summary>
        public double AltitudeKm { get; set; }

        /// <summary>
        /// Orbital velocity magnitude in kilometers per second.
        /// </summary>
        public double VelocityKmPerSec { get; set; }

        /// <summary>
        /// Indicates if the position was propagated using stale orbital elements.
        /// </summary>
        public bool IsStaleOrbitalData { get; set; }
    }
}
