using FluentValidation;
using Maliev.CareerService.Api.Models.TrainingPrograms;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for UpdateTrainingProgramRequest
/// </summary>
public class UpdateTrainingProgramRequestValidator : AbstractValidator<UpdateTrainingProgramRequest>
{
    public UpdateTrainingProgramRequestValidator()
    {
        RuleFor(x => x.ProgramName)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(200).WithMessage("Program name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Duration hours must be greater than 0")
            .LessThanOrEqualTo(9999.99m).WithMessage("Duration hours cannot exceed 9999.99");

        RuleFor(x => x.Provider)
            .MaximumLength(200).WithMessage("Provider cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Provider));

        RuleFor(x => x.ExternalLmsUrl)
            .MaximumLength(500).WithMessage("External LMS URL cannot exceed 500 characters")
            .Must(BeValidUrl).WithMessage("External LMS URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ExternalLmsUrl));

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("Max participants must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Max participants cannot exceed 10000")
            .When(x => x.MaxParticipants.HasValue);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for optimistic concurrency");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
