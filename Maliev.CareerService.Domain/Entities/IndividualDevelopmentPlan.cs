namespace Maliev.CareerService.Domain.Entities;

public class IndividualDevelopmentPlan : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public int PlanYear { get; set; }

    public string Status { get; set; } = IDPStatusConstants.Draft;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    public ICollection<EmployeeDevelopmentGoal> Goals { get; set; } = [];
}

public static class IDPStatusConstants
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Approved = "approved";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";

    public static readonly string[] ValidStatuses =
    [
        Draft,
        Submitted,
        Approved,
        InProgress,
        Completed
    ];

    public static readonly string[] EditableStatuses = [Draft];

    public static readonly string[] TerminalStatuses = [Completed];

    public static bool IsValid(string status) => ValidStatuses.Contains(status);
    public static bool IsEditable(string status) => EditableStatuses.Contains(status);
    public static bool IsTerminal(string status) => TerminalStatuses.Contains(status);
}
