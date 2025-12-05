using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data.Models;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for CreateDevelopmentGoalRequest
/// </summary>
public class CreateDevelopmentGoalRequestValidator : AbstractValidator<CreateDevelopmentGoalRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDevelopmentGoalRequestValidator"/> class.
    /// </summary>
    public CreateDevelopmentGoalRequestValidator()
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
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Target date must be in the future");

        RuleFor(x => x.ActionItems)
            .MaximumLength(4000)
            .WithMessage("Action items cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.ActionItems));
    }

    private bool BeValidCategory(string category)
    {
        return DevelopmentGoalCategory.IsValid(category);
    }
}
