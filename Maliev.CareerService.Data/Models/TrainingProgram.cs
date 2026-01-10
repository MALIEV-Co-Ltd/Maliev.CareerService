using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Training program entity for employee learning and development
/// </summary>
public class TrainingProgram : BaseEntity
{
    /// <summary>
    /// Unique program code (e.g., "LEAD-2025-001")
    /// </summary>
    public string ProgramCode { get; set; } = string.Empty;

    /// <summary>
    /// Training program name
    /// </summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Training program description in Markdown format
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Training category (e.g., "Leadership", "Technical", "Compliance")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Duration in hours
    /// </summary>
    public decimal DurationHours { get; set; }

    /// <summary>
    /// Training provider (e.g., "Internal", "LinkedIn Learning", "Coursera")
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// External LMS URL for accessing the training
    /// </summary>
    public string? ExternalLmsUrl { get; set; }

    /// <summary>
    /// Whether this training is mandatory for target roles
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Target roles for this training (array of role names)
    /// </summary>
    public string[] TargetRoles { get; set; } = [];

    /// <summary>
    /// Maximum number of participants (null = unlimited)
    /// </summary>
    public int? MaxParticipants { get; set; }

    /// <summary>
    /// Whether this training is currently active and available for enrollment
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Number of months until the certification expires after completion (null = never expires)
    /// </summary>
    public int? ValidityMonths { get; set; }

    /// <summary>
    /// Navigation property: Enrollments for this training program
    /// </summary>
    public ICollection<EmployeeTrainingEnrollment> Enrollments { get; set; } = [];
}
