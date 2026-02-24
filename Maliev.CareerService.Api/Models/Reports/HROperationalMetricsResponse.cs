namespace Maliev.CareerService.Api.Models.Reports;

/// <summary>
/// Response containing HR operational metrics
/// </summary>
public class HROperationalMetricsResponse
{
    /// <summary>
    /// Number of currently active job postings
    /// </summary>
    public int ActiveJobPostings { get; set; }

    /// <summary>
    /// Ratio of applicants to interviews (percentage)
    /// </summary>
    public decimal ApplicantToInterviewRatio { get; set; }

    /// <summary>
    /// Offer acceptance rate (percentage)
    /// </summary>
    public decimal OfferAcceptanceRates { get; set; }

    /// <summary>
    /// Training capacity utilization (percentage)
    /// </summary>
    public decimal TrainingCapacityUtilization { get; set; }

    /// <summary>
    /// Average application review time in days
    /// </summary>
    public decimal AverageReviewTime { get; set; }
}
