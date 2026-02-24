using Maliev.CareerService.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Api.Models.TrainingRecords;

/// <summary>
/// Request to record a training completion (Feature 003)
/// </summary>
public class RecordTrainingCompletionRequest
{
    /// <summary>
    /// Reference to internal training program (optional)
    /// </summary>
    public Guid? TrainingProgramId { get; set; }

    /// <summary>
    /// Name of the training course
    /// </summary>
    [Required(ErrorMessage = "Course name is required")]
    [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Date the training was completed
    /// </summary>
    [Required(ErrorMessage = "Completion date is required")]
    public DateTime CompletionDate { get; set; }

    /// <summary>
    /// Date when the certification expires (if applicable)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Reference to certificate document in Upload Service
    /// </summary>
    public Guid? CertificateDocumentId { get; set; }

    /// <summary>
    /// Type/delivery method of the training
    /// </summary>
    [Required(ErrorMessage = "Training type is required")]
    public TrainingType TrainingType { get; set; }

    /// <summary>
    /// Training provider name (for external training)
    /// </summary>
    [StringLength(200, ErrorMessage = "Provider cannot exceed 200 characters")]
    public string? Provider { get; set; }

    /// <summary>
    /// Training score/grade (0-100 if applicable)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
    public decimal? Score { get; set; }
}
