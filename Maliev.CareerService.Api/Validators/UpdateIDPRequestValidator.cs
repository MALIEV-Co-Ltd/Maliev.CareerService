using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentPlans;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateIDPRequest
/// </summary>
public class UpdateIDPRequestValidator : AbstractValidator<UpdateIDPRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateIDPRequestValidator"/> class.
    /// </summary>
    public UpdateIDPRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for optimistic concurrency control");
    }
}
