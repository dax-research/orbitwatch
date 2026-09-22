using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models;

public class Mission : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime LaunchDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required]
    [StringLength(150)]
    public string Agency { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [AllowedValues("Planned", "Active", "Completed", "On Hold",
        ErrorMessage = "Select a valid mission status.")]
    public string Status { get; set; } = string.Empty;

    public List<Satellite> Satellites { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var today = DateTime.UtcNow.Date;
        var launchDate = LaunchDate.Date;
        var endDate = EndDate?.Date;
        var status = Status?.Trim() ?? string.Empty;

        if (endDate.HasValue && endDate.Value < launchDate)
        {
            yield return new ValidationResult(
                "End date cannot be earlier than launch date.",
                [nameof(EndDate)]);
        }

        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            if (launchDate > today)
            {
                yield return new ValidationResult(
                    "An active mission cannot have a launch date in the future.",
                    [nameof(LaunchDate)]);
            }

            if (endDate.HasValue && endDate.Value < today)
            {
                yield return new ValidationResult(
                    "An active mission cannot have an end date in the past.",
                    [nameof(EndDate)]);
            }
        }

        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            if (!endDate.HasValue)
            {
                yield return new ValidationResult(
                    "Completed missions must have an end date.",
                    [nameof(EndDate)]);
            }
            else if (endDate.Value > today)
            {
                yield return new ValidationResult(
                    "Completed missions cannot have an end date in the future.",
                    [nameof(EndDate)]);
            }
        }

        if (string.Equals(status, "Planned", StringComparison.OrdinalIgnoreCase)
            && launchDate < today)
        {
            yield return new ValidationResult(
                "A planned mission cannot have a launch date in the past.",
                [nameof(LaunchDate)]);
        }
    }
}
