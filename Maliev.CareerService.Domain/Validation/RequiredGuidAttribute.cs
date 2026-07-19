using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Domain.Validation;

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
