using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Request model for updateskill
/// </summary>

public class UpdateSkillRequest
{
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the skill category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the skill is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
