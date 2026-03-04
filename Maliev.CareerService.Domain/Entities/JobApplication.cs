namespace Maliev.CareerService.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid JobPostingId { get; set; }

    public JobPosting JobPosting { get; set; } = null!;

    public string ApplicantFirstName { get; set; } = string.Empty;

    public string ApplicantLastName { get; set; } = string.Empty;

    public string ApplicantEmail { get; set; } = string.Empty;

    public string? ApplicantPhone { get; set; }

    public string? ApplicantCountryCode { get; set; }

    public Guid ResumeFileId { get; set; }

    public string? CoverLetter { get; set; }

    public Guid[] AdditionalFileIds { get; set; } = [];

    public string Status { get; set; } = ApplicationStatusConstants.Submitted;

    public DateTime AppliedAt { get; set; }

    public ICollection<ApplicationStatusChange> StatusChanges { get; set; } = [];
}

public static class ApplicationStatusConstants
{
    public const string Submitted = "submitted";
    public const string UnderReview = "under_review";
    public const string Interviewing = "interviewing";
    public const string Offered = "offered";
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
    public const string Withdrawn = "withdrawn";

    public static readonly string[] ValidStatuses =
    [
        Submitted,
        UnderReview,
        Interviewing,
        Offered,
        Accepted,
        Rejected,
        Withdrawn
    ];

    public static readonly string[] TerminalStatuses =
    [
        Accepted,
        Rejected,
        Withdrawn
    ];

    public static bool IsValid(string status) => ValidStatuses.Contains(status);

    public static bool IsTerminal(string status) => TerminalStatuses.Contains(status);
}
