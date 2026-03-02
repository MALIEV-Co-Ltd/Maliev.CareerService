namespace Maliev.CareerService.Domain.Entities;

public class TrainingRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public Guid? TrainingProgramId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DateTime CompletionDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public Guid? CertificateDocumentId { get; set; }

    public TrainingType TrainingType { get; set; }

    public string? Provider { get; set; }

    public TrainingStatus Status { get; set; } = TrainingStatus.Completed;

    public decimal? Score { get; set; }

    public TrainingProgram? TrainingProgram { get; set; }
}

public enum TrainingType
{
    InPerson = 0,
    Online = 1,
    SelfPaced = 2,
    Workshop = 3,
    Certification = 4,
    External = 5
}

public enum TrainingStatus
{
    Completed = 0,
    InProgress = 1,
    NotStarted = 2,
    Expired = 3,
    Failed = 4
}
