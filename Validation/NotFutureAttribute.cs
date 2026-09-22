using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Validation;

public sealed class NotFutureAttribute : ValidationAttribute
{
    public bool DateOnly { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime dateTime)
        {
            return ValidationResult.Success;
        }

        var isFuture = DateOnly
            ? dateTime.Date > DateTime.UtcNow.Date
            : dateTime > DateTime.UtcNow;

        if (!isFuture)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            ErrorMessage ?? $"{validationContext.DisplayName} cannot be in the future.");
    }
}
