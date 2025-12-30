using System.ComponentModel.DataAnnotations;
using Maliev.CareerService.Data.Enums;
using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Represents an employee's skill and proficiency level
/// </summary>
public class Skill : BaseEntity
{
    /// <summary>
    /// Employee who possesses this skill
    /// </summary>
    [Required]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Name of the skill
    /// Must be unique per employee (enforced by unique constraint)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's proficiency level (1-5 scale)
    /// </summary>
    [Required]
    [Range(1, 5)]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    /// <summary>
    /// Date when proficiency was last assessed
    /// Automatically updated when ProficiencyLevel changes
    /// </summary>
    [Required]
    public DateTime LastAssessedDate { get; set; }

    /// <summary>
    /// Flag indicating if this skill is marked as a development area for IDP integration
    /// </summary>
    public bool IsDevelopmentArea { get; set; } = false;

    /// <summary>
    /// Additional notes about the skill (e.g., context, certifications, projects)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
