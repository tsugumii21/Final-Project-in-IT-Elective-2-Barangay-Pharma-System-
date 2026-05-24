using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Validation;

/// <summary>
/// Validates that a date value is not in the future.
/// Use on Birthdate fields to ensure the user cannot enter a future birthdate.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotFutureDateAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "Date cannot be in the future.";

    public NotFutureDateAttribute() : base(DefaultErrorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? DefaultErrorMessage);
            }
        }

        // Allow null — use [Required] separately if the field is mandatory
        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a date value is not in the past.
/// Use on ExpiryDate fields to ensure medicines are not added with a past expiry date.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotPastDateAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "Expiry date cannot be in the past.";

    public NotPastDateAttribute() : base(DefaultErrorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? DefaultErrorMessage);
            }
        }

        // Allow null — use [Required] separately if the field is mandatory
        return ValidationResult.Success;
    }
}
