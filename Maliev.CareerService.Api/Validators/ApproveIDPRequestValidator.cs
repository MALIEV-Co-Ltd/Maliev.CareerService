using FluentValidation;
using Maliev.CareerService.Api.Models.DevelopmentPlans;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for ApproveIDPRequest
/// </summary>
public class ApproveIDPRequestValidator : AbstractValidator<ApproveIDPRequest>
{
    public ApproveIDPRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for optimistic concurrency control");

        RuleFor(x => x.ApprovalNotes)
            .MaximumLength(1000)
            .WithMessage("Approval notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ApprovalNotes));
    }
}
