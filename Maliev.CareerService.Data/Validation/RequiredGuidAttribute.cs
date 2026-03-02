using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Validation;

/// <summary>
/// Validation attribute that ensures a Guid is not empty (Guid.Empty)
/// </summary>
public class RequiredGuidAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guidValue && guidValue == Guid.Empty)
        {
            var memberName = validationContext.MemberName ?? "Field";
            return new ValidationResult(
                ErrorMessage ?? $"The {memberName} field is required and cannot be empty.",
                new[] { validationContext.MemberName ?? string.Empty });
        }

        return ValidationResult.Success;
    }
}
