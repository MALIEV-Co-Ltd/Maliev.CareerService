using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data.Models;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateDevelopmentGoalRequest
/// </summary>
public class UpdateDevelopmentGoalRequestValidator : AbstractValidator<UpdateDevelopmentGoalRequest>
{
    public UpdateDevelopmentGoalRequestValidator()
    {
        RuleFor(x => x.GoalTitle)
            .NotEmpty()
            .WithMessage("Goal title is required")
            .MaximumLength(200)
            .WithMessage("Goal title cannot exceed 200 characters");

        RuleFor(x => x.GoalDescription)
            .NotEmpty()
            .WithMessage("Goal description is required")
            .MaximumLength(2000)
            .WithMessage("Goal description cannot exceed 2000 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .Must(BeValidCategory)
            .WithMessage($"Category must be one of: {string.Join(", ", DevelopmentGoalCategory.ValidCategories)}");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.AddDays(-1))
            .WithMessage("Target date cannot be in the past (allow same-day updates)");

        RuleFor(x => x.ActionItems)
            .MaximumLength(4000)
            .WithMessage("Action items cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.ActionItems));

        RuleFor(x => x.ProgressNotes)
            .MaximumLength(4000)
            .WithMessage("Progress notes cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.ProgressNotes));

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for optimistic concurrency control");
    }

    private bool BeValidCategory(string category)
    {
        return DevelopmentGoalCategory.IsValid(category);
    }
}
