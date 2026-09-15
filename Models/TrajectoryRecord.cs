using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class TrajectoryRecord
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public double Velocity { get; set; }

        [Required]
        [StringLength(100)]
        public required string OrbitType { get; set; }

        public required Satellite Satellite { get; set; }
    }
}