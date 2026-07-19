namespace Maliev.CareerService.Application.Models.Reports;

/// <summary>
/// Response containing learning and training metrics
/// </summary>
public class LearningMetricsResponse
{
    /// <summary>
    /// Enrollment rates by program category (percentage)
    /// </summary>
    public Dictionary<string, decimal> EnrollmentRates { get; set; } = [];

    /// <summary>
    /// Overall completion rate (percentage)
    /// </summary>
    public decimal CompletionRates { get; set; }

    /// <summary>
    /// Average time to complete training programs in days
    /// </summary>
    public decimal TimeToComplete { get; set; }

    /// <summary>
    /// List of most popular training programs (program name -> enrollment count)
    /// </summary>
    public Dictionary<string, int> PopularPrograms { get; set; } = [];

    /// <summary>
    /// Certification success rate (percentage)
    /// </summary>
    public decimal CertificationRates { get; set; }

    /// <summary>
    /// Individual Development Plan (IDP) adoption rate (percentage)
    /// </summary>
    public decimal IDPAdoption { get; set; }
}
