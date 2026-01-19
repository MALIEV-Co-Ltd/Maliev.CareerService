using Maliev.CareerService.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.Skills;

/// <summary>
/// Request to update an employee's skill (Feature 003)
/// </summary>
public class UpdateEmployeeSkillRequest
{
    /// <summary>
    /// Gets or sets the updated proficiency level
    /// </summary>
    [Required(ErrorMessage = "Proficiency level is required")]
    [Range(1, 5, ErrorMessage = "Proficiency level must be between 1 and 5")]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this skill is a development area
    /// </summary>
    public bool IsDevelopmentArea { get; set; }

    /// <summary>
    /// Gets or sets additional notes about the skill
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
