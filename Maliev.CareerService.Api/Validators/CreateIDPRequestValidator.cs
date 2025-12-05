using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentPlans;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for CreateIDPRequest
/// </summary>
public class CreateIDPRequestValidator : AbstractValidator<CreateIDPRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateIDPRequestValidator"/> class.
    /// </summary>
    public CreateIDPRequestValidator()
    {
        RuleFor(x => x.PlanYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year - 1)
            .WithMessage("Plan year must be current year or previous year (allow updates to previous year plans)")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 5)
            .WithMessage("Plan year cannot be more than 5 years in the future");
    }
}
