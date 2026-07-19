using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models;
/// <summary>
/// Request model for createskill
/// </summary>

public class CreateSkillRequest
{
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the skill category (e.g., Technical, Soft Skills).
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the skill is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
