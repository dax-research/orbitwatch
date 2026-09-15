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
        public string Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Severity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation property
        public Satellite Satellite { get; set; }
    }
}