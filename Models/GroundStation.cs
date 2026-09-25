using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            // Validate geographic coordinates
            if (!string.IsNullOrWhiteSpace(Location))
            {
                var coordinatePattern =
                    @"^\s*(\d{1,2}(?:\.\d+)?)\s*°?\s*([NS])\s*,\s*(\d{1,3}(?:\.\d+)?)\s*°?\s*([EW])\s*$";

                var match = Regex.Match(
                    Location,
                    coordinatePattern,
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    yield return new ValidationResult(
                        "Enter coordinates in the format: 67.8557° N, 20.2251° E.",
                        [nameof(Location)]);
                }
                else
                {
                    double latitude = double.Parse(match.Groups[1].Value);
                    double longitude = double.Parse(match.Groups[3].Value);

                    if (latitude > 90)
                    {
                        yield return new ValidationResult(
                            "Latitude must be between 0° and 90°.",
                            [nameof(Location)]);
                    }

                    if (longitude > 180)
                    {
                        yield return new ValidationResult(
                            "Longitude must be between 0° and 180°.",
                            [nameof(Location)]);
                    }
                }
            }

            // Validate status
            if (!string.IsNullOrWhiteSpace(Status))
            {
                string[] allowed =
                [
                    "Operational",
                    "Maintenance",
                    "Offline",
                    "Decommissioned"
                ];

                if (!allowed.Contains(
                    Status,
                    StringComparer.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Select a valid ground station status.",
                        [nameof(Status)]);
                }
            }
        }
    }
}