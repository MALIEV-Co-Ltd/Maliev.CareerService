using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Employee training enrollment entity
/// </summary>
public class EmployeeTrainingEnrollment : BaseEntity
{
    /// <summary>
    /// Training program ID
    /// </summary>
    public Guid TrainingProgramId { get; set; }

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
    public string Status { get; set; } = TrainingEnrollmentStatus.Enrolled;

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
    /// Navigation property: Training program
    /// </summary>
    public TrainingProgram TrainingProgram { get; set; } = null!;
}

/// <summary>
/// Training enrollment status constants
/// </summary>
public static class TrainingEnrollmentStatus
{
    public const string Enrolled = "Enrolled";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Enrollment type constants
/// </summary>
public static class EnrollmentType
{
    public const string Voluntary = "Voluntary";
    public const string Mandatory = "Mandatory";
    public const string Assigned = "Assigned";
}
