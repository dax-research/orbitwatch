using System.ComponentModel.DataAnnotations;
using OrbitWatch.Validation;

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
        [NotFuture(ErrorMessage = "An incident cannot occur in the future.")]
        public DateTime OccurredAt { get; set; }

        [Required]
        [StringLength(50)]
        [AllowedValues("Low", "Medium", "High", "Critical",
            ErrorMessage = "Select a valid severity.")]
        public required string Severity { get; set; }

        [Required]
        [StringLength(50)]
        [AllowedValues("Open", "Investigating", "Resolved", "Closed",
            ErrorMessage = "Select a valid incident status.")]
        public required string Status { get; set; }

        public required Satellite Satellite { get; set; }
    }
}
