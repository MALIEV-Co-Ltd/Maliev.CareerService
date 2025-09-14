using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;

public class CreateJobPositionRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Department { get; set; }

    [Required]
    public required string Description { get; set; }

    public string? Requirements { get; set; }

    public string? Responsibilities { get; set; }

    [Required]
    [MaxLength(50)]
    public required string EmploymentType { get; set; }

    [Required]
    [MaxLength(50)]
    public required string ExperienceLevel { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary range minimum must be positive")]
    public decimal? SalaryRangeMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary range maximum must be positive")]
    public decimal? SalaryRangeMax { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPublic { get; set; } = true;

    public List<int> WorkLocationIds { get; set; } = new();
    public List<CreateJobPositionSkillRequest> Skills { get; set; } = new();

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

public class CreateJobPositionSkillRequest
{
    [Required]
    public int SkillId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string RequiredLevel { get; set; }

    public bool IsRequired { get; set; } = true;
}