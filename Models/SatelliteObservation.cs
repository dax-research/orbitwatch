using System.ComponentModel.DataAnnotations;
using OrbitWatch.Validation;

namespace OrbitWatch.Models
{
    public class SatelliteObservation : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public int SatelliteId { get; set; }

        [Required]
        [NotFuture(ErrorMessage = "An observation cannot be recorded in the future.")]
        public DateTime ObservedAt { get; set; }

        [Required]
        [StringLength(100)]
        [AllowedValues("Optical", "Radar", "Telemetry", "Spectrometric", "Thermal",
            ErrorMessage = "Select a valid observation type.")]
        public required string ObservationType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public double? Temperature { get; set; }

        public double? SignalStrength { get; set; }

        [Required]
        [StringLength(100)]
        public required string GroundStation { get; set; }

        public required Satellite Satellite { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Temperature.HasValue && Temperature.Value < -273.15)
            {
                yield return new ValidationResult(
                    "Temperature cannot be below absolute zero (-273.15 °C).",
                    [nameof(Temperature)]);
            }
        }
    }
}
