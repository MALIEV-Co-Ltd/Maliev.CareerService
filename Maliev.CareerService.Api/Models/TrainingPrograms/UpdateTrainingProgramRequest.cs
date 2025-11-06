using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.TrainingPrograms;

/// <summary>
/// Request to update an existing training program
/// </summary>
public class UpdateTrainingProgramRequest
{
    /// <summary>
    /// Training program name
    /// </summary>
    [Required(ErrorMessage = "Program name is required")]
    [StringLength(200, ErrorMessage = "Program name cannot exceed 200 characters")]
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Training program description in Markdown format
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Training category (e.g., "Leadership", "Technical", "Compliance")
    /// </summary>
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    /// <summary>
    /// Duration in hours
    /// </summary>
    [Required(ErrorMessage = "Duration hours is required")]
    [Range(0.01, 9999.99, ErrorMessage = "Duration hours must be between 0.01 and 9999.99")]
    public decimal DurationHours { get; set; }

    /// <summary>
    /// Training provider (e.g., "Internal", "LinkedIn Learning", "Coursera")
    /// </summary>
    [StringLength(200, ErrorMessage = "Provider cannot exceed 200 characters")]
    public string? Provider { get; set; }

    /// <summary>
    /// External LMS URL for accessing the training
    /// </summary>
    [StringLength(500, ErrorMessage = "External LMS URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "External LMS URL must be a valid URL")]
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
    [Range(1, 10000, ErrorMessage = "Max participants must be between 1 and 10000")]
    public int? MaxParticipants { get; set; }

    /// <summary>
    /// Whether this training is currently active and available for enrollment
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Optimistic concurrency token (Base64 encoded)
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for optimistic concurrency")]
    public string RowVersion { get; set; } = string.Empty;
}
