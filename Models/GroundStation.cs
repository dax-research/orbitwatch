using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models
{
    public class GroundStation : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100)]
        public required string Location { get; set; }

        [Required]
        [StringLength(100)]
        public required string Country { get; set; }

        [StringLength(100)]
        public string? Operator { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Status))
            {
                yield break;
            }

            string[] allowed = ["Operational", "Maintenance", "Offline", "Decommissioned"];
            if (!allowed.Contains(Status, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Select a valid ground station status.",
                    [nameof(Status)]);
            }
        }
    }
}
