using FluentValidation;
using Maliev.CareerService.Api.Models.JobPostings;

namespace Maliev.CareerService.Api.Validators;

/// <summary>
/// Validator for CreateJobPostingRequest
/// </summary>
public class CreateJobPostingRequestValidator : AbstractValidator<CreateJobPostingRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateJobPostingRequestValidator"/> class.
    /// </summary>
    public CreateJobPostingRequestValidator()
    {
        RuleFor(x => x.PositionTitle)
            .NotEmpty().WithMessage("Position title is required")
            .MaximumLength(200).WithMessage("Position title cannot exceed 200 characters");

        RuleFor(x => x.PositionCode)
            .NotEmpty().WithMessage("Position code is required")
            .MaximumLength(50).WithMessage("Position code cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9-]+$").WithMessage("Position code must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.EmploymentType)
            .NotEmpty().WithMessage("Employment type is required")
            .MaximumLength(50).WithMessage("Employment type cannot exceed 50 characters")
            .Must(BeValidEmploymentType).WithMessage("Employment type must be one of: Full-time, Part-time, Contract, Internship");

        RuleFor(x => x.SalaryMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary must be non-negative")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Minimum salary cannot exceed 999999999.99")
            .When(x => x.SalaryMin.HasValue);

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum salary must be non-negative")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Maximum salary cannot exceed 999999999.99")
            .When(x => x.SalaryMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.SalaryMin.HasValue || !x.SalaryMax.HasValue || x.SalaryMin <= x.SalaryMax)
            .WithMessage("Minimum salary cannot exceed maximum salary")
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency code must be exactly 3 characters")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency code must be 3 uppercase letters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(10000).WithMessage("Description cannot exceed 10000 characters");

        RuleFor(x => x.Requirements)
            .NotEmpty().WithMessage("Requirements are required")
            .MaximumLength(10000).WithMessage("Requirements cannot exceed 10000 characters");

        RuleFor(x => x.Responsibilities)
            .NotEmpty().WithMessage("Responsibilities are required")
            .MaximumLength(10000).WithMessage("Responsibilities cannot exceed 10000 characters");

        RuleFor(x => x.ApplicationDeadline)
            .NotEmpty().WithMessage("Application deadline is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Application deadline must be in the future");
    }

    private bool BeValidEmploymentType(string employmentType)
    {
        var validTypes = new[] { "Full-time", "Part-time", "Contract", "Internship" };
        return validTypes.Contains(employmentType, StringComparer.OrdinalIgnoreCase);
    }
}
