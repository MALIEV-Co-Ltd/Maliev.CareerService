using Maliev.CareerService.Data.Enums;

namespace Maliev.CareerService.Api.Models.Skills;

/// <summary>
/// Data transfer object for employee skill (Feature 003)
/// </summary>
public class EmployeeSkillDto
{
    /// <summary>
    /// Gets or sets the skill record identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the skill
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee's proficiency level
    /// </summary>
    public ProficiencyLevel ProficiencyLevel { get; set; }

    /// <summary>
    /// Gets or sets the date when the skill was last assessed
    /// </summary>
    public DateTime LastAssessedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this skill is marked as a development area
    /// </summary>
    public bool IsDevelopmentArea { get; set; }

    /// <summary>
    /// Gets or sets additional notes about the skill
    /// </summary>
    public string? Notes { get; set; }
}