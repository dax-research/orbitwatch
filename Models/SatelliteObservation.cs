using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class SatelliteObservation
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        [Required]
        public DateTime ObservedAt { get; set; }

        [Required]
        [StringLength(100)]
        public string ObservationType { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public double? Temperature { get; set; }

        public double? SignalStrength { get; set; }

        [Required]
        [StringLength(100)]
        public string GroundStation { get; set; }

        // Navigation property
        public Satellite Satellite { get; set; }
    }
}