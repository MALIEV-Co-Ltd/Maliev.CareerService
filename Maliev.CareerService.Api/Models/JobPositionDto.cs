namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Data transfer object for JobPosition
/// </summary>

public class JobPositionDto
{
    /// <summary>
    /// Gets or sets the job position identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public required string Department { get; set; }
    /// <summary>
    /// Gets or sets the job description.
    /// </summary>
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
    public required string EmploymentType { get; set; }
    /// <summary>
    /// Gets or sets the experience level required.
    /// </summary>
    public required string ExperienceLevel { get; set; }
    /// <summary>
    /// Gets or sets the minimum salary range.
    /// </summary>
    public decimal? SalaryRangeMin { get; set; }
    /// <summary>
    /// Gets or sets the maximum salary range.
    /// </summary>
    public decimal? SalaryRangeMax { get; set; }
    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string? Currency { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this position is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this position is publicly visible.
    /// </summary>
    public bool IsPublic { get; set; }
    /// <summary>
    /// Gets or sets the record creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// Gets or sets the last modification date.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the list of work locations for this position.
    /// </summary>
    public List<WorkLocationDto> WorkLocations { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of required skills for this position.
    /// </summary>
    public List<JobPositionSkillDto> Skills { get; set; } = [];
    /// <summary>
    /// Gets or sets the number of applications for this position.
    /// </summary>
    public int ApplicationCount { get; set; }
}
