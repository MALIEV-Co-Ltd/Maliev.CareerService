using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentPlans;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateIDPRequest
/// </summary>
public class UpdateIDPRequestValidator : AbstractValidator<UpdateIDPRequest>
{
    public UpdateIDPRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for optimistic concurrency control");
    }
}
