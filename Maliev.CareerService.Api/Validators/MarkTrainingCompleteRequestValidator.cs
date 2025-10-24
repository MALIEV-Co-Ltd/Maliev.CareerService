using FluentValidation;
using Maliev.CareerService.Api.Models.Enrollments;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for MarkTrainingCompleteRequest
/// </summary>
public class MarkTrainingCompleteRequestValidator : AbstractValidator<MarkTrainingCompleteRequest>
{
    public MarkTrainingCompleteRequestValidator()
    {
        RuleFor(x => x.CompletionNotes)
            .MaximumLength(2000).WithMessage("Completion notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CompletionNotes));

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for optimistic concurrency");
    }
}
