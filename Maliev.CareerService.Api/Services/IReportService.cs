using Maliev.CareerService.Api.Models.Reports;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for generating reports and metrics
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates recruitment metrics for the specified date range
    /// </summary>
    /// <param name="startDate">Start date for metrics calculation (optional)</param>
    /// <param name="endDate">End date for metrics calculation (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recruitment metrics including applications, conversion rates, and time-to-fill</returns>
    Task<RecruitmentMetricsResponse> GenerateRecruitmentMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates learning and development metrics for the specified date range
    /// </summary>
    /// <param name="startDate">Start date for metrics calculation (optional)</param>
    /// <param name="endDate">End date for metrics calculation (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Learning metrics including course enrollments, completion rates, and satisfaction scores</returns>
    Task<LearningMetricsResponse> GenerateLearningMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates HR operational metrics for the specified date range
    /// </summary>
    /// <param name="startDate">Start date for metrics calculation (optional)</param>
    /// <param name="endDate">End date for metrics calculation (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HR operational metrics including active postings, interview ratios, and review times</returns>
    Task<HROperationalMetricsResponse> GenerateHROperationalMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates organization-wide training compliance report (Feature 003)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Training compliance report with department breakdown and overdue list</returns>
    Task<TrainingComplianceReportDto> GenerateTrainingComplianceReportAsync(
        CancellationToken cancellationToken = default);
}
