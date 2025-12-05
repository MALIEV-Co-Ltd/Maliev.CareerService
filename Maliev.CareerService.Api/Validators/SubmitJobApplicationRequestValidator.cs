using FluentValidation;
using Maliev.CareerService.Api.Models.Applications;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for SubmitJobApplicationRequest
/// </summary>
public class SubmitJobApplicationRequestValidator : AbstractValidator<SubmitJobApplicationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitJobApplicationRequestValidator"/> class.
    /// </summary>
    public SubmitJobApplicationRequestValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty().WithMessage("Job posting ID is required");

        RuleFor(x => x.ApplicantFirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.ApplicantLastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.ApplicantEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.ApplicantPhone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone format")
            .When(x => !string.IsNullOrEmpty(x.ApplicantPhone));

        RuleFor(x => x.ApplicantCountryCode)
            .Length(2).WithMessage("Country code must be exactly 2 characters")
            .Matches(@"^[A-Z]{2}$").WithMessage("Country code must be 2 uppercase letters")
            .When(x => !string.IsNullOrEmpty(x.ApplicantCountryCode));

        RuleFor(x => x.ResumeFileId)
            .NotEmpty().WithMessage("Resume file is required");

        RuleFor(x => x.CoverLetter)
            .MaximumLength(5000).WithMessage("Cover letter cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.CoverLetter));

        RuleFor(x => x.AdditionalFileIds)
            .Must(x => x.Length <= 4).WithMessage("Maximum 4 additional files allowed")
            .Must(x => x.Distinct().Count() == x.Length).WithMessage("Duplicate file IDs are not allowed");

        RuleFor(x => x)
            .Must(x => x.AdditionalFileIds.Length + 1 <= 5)
            .WithMessage("Total number of files (resume + additional) cannot exceed 5");
    }
}
