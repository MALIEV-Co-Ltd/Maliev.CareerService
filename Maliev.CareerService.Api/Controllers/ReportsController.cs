using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Api.Models.Reports;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for generating HR and recruitment reports
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/reports")]
[Produces("application/json")]
[RequirePermission(CareerPermissions.Reports.Read)]
public class ReportsController(
    IReportService reportService,
    ILogger<ReportsController> logger) : ControllerBase
{
    private readonly IReportService _reportService = reportService;
    private readonly ILogger<ReportsController> _logger = logger;

    /// <summary>
    /// Generates recruitment metrics report
    /// </summary>
    /// <param name="start_date">Start date for metrics calculation (optional, defaults to 3 months ago)</param>
    /// <param name="end_date">End date for metrics calculation (optional, defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recruitment metrics including applications, conversion rates, and time-to-fill</returns>
    [HttpGet("recruitment-metrics")]
    [ProducesResponseType(typeof(RecruitmentMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecruitmentMetricsResponse>> GetRecruitmentMetrics(
        [FromQuery] DateTime? start_date = null,
        [FromQuery] DateTime? end_date = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (start_date.HasValue && end_date.HasValue && start_date > end_date)
            {
                return BadRequest(new { error = "start_date cannot be after end_date" });
            }

            var result = await _reportService.GenerateRecruitmentMetricsAsync(
                start_date,
                end_date,
                cancellationToken);

            _logger.LogInformation(
                "Generated recruitment metrics for date range {StartDate} - {EndDate}",
                start_date,
                end_date);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recruitment metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while generating recruitment metrics" });
        }
    }

    /// <summary>
    /// Generates learning and development metrics report
    /// </summary>
    /// <param name="start_date">Start date for metrics calculation (optional, defaults to 3 months ago)</param>
    /// <param name="end_date">End date for metrics calculation (optional, defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Learning metrics including course enrollments, completion rates, and satisfaction scores</returns>
    [HttpGet("learning-metrics")]
    [ProducesResponseType(typeof(LearningMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LearningMetricsResponse>> GetLearningMetrics(
        [FromQuery] DateTime? start_date = null,
        [FromQuery] DateTime? end_date = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (start_date.HasValue && end_date.HasValue && start_date > end_date)
            {
                return BadRequest(new { error = "start_date cannot be after end_date" });
            }

            var result = await _reportService.GenerateLearningMetricsAsync(
                start_date,
                end_date,
                cancellationToken);

            _logger.LogInformation(
                "Generated learning metrics for date range {StartDate} - {EndDate}",
                start_date,
                end_date);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while generating learning metrics" });
        }
    }

    /// <summary>
    /// Generates HR operational metrics report
    /// </summary>
    /// <param name="start_date">Start date for metrics calculation (optional, defaults to 3 months ago)</param>
    /// <param name="end_date">End date for metrics calculation (optional, defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HR operational metrics including active postings, interview ratios, and review times</returns>
    [HttpGet("hr-operational-metrics")]
    [ProducesResponseType(typeof(HROperationalMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HROperationalMetricsResponse>> GetHROperationalMetrics(
        [FromQuery] DateTime? start_date = null,
        [FromQuery] DateTime? end_date = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (start_date.HasValue && end_date.HasValue && start_date > end_date)
            {
                return BadRequest(new { error = "start_date cannot be after end_date" });
            }

            var result = await _reportService.GenerateHROperationalMetricsAsync(
                start_date,
                end_date,
                cancellationToken);

            _logger.LogInformation(
                "Generated HR operational metrics for date range {StartDate} - {EndDate}",
                start_date,
                end_date);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HR operational metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while generating HR operational metrics" });
        }
    }

    /// <summary>
    /// Generates organization-wide training compliance report (Feature 003)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Training compliance report with department breakdown and overdue list</returns>
    [HttpGet("training-compliance")]
    [RequirePermission(CareerPermissions.ComplianceReports.View)]
    [ProducesResponseType(typeof(TrainingComplianceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingComplianceReportDto>> GetTrainingComplianceReport(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _reportService.GenerateTrainingComplianceReportAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating training compliance report");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while generating training compliance report" });
        }
    }
}
