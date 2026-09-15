using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class Incident
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Type { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; }

        [Required]
        [StringLength(50)]
        public required string Severity { get; set; }

        [Required]
        [StringLength(50)]
        public required string Status { get; set; }

        public required Satellite Satellite { get; set; }
    }
}