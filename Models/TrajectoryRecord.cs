using System.ComponentModel.DataAnnotations;
using OrbitWatch.Validation;

namespace OrbitWatch.Models
{
    public class TrajectoryRecord
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        [Required]
        [NotFuture(ErrorMessage = "A trajectory record cannot be recorded in the future.")]
        public DateTime RecordedAt { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Altitude cannot be negative.")]
        public double Altitude { get; set; }

        [Range(0, double.MaxValue, MinimumIsExclusive = true, ErrorMessage = "Velocity must be greater than 0.")]
        public double Velocity { get; set; }

        [Required]
        [StringLength(100)]
        [AllowedValues("LEO", "MEO", "GEO", "HEO", "SSO",
            ErrorMessage = "Select a valid orbit classification.")]
        public required string OrbitType { get; set; }

        public required Satellite Satellite { get; set; }
    }
}
