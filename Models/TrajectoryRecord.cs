using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class TrajectoryRecord
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        public DateTime RecordedAt { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public double Velocity { get; set; }

        [StringLength(100)]
        public string OrbitType { get; set; }

        // Navigation property
        public Satellite Satellite { get; set; }
    }
}