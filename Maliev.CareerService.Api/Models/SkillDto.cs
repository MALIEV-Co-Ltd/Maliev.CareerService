namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Data transfer object for Skill
/// </summary>

public class SkillDto
{
    /// <summary>
    /// Gets or sets the skill identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the skill category.
    /// </summary>
    public string? Category { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this skill is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the record creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// Gets or sets the last modification date.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
