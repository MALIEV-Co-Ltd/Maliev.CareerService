using FluentValidation;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Data.Models;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateApplicationStatusRequest
/// </summary>
public class UpdateApplicationStatusRequestValidator : AbstractValidator<UpdateApplicationStatusRequest>
{
    private static readonly string[] ValidStatuses =
    [
        ApplicationStatus.Submitted,
        ApplicationStatus.UnderReview,
        ApplicationStatus.Interviewing,
        ApplicationStatus.Offered,
        ApplicationStatus.Accepted,
        ApplicationStatus.Rejected,
        ApplicationStatus.Withdrawn
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateApplicationStatusRequestValidator"/> class.
    /// </summary>
    public UpdateApplicationStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("New status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .Must(BeValidStatus).WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required");
    }

    private bool BeValidStatus(string status)
    {
        return ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
