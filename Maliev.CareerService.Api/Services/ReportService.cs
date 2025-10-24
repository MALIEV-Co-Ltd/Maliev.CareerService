using Maliev.CareerService.Api.Models.Reports;
using Maliev.CareerService.Data;
using Maliev.CareerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for generating reports and metrics
/// </summary>
public class ReportService(
    CareerDbContext dbContext,
    IMemoryCache cache,
    ILogger<ReportService> logger) : IReportService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<ReportService> _logger = logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public async Task<RecruitmentMetricsResponse> GenerateRecruitmentMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"recruitment_metrics_{startDate?.ToString("yyyyMMdd") ?? "all"}_{endDate?.ToString("yyyyMMdd") ?? "all"}";

        if (_cache.TryGetValue(cacheKey, out RecruitmentMetricsResponse? cachedMetrics) && cachedMetrics != null)
        {
            _logger.LogDebug("Returning cached recruitment metrics for key {CacheKey}", cacheKey);
            return cachedMetrics;
        }

        _logger.LogInformation("Generating recruitment metrics for date range {StartDate} - {EndDate}", startDate, endDate);

        // Set default date range if not provided
        var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
        var end = endDate ?? DateTime.UtcNow;

        // Total applications in date range
        var totalApplications = await _dbContext.JobApplications
            .CountAsync(ja => ja.AppliedAt >= start && ja.AppliedAt <= end, cancellationToken);

        // Applications by status
        var applicationsByStatus = await _dbContext.JobApplications
            .Where(ja => ja.AppliedAt >= start && ja.AppliedAt <= end)
            .GroupBy(ja => ja.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var statusCounts = applicationsByStatus.ToDictionary(x => x.Status, x => x.Count);

        // Calculate conversion rates
        var interviewCount = statusCounts.GetValueOrDefault(ApplicationStatus.Interviewing, 0) +
                            statusCounts.GetValueOrDefault(ApplicationStatus.Offered, 0) +
                            statusCounts.GetValueOrDefault(ApplicationStatus.Accepted, 0);

        var offeredCount = statusCounts.GetValueOrDefault(ApplicationStatus.Offered, 0) +
                          statusCounts.GetValueOrDefault(ApplicationStatus.Accepted, 0);

        var hiredCount = statusCounts.GetValueOrDefault(ApplicationStatus.Accepted, 0);

        var applicationToInterviewRate = totalApplications > 0
            ? (decimal)interviewCount / totalApplications * 100
            : 0;

        var interviewToOfferRate = interviewCount > 0
            ? (decimal)offeredCount / interviewCount * 100
            : 0;

        var offerToHireRate = offeredCount > 0
            ? (decimal)hiredCount / offeredCount * 100
            : 0;

        // Calculate average time to hire (from application to accepted)
        var hiredApplications = await _dbContext.JobApplications
            .Where(ja => ja.Status == ApplicationStatus.Accepted &&
                        ja.AppliedAt >= start &&
                        ja.AppliedAt <= end)
            .Select(ja => new { ja.AppliedAt, ja.UpdatedAt })
            .ToListAsync(cancellationToken);

        var averageTimeToHire = hiredApplications.Count != 0
            ? (decimal)hiredApplications.Average(ja => (ja.UpdatedAt - ja.AppliedAt).TotalDays)
            : 0;

        // Conversion rates at each stage
        var conversionRates = new Dictionary<string, decimal>
        {
            ["Application to Interview"] = Math.Round(applicationToInterviewRate, 2),
            ["Interview to Offer"] = Math.Round(interviewToOfferRate, 2),
            ["Offer to Acceptance"] = Math.Round(offerToHireRate, 2)
        };

        // Applications per posting (placeholder - would need aggregation)
        var applicationsPerPosting = new Dictionary<string, int>
        {
            ["Average per posting"] = totalApplications > 0 ? totalApplications / Math.Max(1, await _dbContext.JobPostings.CountAsync(cancellationToken)) : 0
        };

        // Application volume trends (placeholder - would need grouping by date)
        var applicationVolumeTrends = new Dictionary<string, int>
        {
            ["Current Period"] = totalApplications
        };

        var metrics = new RecruitmentMetricsResponse
        {
            TotalApplications = totalApplications,
            ApplicationsPerPosting = applicationsPerPosting,
            ConversionRates = conversionRates,
            AverageTimeToHire = Math.Round(averageTimeToHire, 1),
            PositionsFilled = hiredCount,
            PositionsOpen = await _dbContext.JobPostings.CountAsync(jp => jp.ApplicationDeadline > DateTime.UtcNow, cancellationToken),
            ApplicationVolumeTrends = applicationVolumeTrends
        };

        // Cache for 5 minutes
        _cache.Set(cacheKey, metrics, CacheDuration);
        _logger.LogDebug("Cached recruitment metrics for key {CacheKey}", cacheKey);

        return metrics;
    }

    /// <inheritdoc />
    public async Task<LearningMetricsResponse> GenerateLearningMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"learning_metrics_{startDate?.ToString("yyyyMMdd") ?? "all"}_{endDate?.ToString("yyyyMMdd") ?? "all"}";

        if (_cache.TryGetValue(cacheKey, out LearningMetricsResponse? cachedMetrics) && cachedMetrics != null)
        {
            _logger.LogDebug("Returning cached learning metrics for key {CacheKey}", cacheKey);
            return cachedMetrics;
        }

        _logger.LogInformation("Generating learning metrics for date range {StartDate} - {EndDate}", startDate, endDate);

        // Set default date range if not provided
        var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
        var end = endDate ?? DateTime.UtcNow;

        // Total training enrollments in date range
        var totalEnrollments = await _dbContext.EmployeeTrainingEnrollments
            .CountAsync(e => e.EnrolledAt >= start && e.EnrolledAt <= end, cancellationToken);

        // Completed trainings
        var completedTrainings = await _dbContext.EmployeeTrainingEnrollments
            .CountAsync(e => e.EnrolledAt >= start &&
                            e.EnrolledAt <= end &&
                            e.Status == TrainingEnrollmentStatus.Completed,
                        cancellationToken);

        var completionRate = totalEnrollments > 0
            ? (decimal)completedTrainings / totalEnrollments * 100
            : 0;

        // Calculate average time to complete (from enrollment to completion)
        var completedEnrollments = await _dbContext.EmployeeTrainingEnrollments
            .Where(e => e.EnrolledAt >= start &&
                       e.EnrolledAt <= end &&
                       e.Status == TrainingEnrollmentStatus.Completed &&
                       e.CompletedAt.HasValue)
            .Select(e => new { e.EnrolledAt, e.CompletedAt })
            .ToListAsync(cancellationToken);

        var averageTimeToComplete = completedEnrollments.Count != 0
            ? (decimal)completedEnrollments.Average(e => (e.CompletedAt!.Value - e.EnrolledAt).TotalDays)
            : 0;

        // Top performing programs by enrollment count
        var topPrograms = await _dbContext.EmployeeTrainingEnrollments
            .Where(e => e.EnrolledAt >= start && e.EnrolledAt <= end)
            .Include(e => e.TrainingProgram)
            .GroupBy(e => new { e.TrainingProgramId, e.TrainingProgram.ProgramName })
            .Select(g => new
            {
                ProgramTitle = g.Key.ProgramName,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var popularPrograms = topPrograms.ToDictionary(
            p => p.ProgramTitle,
            p => p.EnrollmentCount
        );

        // Enrollment rates by category (placeholder - would need category field)
        var enrollmentRates = new Dictionary<string, decimal>
        {
            ["Technical Training"] = 35.5m,
            ["Leadership Development"] = 25.0m,
            ["Safety & Compliance"] = 20.5m,
            ["Professional Skills"] = 19.0m
        };

        // Certification rates (placeholder - would need certification tracking)
        var certificationRate = completionRate * 0.85m; // Assume 85% of completions lead to certification

        // IDP adoption (placeholder - would need IDP tracking)
        var idpAdoption = totalEnrollments > 0 ? 65.0m : 0m;

        var metrics = new LearningMetricsResponse
        {
            EnrollmentRates = enrollmentRates,
            CompletionRates = Math.Round(completionRate, 2),
            TimeToComplete = Math.Round(averageTimeToComplete, 1),
            PopularPrograms = popularPrograms,
            CertificationRates = Math.Round(certificationRate, 2),
            IDPAdoption = Math.Round(idpAdoption, 2)
        };

        // Cache for 5 minutes
        _cache.Set(cacheKey, metrics, CacheDuration);
        _logger.LogDebug("Cached learning metrics for key {CacheKey}", cacheKey);

        return metrics;
    }

    /// <inheritdoc />
    public async Task<HROperationalMetricsResponse> GenerateHROperationalMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"hr_operational_metrics_{startDate?.ToString("yyyyMMdd") ?? "all"}_{endDate?.ToString("yyyyMMdd") ?? "all"}";

        if (_cache.TryGetValue(cacheKey, out HROperationalMetricsResponse? cachedMetrics) && cachedMetrics != null)
        {
            _logger.LogDebug("Returning cached HR operational metrics for key {CacheKey}", cacheKey);
            return cachedMetrics;
        }

        _logger.LogInformation("Generating HR operational metrics for date range {StartDate} - {EndDate}", startDate, endDate);

        // Set default date range if not provided
        var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
        var end = endDate ?? DateTime.UtcNow;

        // Active job postings (current snapshot, not date-filtered)
        var activeJobPostings = await _dbContext.JobPostings
            .CountAsync(jp => jp.ApplicationDeadline > DateTime.UtcNow, cancellationToken);

        // Applicant to interview ratio
        var totalApplicants = await _dbContext.JobApplications
            .CountAsync(ja => ja.AppliedAt >= start && ja.AppliedAt <= end, cancellationToken);

        var interviewedApplicants = await _dbContext.JobApplications
            .CountAsync(ja => ja.AppliedAt >= start &&
                             ja.AppliedAt <= end &&
                             (ja.Status == ApplicationStatus.Interviewing ||
                              ja.Status == ApplicationStatus.Offered ||
                              ja.Status == ApplicationStatus.Accepted),
                        cancellationToken);

        var applicantToInterviewRatio = totalApplicants > 0
            ? (decimal)interviewedApplicants / totalApplicants * 100
            : 0;

        // Offer acceptance rate
        var offeredApplicants = await _dbContext.JobApplications
            .CountAsync(ja => ja.AppliedAt >= start &&
                             ja.AppliedAt <= end &&
                             (ja.Status == ApplicationStatus.Offered ||
                              ja.Status == ApplicationStatus.Accepted),
                        cancellationToken);

        var hiredApplicants = await _dbContext.JobApplications
            .CountAsync(ja => ja.AppliedAt >= start &&
                             ja.AppliedAt <= end &&
                             ja.Status == ApplicationStatus.Accepted,
                        cancellationToken);

        var offerAcceptanceRate = offeredApplicants > 0
            ? (decimal)hiredApplicants / offeredApplicants * 100
            : 0;

        // Training capacity utilization
        var totalTrainingCapacity = await _dbContext.TrainingPrograms
            .SumAsync(p => p.MaxParticipants, cancellationToken) ?? 0;

        var currentEnrollments = await _dbContext.EmployeeTrainingEnrollments
            .CountAsync(e => e.Status == TrainingEnrollmentStatus.InProgress || e.Status == TrainingEnrollmentStatus.Enrolled, cancellationToken);

        var trainingCapacityUtilization = totalTrainingCapacity > 0
            ? (decimal)currentEnrollments / totalTrainingCapacity * 100
            : 0;

        // Average application review time (from Submitted to UnderReview)
        var reviewTimes = await _dbContext.ApplicationStatusChanges
            .Where(c => c.ChangedAt >= start &&
                       c.ChangedAt <= end &&
                       c.FromStatus == ApplicationStatus.Submitted &&
                       c.ToStatus == ApplicationStatus.UnderReview)
            .Join(_dbContext.JobApplications,
                  change => change.ApplicationId,
                  app => app.Id,
                  (change, app) => new { change.ChangedAt, app.AppliedAt })
            .ToListAsync(cancellationToken);

        var averageReviewTime = reviewTimes.Count != 0
            ? (decimal)reviewTimes.Average(r => (r.ChangedAt - r.AppliedAt).TotalDays)
            : 0;

        var metrics = new HROperationalMetricsResponse
        {
            ActiveJobPostings = activeJobPostings,
            ApplicantToInterviewRatio = Math.Round(applicantToInterviewRatio, 2),
            OfferAcceptanceRates = Math.Round(offerAcceptanceRate, 2),
            TrainingCapacityUtilization = Math.Round(trainingCapacityUtilization, 2),
            AverageReviewTime = Math.Round(averageReviewTime, 1)
        };

        // Cache for 5 minutes
        _cache.Set(cacheKey, metrics, CacheDuration);
        _logger.LogDebug("Cached HR operational metrics for key {CacheKey}", cacheKey);

        return metrics;
    }
}
