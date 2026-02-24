using Maliev.CareerService.Data.Enums;
using Maliev.CareerService.Data.Models.Base;
using Maliev.CareerService.Data.Validation;
using System.ComponentModel.DataAnnotations;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Represents completion of a training course by an employee
/// </summary>
public class TrainingRecord : BaseEntity
{
    /// <summary>
    /// Employee who completed the training
    /// </summary>
    [Required]
    [RequiredGuid]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Reference to internal training program (optional)
    /// </summary>
    public Guid? TrainingProgramId { get; set; }

    /// <summary>
    /// Name of the training course
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Date the training was completed
    /// Must not be in the future (validated at service layer)
    /// </summary>
    [Required]
    public DateTime CompletionDate { get; set; }

    /// <summary>
    /// Date when the certification expires (if applicable)
    /// Must be after CompletionDate (validated at service layer)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Reference to certificate document in Upload Service (if applicable)
    /// </summary>
    public Guid? CertificateDocumentId { get; set; }

    /// <summary>
    /// Type/delivery method of the training
    /// </summary>
    [Required]
    public TrainingType TrainingType { get; set; }

    /// <summary>
    /// Training provider name (for external training)
    /// </summary>
    [MaxLength(200)]
    public string? Provider { get; set; }

    /// <summary>
    /// Current status of the training record
    /// </summary>
    [Required]
    public TrainingStatus Status { get; set; } = TrainingStatus.Completed;

    /// <summary>
    /// Training score/grade (0-100 if applicable)
    /// </summary>
    [Range(0, 100)]
    public decimal? Score { get; set; }

    // Navigation properties
    /// <summary>
    /// Associated training program (if TrainingProgramId is set)
    /// </summary>
    public TrainingProgram? TrainingProgram { get; set; }
}
