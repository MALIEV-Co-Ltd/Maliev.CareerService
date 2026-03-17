using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Models.TrainingRecords;
using Maliev.CareerService.Application.Services;
using Maliev.CareerService.Application.Services.External;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing employee training records (Feature 003)
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("career/v{version:apiVersion}/employees/{employeeId:guid}/training-records")]
[Produces("application/json")]
public class TrainingRecordsController(
    ITrainingRecordService trainingRecordService,
    IEmployeeServiceClient employeeServiceClient,
    ILogger<TrainingRecordsController> logger) : ControllerBase
{
    private readonly ITrainingRecordService _trainingRecordService = trainingRecordService;
    private readonly IEmployeeServiceClient _employeeServiceClient = employeeServiceClient;
    private readonly ILogger<TrainingRecordsController> _logger = logger;

    /// <summary>
    /// Records a training completion for an employee
    /// </summary>
    [HttpPost]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(typeof(TrainingRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingRecordResponse>> RecordTrainingCompletion(
        Guid employeeId,
        [FromBody] RecordTrainingCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        try
        {
            var result = await _trainingRecordService.RecordCompletionAsync(
                employeeId,
                request,
                currentUserId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetTrainingRecord),
                new { employeeId = employeeId, id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for training completion: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all training records for an employee
    /// </summary>
    [HttpGet]
    [RequirePermission(CareerPermissions.Trainings.ViewOwn)]
    [ProducesResponseType(typeof(TrainingRecordListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingRecordListResponse>> GetTrainingRecords(
        Guid employeeId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToEmployeeRecords(employeeId, cancellationToken))
        {
            return Forbid();
        }

        if (limit <= 0 || limit > 100)
        {
            return BadRequest(new { error = "Limit must be between 1 and 100" });
        }

        var pageNumber = (offset / limit) + 1;
        var pageSize = limit;

        var result = await _trainingRecordService.GetByEmployeeIdAsync(
            employeeId,
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific training record by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetTrainingRecord))]
    [RequirePermission(CareerPermissions.Trainings.ViewOwn)]
    [ProducesResponseType(typeof(TrainingRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingRecordResponse>> GetTrainingRecord(
        Guid employeeId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await HasAccessToEmployeeRecords(employeeId, cancellationToken))
        {
            return Forbid();
        }

        var record = await _trainingRecordService.GetByIdAsync(id, cancellationToken);

        if (record == null || record.EmployeeId != employeeId)
        {
            return NotFound(new { error = "Training record not found" });
        }

        return Ok(record);
    }

    /// <summary>
    /// Updates an existing training record
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(typeof(TrainingRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingRecordResponse>> UpdateTrainingRecord(
        Guid employeeId,
        Guid id,
        [FromBody] UpdateTrainingRecordRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        try
        {
            var result = await _trainingRecordService.UpdateAsync(id, request, currentUserId, cancellationToken);

            if (result == null || result.EmployeeId != employeeId)
            {
                return NotFound(new { error = "Training record not found" });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for training update: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a training record (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTrainingRecord(
        Guid employeeId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        // Verify record exists and belongs to employee
        var record = await _trainingRecordService.GetByIdAsync(id, cancellationToken);
        if (record == null || record.EmployeeId != employeeId)
        {
            return NotFound(new { error = "Training record not found" });
        }

        var deleted = await _trainingRecordService.DeleteAsync(id, currentUserId, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { error = "Training record not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Checks if the current user has access to view an employee's records
    /// </summary>
    private async Task<bool> HasAccessToEmployeeRecords(Guid employeeId, CancellationToken cancellationToken)
    {
        var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
        var currentUserId = GetAuthenticatedUserId();

        // HR Admins can see all
        if (PermissionMatcher.Match("Permission:career.*", permissions) ||
            PermissionMatcher.Match($"Permission:{CareerPermissions.Trainings.Manage}", permissions))
        {
            return true;
        }

        // Employees can see their own
        if (employeeId == currentUserId &&
            PermissionMatcher.Match($"Permission:{CareerPermissions.Trainings.ViewOwn}", permissions))
        {
            return true;
        }

        // Managers can see their team
        if (PermissionMatcher.Match($"Permission:{CareerPermissions.Trainings.ViewTeam}", permissions))
        {
            try
            {
                var employee = await _employeeServiceClient.GetEmployeeAsync(employeeId, cancellationToken);
                return employee?.ManagerId == currentUserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying manager relationship for Employee {EmployeeId}", employeeId);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all expiring training records (for HR monitoring)
    /// </summary>
    [HttpGet("~/career/v{version:apiVersion}/training-records/expiring")]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(typeof(TrainingRecordListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingRecordListResponse>> GetExpiringTrainingRecords(
        [FromQuery] int days = 90,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            return BadRequest(new { error = "Days must be positive" });
        }

        if (limit <= 0 || limit > 100)
        {
            return BadRequest(new { error = "Limit must be between 1 and 100" });
        }

        var pageNumber = (offset / limit) + 1;
        var pageSize = limit;

        var result = await _trainingRecordService.GetExpiringAsync(
            days,
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Extracts authenticated user ID from JWT claims
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
