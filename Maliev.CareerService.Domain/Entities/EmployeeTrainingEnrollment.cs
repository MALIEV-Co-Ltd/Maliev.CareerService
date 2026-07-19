namespace Maliev.CareerService.Domain.Entities;

public class EmployeeTrainingEnrollment : BaseEntity
{
    public Guid TrainingProgramId { get; set; }

    public Guid EmployeeId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string EnrollmentType { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = TrainingEnrollmentStatusConstants.Enrolled;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? CompletionNotes { get; set; }

    public Guid? MarkedCompleteBy { get; set; }

    public TrainingProgram TrainingProgram { get; set; } = null!;
}

public static class TrainingEnrollmentStatusConstants
{
    public const string Enrolled = "enrolled";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
    public const string Withdrawn = "withdrawn";
}

public static class EnrollmentTypeConstants
{
    public const string Voluntary = "voluntary";
    public const string Mandatory = "mandatory";
    public const string Assigned = "assigned";
}
