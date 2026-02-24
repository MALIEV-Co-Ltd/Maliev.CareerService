using Maliev.CareerService.Api.Models.TrainingPrograms;

namespace Maliev.CareerService.Api.Models.Enrollments;

/// <summary>
/// Response DTO for training enrollment
/// </summary>
public class TrainingEnrollmentResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Training program ID
    /// </summary>
    public Guid TrainingProgramId { get; set; }

    /// <summary>
    /// Training program details
    /// </summary>
    public TrainingProgramResponse? TrainingProgram { get; set; }

    /// <summary>
    /// Employee ID from Employee Service
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// When the employee enrolled
    /// </summary>
    public DateTime EnrolledAt { get; set; }

    /// <summary>
    /// Enrollment type (Voluntary, Mandatory, Assigned)
    /// </summary>
    public string EnrollmentType { get; set; } = string.Empty;

    /// <summary>
    /// Current enrollment status (Enrolled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the employee started the training
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the employee completed the training
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Completion notes from HR staff or system
    /// </summary>
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// HR staff member who marked this as complete
    /// </summary>
    public Guid? MarkedCompleteBy { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token (Base64 encoded)
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}
