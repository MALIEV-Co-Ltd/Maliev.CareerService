namespace Maliev.CareerService.Domain.Entities;

public class EmployeeDevelopmentGoal : BaseEntity
{
    public Guid IdpId { get; set; }

    public string GoalTitle { get; set; } = string.Empty;

    public string GoalDescription { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime TargetDate { get; set; }

    public string Status { get; set; } = DevelopmentGoalStatusConstants.NotStarted;

    public DateTime? CompletionDate { get; set; }

    public string? ActionItems { get; set; }

    public string? ProgressNotes { get; set; }

    public IndividualDevelopmentPlan Idp { get; set; } = null!;
}

public static class DevelopmentGoalStatusConstants
{
    public const string NotStarted = "NotStarted";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Deferred = "Deferred";

    public static readonly string[] ValidStatuses =
    [
        NotStarted,
        InProgress,
        Completed,
        Deferred
    ];

    public static bool IsValid(string status) => ValidStatuses.Contains(status);
}

public static class DevelopmentGoalCategoryConstants
{
    public const string Technical = "Technical";
    public const string Leadership = "Leadership";
    public const string SoftSkills = "SoftSkills";
    public const string Certification = "Certification";

    public static readonly string[] ValidCategories =
    [
        Technical,
        Leadership,
        SoftSkills,
        Certification
    ];

    public static bool IsValid(string category) => ValidCategories.Contains(category);
}
