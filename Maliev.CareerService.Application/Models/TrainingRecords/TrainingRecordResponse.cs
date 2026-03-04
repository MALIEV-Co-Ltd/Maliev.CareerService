using Maliev.CareerService.Domain.Entities;

namespace Maliev.CareerService.Application.Models.TrainingRecords;

/// <summary>
/// Response model for a training record (Feature 003)
/// </summary>
public class TrainingRecordResponse
{
    /// <summary>Training record ID</summary>
    public Guid Id { get; set; }

    /// <summary>Employee who completed the training</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Reference to internal training program (optional)</summary>
    public Guid? TrainingProgramId { get; set; }

    /// <summary>Name of the training course</summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>Date the training was completed</summary>
    public DateTime CompletionDate { get; set; }

    /// <summary>Date when the certification expires</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Reference to certificate document</summary>
    public Guid? CertificateDocumentId { get; set; }

    /// <summary>Type/delivery method of the training</summary>
    public TrainingType TrainingType { get; set; }

    /// <summary>Training provider name</summary>
    public string? Provider { get; set; }

    /// <summary>Current status of the training record</summary>
    public TrainingStatus Status { get; set; }

    /// <summary>Training score/grade (0-100)</summary>
    public decimal? Score { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }
}
