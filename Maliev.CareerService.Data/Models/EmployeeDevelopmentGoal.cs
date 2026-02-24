using Maliev.CareerService.Data.Models.Base;

namespace Maliev.CareerService.Data.Models;

/// <summary>
/// Employee development goal within an Individual Development Plan
/// </summary>
public class EmployeeDevelopmentGoal : BaseEntity
{
    /// <summary>
    /// Individual Development Plan ID this goal belongs to
    /// </summary>
    public Guid IdpId { get; set; }

    /// <summary>
    /// Goal title (e.g., "Master Kubernetes Administration")
    /// </summary>
    public string GoalTitle { get; set; } = string.Empty;

    /// <summary>
    /// Detailed goal description
    /// </summary>
    public string GoalDescription { get; set; } = string.Empty;

    /// <summary>
    /// Goal category (Technical, Leadership, SoftSkills, Certification)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Goal status (NotStarted, InProgress, Completed, Deferred)
    /// </summary>
    public string Status { get; set; } = DevelopmentGoalStatus.NotStarted;

    /// <summary>
    /// When the goal was completed
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Action items to achieve this goal (Markdown format)
    /// </summary>
    public string? ActionItems { get; set; }

    /// <summary>
    /// Progress notes and updates (Markdown format)
    /// </summary>
    public string? ProgressNotes { get; set; }

    /// <summary>
    /// Navigation property: Parent Individual Development Plan
    /// </summary>
    public IndividualDevelopmentPlan Idp { get; set; } = null!;
}

/// <summary>
/// Development goal status constants
/// </summary>
public static class DevelopmentGoalStatus
{
    public const string NotStarted = "NotStarted";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Deferred = "Deferred";

    /// <summary>
    /// All valid status values
    /// </summary>
    public static readonly string[] ValidStatuses =
    [
        NotStarted,
        InProgress,
        Completed,
        Deferred
    ];

    /// <summary>
    /// Validates if a status is valid
    /// </summary>
    public static bool IsValid(string status) => ValidStatuses.Contains(status);
}

/// <summary>
/// Development goal category constants
/// </summary>
public static class DevelopmentGoalCategory
{
    public const string Technical = "Technical";
    public const string Leadership = "Leadership";
    public const string SoftSkills = "SoftSkills";
    public const string Certification = "Certification";

    /// <summary>
    /// All valid category values
    /// </summary>
    public static readonly string[] ValidCategories =
    [
        Technical,
        Leadership,
        SoftSkills,
        Certification
    ];

    /// <summary>
    /// Validates if a category is valid
    /// </summary>
    public static bool IsValid(string category) => ValidCategories.Contains(category);
}
