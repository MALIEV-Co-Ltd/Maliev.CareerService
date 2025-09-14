namespace Maliev.CareerService.Api.Models;

public static class ApplicationStatus
{
    public const string Submitted = "Submitted";
    public const string UnderReview = "Under Review";
    public const string Interview = "Interview";
    public const string Rejected = "Rejected";
    public const string Accepted = "Accepted";
    
    public static readonly string[] ValidStatuses = 
    {
        Submitted,
        UnderReview,
        Interview,
        Rejected,
        Accepted
    };
}