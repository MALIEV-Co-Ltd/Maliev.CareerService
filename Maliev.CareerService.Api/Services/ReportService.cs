
using Maliev.CareerService.Api.Models.Reports;
using Maliev.CareerService.Api.Services.External;
using Maliev.CareerService.Infrastructure.Data;
using Maliev.CareerService.Domain.Entities;
using ApplicationStatus = Maliev.CareerService.Domain.Entities.ApplicationStatusConstants;
using TrainingEnrollmentStatus = Maliev.CareerService.Domain.Entities.TrainingEnrollmentStatusConstants;
using EnrollmentType = Maliev.CareerService.Domain.Entities.EnrollmentTypeConstants;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service implementation for generating reports and metrics
/// </summary>
public class ReportService(
    CareerDbContext dbContext,
    IEmployeeServiceClient employeeServiceClient,
    ILogger<ReportService> logger) : IReportService
{
    private readonly CareerDbContext _dbContext = dbContext;
    private readonly IEmployeeServiceClient _employeeServiceClient = employeeServiceClient;
    private readonly ILogger<ReportService> _logger = logger;

    /// <inheritdoc />
    public async Task<RecruitmentMetricsResponse> GenerateRecruitmentMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Default to last 30 days if not specified
        var start = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.AddDays(-30);
        var end = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        // Get applications in date range
        var applicationsQuery = _dbContext.JobApplications
            .Include(a => a.JobPosting)
            .Where(a => a.AppliedAt >= start && a.AppliedAt <= end)
            .AsAsyncEnumerable();

        var applications = new List<JobApplication>();
        await foreach (var app in applicationsQuery.WithCancellation(cancellationToken))
        {
            applications.Add(app);
        }

        var response = new RecruitmentMetricsResponse
        {
            TotalApplications = applications.Count
        };

        // Applications per posting
        response.ApplicationsPerPosting = applications
            .GroupBy(a => a.JobPosting.PositionTitle)
            .ToDictionary(g => g.Key, g => g.Count());

        // Conversion rates
        var totalApps = applications.Count;
        if (totalApps > 0)
        {
            var interviewingCount = applications.Count(a => a.Status == ApplicationStatus.Interviewing);
            var offeredCount = applications.Count(a => a.Status == ApplicationStatus.Offered);
            var acceptedCount = applications.Count(a => a.Status == ApplicationStatus.Accepted);

            response.ConversionRates["submitted_to_interviewing"] = totalApps > 0 ? (decimal)interviewingCount / totalApps * 100 : 0;
            response.ConversionRates["interviewing_to_offered"] = interviewingCount > 0 ? (decimal)offeredCount / interviewingCount * 100 : 0;
            response.ConversionRates["offered_to_accepted"] = offeredCount > 0 ? (decimal)acceptedCount / offeredCount * 100 : 0;
        }

        // Average time to hire (for accepted applications)
        var acceptedApps = applications.Where(a => a.Status == ApplicationStatus.Accepted).ToList();
        if (acceptedApps.Count > 0)
        {
            var totalDays = acceptedApps.Sum(a => (a.UpdatedAt - a.AppliedAt).TotalDays);
            response.AverageTimeToHire = (decimal)(totalDays / acceptedApps.Count);
        }

        // Positions filled
        response.PositionsFilled = acceptedApps.Count;

        // Positions open
        response.PositionsOpen = await _dbContext.JobPostings
            .CountAsync(p => p.IsActive, cancellationToken);

        // Application volume trends (by date)
        response.ApplicationVolumeTrends = applications
            .GroupBy(a => a.AppliedAt.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        return response;
    }

    /// <inheritdoc />
    public async Task<LearningMetricsResponse> GenerateLearningMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Default to last 30 days if not specified
        var start = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.AddDays(-30);
        var end = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        // Get enrollments in date range
        var enrollmentsQuery = _dbContext.EmployeeTrainingEnrollments
            .Include(e => e.TrainingProgram)
            .Where(e => e.EnrolledAt >= start && e.EnrolledAt <= end)
            .AsAsyncEnumerable();

        var enrollments = new List<EmployeeTrainingEnrollment>();
        await foreach (var enrollment in enrollmentsQuery.WithCancellation(cancellationToken))
        {
            enrollments.Add(enrollment);
        }

        var response = new LearningMetricsResponse();

        // Enrollment rates by category (using program name as category for now)
        var totalEmployees = enrollments.Select(e => e.EmployeeId).Distinct().Count();
        if (totalEmployees > 0)
        {
            response.EnrollmentRates = enrollments
                .GroupBy(e => e.TrainingProgram.ProgramName)
                .ToDictionary(
                    g => g.Key,
                    g => (decimal)g.Select(e => e.EmployeeId).Distinct().Count() / totalEmployees * 100
                );
        }

        // Overall completion rate
        var completedCount = enrollments.Count(e => e.Status == TrainingEnrollmentStatus.Completed);
        response.CompletionRates = enrollments.Count > 0 ? (decimal)completedCount / enrollments.Count * 100 : 0;

        // Average time to complete (for completed enrollments with completion date)
        var completedEnrollments = enrollments
            .Where(e => e.Status == TrainingEnrollmentStatus.Completed && e.CompletedAt.HasValue)
            .ToList();

        if (completedEnrollments.Count > 0)
        {
            var totalDays = completedEnrollments.Sum(e => (e.CompletedAt!.Value - e.EnrolledAt).TotalDays);
            response.TimeToComplete = (decimal)(totalDays / completedEnrollments.Count);
        }

        // Popular programs (top programs by enrollment count)
        response.PopularPrograms = enrollments
            .GroupBy(e => e.TrainingProgram.ProgramName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        // Certification success rate (for now, use completion rate as proxy)
        response.CertificationRates = response.CompletionRates;

        // IDP adoption (placeholder - would need IDP table to calculate properly)
        response.IDPAdoption = 0;

        return response;
    }

    /// <inheritdoc />
    public async Task<HROperationalMetricsResponse> GenerateHROperationalMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Pre-existing implementation
        return await Task.FromResult(new HROperationalMetricsResponse());
    }

    /// <inheritdoc />
    public async Task<TrainingComplianceReportDto> GenerateTrainingComplianceReportAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating training compliance report");

        // 1. Get all mandatory requirements
        var requirementsQuery = _dbContext.MandatoryTrainingRequirements
            .Include(r => r.TrainingProgram)
            .Where(r => r.IsActive && !r.IsDeleted)
            .AsAsyncEnumerable();

        var requirements = new List<MandatoryTrainingRequirement>();
        await foreach (var req in requirementsQuery.WithCancellation(cancellationToken))
        {
            requirements.Add(req);
        }

        // 2. Get all mandatory enrollments
        var enrollmentsQuery = _dbContext.EmployeeTrainingEnrollments
            .Include(e => e.TrainingProgram)
            .Where(e => e.EnrollmentType == EnrollmentType.Mandatory && !e.IsDeleted)
            .AsAsyncEnumerable();

        var enrollments = new List<EmployeeTrainingEnrollment>();
        await foreach (var enrollment in enrollmentsQuery.WithCancellation(cancellationToken))
        {
            enrollments.Add(enrollment);
        }

        // 3. Process data to identify compliance per employee
        var employeeIds = enrollments.Select(e => e.EmployeeId).Distinct().ToList();
        var report = new TrainingComplianceReportDto
        {
            TotalEmployees = employeeIds.Count
        };

        foreach (var empId in employeeIds)
        {
            var empEnrollments = enrollments.Where(e => e.EmployeeId == empId).ToList();
            var totalMandatory = empEnrollments.Count;
            var completedCount = empEnrollments.Count(e => e.Status == TrainingEnrollmentStatus.Completed);

            if (completedCount == totalMandatory)
            {
                report.FullyCompliantEmployees++;
            }
            else if (completedCount > 0)
            {
                report.PartiallyCompliantEmployees++;
            }
            else
            {
                report.NonCompliantEmployees++;
            }

            // Add to overdue list if any are overdue
            var now = DateTime.UtcNow;
            var overdue = empEnrollments.Where(e =>
                e.Status != TrainingEnrollmentStatus.Completed &&
                e.DueDate.HasValue &&
                e.DueDate.Value < now).ToList();

            foreach (var o in overdue)
            {
                report.OverdueTrainings.Add(new OverdueTrainingDto
                {
                    EmployeeId = empId,
                    EmployeeName = "Employee " + empId.ToString()[..8], // Placeholder until name resolved
                    TrainingName = o.TrainingProgram.ProgramName,
                    DueDate = o.DueDate!.Value,
                    DaysOverdue = (now - o.DueDate.Value).Days
                });
            }
        }

        // Calculate rates
        if (report.TotalEmployees > 0)
        {
            report.OverallComplianceRate = (decimal)report.FullyCompliantEmployees / report.TotalEmployees * 100;
        }

        // 4. Department breakdown (simplification for this task)
        // In a real implementation, we would group by DepartmentId from enrollments or fetch from Employee Service

        return report;
    }
}
