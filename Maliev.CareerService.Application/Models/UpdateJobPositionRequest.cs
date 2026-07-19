using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Request model for updatejobposition
/// </summary>

public class UpdateJobPositionRequest
{
    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Department { get; set; }

    /// <summary>
    /// Gets or sets the job description.
    /// </summary>
    [Required]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the job requirements.
    /// </summary>
    public string? Requirements { get; set; }

    /// <summary>
    /// Gets or sets the job responsibilities.
    /// </summary>
    public string? Responsibilities { get; set; }

    /// <summary>
    /// Gets or sets the employment type.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string EmploymentType { get; set; }

    /// <summary>
    /// Gets or sets the required experience level.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string ExperienceLevel { get; set; }

    /// <summary>
    /// Gets or sets the minimum salary range.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Salary range minimum must be positive")]
    public decimal? SalaryRangeMin { get; set; }

    /// <summary>
    /// Gets or sets the maximum salary range.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Salary range maximum must be positive")]
    public decimal? SalaryRangeMax { get; set; }

    /// <summary>
    /// Gets or sets the currency code for salary.
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the position is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the position is publicly visible.
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of work location IDs.
    /// </summary>
    public List<int> WorkLocationIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of required skills.
    /// </summary>
    public List<CreateJobPositionSkillRequest> Skills { get; set; } = [];

    /// <summary>
    /// Validates the update job position request.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryRangeMin.HasValue && SalaryRangeMax.HasValue && SalaryRangeMin > SalaryRangeMax)
        {
            yield return new ValidationResult(
                "Salary range minimum cannot be greater than maximum",
                new[] { nameof(SalaryRangeMin), nameof(SalaryRangeMax) });
        }
    }
}
