using Maliev.CareerService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Application.Models.Skills;

/// <summary>
/// Request to add a skill to an employee (Feature 003)
/// </summary>
public class AddSkillRequest
{
    /// <summary>
    /// Gets or sets the name of the skill
    /// </summary>
    [Required(ErrorMessage = "Skill name is required")]
    [StringLength(100, ErrorMessage = "Skill name cannot exceed 100 characters")]
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the proficiency level
    /// </summary>
    [Required(ErrorMessage = "Proficiency level is required")]
    [Range(1, 5, ErrorMessage = "Proficiency level must be between 1 and 5")]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this skill is a development area
    /// </summary>
    public bool IsDevelopmentArea { get; set; } = false;

    /// <summary>
    /// Gets or sets additional notes about the skill
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
