using System.ComponentModel.DataAnnotations;
using Maliev.CareerService.Data.Enums;

namespace Maliev.CareerService.Api.Models.TrainingRecords;

/// <summary>
/// Request to update an existing training record (Feature 003)
/// </summary>
public class UpdateTrainingRecordRequest
{
    /// <summary>
    /// Name of the training course
    /// </summary>
    [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
    public string? CourseName { get; set; }

    /// <summary>
    /// Date the training was completed
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Date when the certification expires
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Reference to certificate document in Upload Service
    /// </summary>
    public Guid? CertificateDocumentId { get; set; }

    /// <summary>
    /// Type/delivery method of the training
    /// </summary>
    public TrainingType? TrainingType { get; set; }

    /// <summary>
    /// Training provider name
    /// </summary>
    [StringLength(200, ErrorMessage = "Provider cannot exceed 200 characters")]
    public string? Provider { get; set; }

    /// <summary>
    /// Current status of the training record
    /// </summary>
    public TrainingStatus? Status { get; set; }

    /// <summary>
    /// Training score/grade (0-100 if applicable)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
    public decimal? Score { get; set; }
}
