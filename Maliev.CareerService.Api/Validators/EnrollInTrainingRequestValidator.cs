using FluentValidation;
using Maliev.CareerService.Api.Models.Enrollments;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for EnrollInTrainingRequest
/// </summary>
public class EnrollInTrainingRequestValidator : AbstractValidator<EnrollInTrainingRequest>
{
    public EnrollInTrainingRequestValidator()
    {
        RuleFor(x => x.TrainingProgramId)
            .NotEmpty().WithMessage("Training program ID is required")
            .Must(BeValidGuid).WithMessage("Training program ID must be a valid GUID");
    }

    private bool BeValidGuid(Guid id)
    {
        return id != Guid.Empty;
    }
}
