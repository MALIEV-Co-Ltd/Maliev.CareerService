using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Models.DevelopmentPlans;
using Maliev.CareerService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing Individual Development Plans
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/idps")]
[Produces("application/json")]
public class DevelopmentPlansController(
    IDevelopmentPlanService developmentPlanService,
    ILogger<DevelopmentPlansController> logger) : ControllerBase
{
    private readonly IDevelopmentPlanService _developmentPlanService = developmentPlanService;
    private readonly ILogger<DevelopmentPlansController> _logger = logger;

    /// <summary>
    /// Gets all IDPs for the authenticated employee
    /// </summary>
    [HttpGet]
    [RequirePermission(CareerPermissions.Development.ViewOwn)]
    [ProducesResponseType(typeof(IDPListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IDPListResponse>> GetIDPs(
        [FromQuery] int? plan_year = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var employeeId = GetAuthenticatedUserId();
        if (employeeId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        var result = await _developmentPlanService.GetEmployeeIDPsAsync(
            employeeId,
            plan_year,
            status,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new IDP for the authenticated employee
    /// </summary>
    [HttpPost]
    [RequirePermission(CareerPermissions.Development.ViewOwn)]
    [ProducesResponseType(typeof(IDPResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IDPResponse>> CreateIDP(
        [FromBody] CreateIDPRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentPlanService.CreateIDPAsync(
                employeeId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetIDP),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate IDP creation attempt");
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific IDP by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(CareerPermissions.Development.ViewOwn)]
    [ProducesResponseType(typeof(IDPResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IDPResponse>> GetIDP(
        Guid id,
        CancellationToken cancellationToken)
    {
        var idp = await _developmentPlanService.GetIDPByIdAsync(id, cancellationToken);

        if (idp == null)
        {
            return NotFound(new { error = $"IDP {id} not found" });
        }

        // Check access: Employees can only view their own IDPs unless HR or Manager (view-team)
        var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
        if (!PermissionMatcher.Match("Permission:career.*", permissions) && !PermissionMatcher.Match($"Permission:{CareerPermissions.Development.ViewTeam}", permissions))
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty || idp.EmployeeId != employeeId)
            {
                return Forbid();
            }
        }

        return Ok(idp);
    }

    /// <summary>
    /// Updates an IDP (only if Status = Draft)
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(CareerPermissions.Development.ViewOwn)]
    [ProducesResponseType(typeof(IDPResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IDPResponse>> UpdateIDP(
        Guid id,
        [FromBody] UpdateIDPRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentPlanService.UpdateIDPAsync(
                id,
                request,
                employeeId,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized IDP update attempt");
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "IDP {IdpId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Cannot update"))
        {
            _logger.LogWarning(ex, "Invalid IDP update attempt");
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating IDP {IdpId}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submits an IDP for approval
    /// </summary>
    [HttpPatch("{id:guid}/submit")]
    [RequirePermission(CareerPermissions.Development.ViewOwn)]
    [ProducesResponseType(typeof(IDPResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IDPResponse>> SubmitIDP(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = GetAuthenticatedUserId();
            if (employeeId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentPlanService.SubmitIDPAsync(
                id,
                employeeId,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized IDP submission attempt");
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "IDP {IdpId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid IDP submission attempt");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approves an IDP (HR Staff only)
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    [RequirePermission(CareerPermissions.Evaluations.Approve)]
    [ProducesResponseType(typeof(IDPResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IDPResponse>> ApproveIDP(
        Guid id,
        [FromBody] ApproveIDPRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var hrUserId = GetAuthenticatedUserId();
            if (hrUserId == Guid.Empty)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var result = await _developmentPlanService.ApproveIDPAsync(
                id,
                request,
                hrUserId,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "IDP {IdpId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid IDP approval attempt");
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict approving IDP {IdpId}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the authenticated user's ID from JWT claims
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
