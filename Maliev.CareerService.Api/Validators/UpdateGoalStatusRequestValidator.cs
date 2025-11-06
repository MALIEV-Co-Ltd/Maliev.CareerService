using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data.Models;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateGoalStatusRequest
/// </summary>
public class UpdateGoalStatusRequestValidator : AbstractValidator<UpdateGoalStatusRequest>
{
    public UpdateGoalStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", DevelopmentGoalStatus.ValidStatuses)}");

        RuleFor(x => x.CompletionDate)
            .NotNull()
            .WithMessage("Completion date is required when marking goal as Completed")
            .When(x => x.Status == DevelopmentGoalStatus.Completed);

        RuleFor(x => x.CompletionDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Completion date cannot be in the future")
            .When(x => x.CompletionDate.HasValue);

        RuleFor(x => x.ProgressNotes)
            .MaximumLength(4000)
            .WithMessage("Progress notes cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.ProgressNotes));

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for optimistic concurrency control");
    }

    private bool BeValidStatus(string status)
    {
        return DevelopmentGoalStatus.IsValid(status);
    }
}
