namespace Maliev.CareerService.Api.Models.Reports;

/// <summary>
/// Response containing recruitment metrics and analytics
/// </summary>
public class RecruitmentMetricsResponse
{
    /// <summary>
    /// Total number of applications in the date range
    /// </summary>
    public int TotalApplications { get; set; }

    /// <summary>
    /// Applications grouped by job posting
    /// </summary>
    public Dictionary<string, int> ApplicationsPerPosting { get; set; } = [];

    /// <summary>
    /// Conversion rates at each stage (e.g., Submitted to Interview, Interview to Offered)
    /// </summary>
    public Dictionary<string, decimal> ConversionRates { get; set; } = [];

    /// <summary>
    /// Average time to hire in days
    /// </summary>
    public decimal AverageTimeToHire { get; set; }

    /// <summary>
    /// Number of positions filled in the date range
    /// </summary>
    public int PositionsFilled { get; set; }

    /// <summary>
    /// Number of positions still open
    /// </summary>
    public int PositionsOpen { get; set; }

    /// <summary>
    /// Application volume trends over time (date -> count)
    /// </summary>
    public Dictionary<string, int> ApplicationVolumeTrends { get; set; } = [];
}
