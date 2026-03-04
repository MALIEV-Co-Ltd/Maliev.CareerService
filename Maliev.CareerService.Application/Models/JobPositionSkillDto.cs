namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Data transfer object for JobPositionSkill
/// </summary>

public class JobPositionSkillDto
{
    /// <summary>
    /// Gets or sets the skill identifier.
    /// </summary>
    public int SkillId { get; set; }
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    public required string SkillName { get; set; }
    /// <summary>
    /// Gets or sets the skill category.
    /// </summary>
    public string? SkillCategory { get; set; }
    /// <summary>
    /// Gets or sets the required proficiency level.
    /// </summary>
    public required string RequiredLevel { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this skill is required.
    /// </summary>
    public bool IsRequired { get; set; }
}
