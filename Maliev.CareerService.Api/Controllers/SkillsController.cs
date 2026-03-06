using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Application.Models.Skills;
using Maliev.CareerService.Application.Services;
using Maliev.CareerService.Application.Services.External;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing employee skills matrix (Feature 003)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("career/v{version:apiVersion}/employees/{employeeId:guid}/skills")]
[Produces("application/json")]
public class SkillsController(
    IEmployeeSkillService skillService,
    IEmployeeServiceClient employeeServiceClient,
    ILogger<SkillsController> logger) : ControllerBase
{
    private readonly IEmployeeSkillService _skillService = skillService;
    private readonly IEmployeeServiceClient _employeeServiceClient = employeeServiceClient;
    private readonly ILogger<SkillsController> _logger = logger;

    /// <summary>
    /// Adds a skill to an employee's profile
    /// </summary>
    [HttpPost]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(typeof(EmployeeSkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeSkillDto>> AddSkill(
        Guid employeeId,
        [FromBody] AddSkillRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        try
        {
            var result = await _skillService.AddSkillAsync(
                employeeId,
                request,
                currentUserId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetSkill),
                new { employeeId = employeeId, id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for adding skill: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all skills for an employee
    /// </summary>
    [HttpGet]
    [RequirePermission(CareerPermissions.Trainings.ViewOwn)]
    [ProducesResponseType(typeof(List<EmployeeSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EmployeeSkillDto>>> GetEmployeeSkills(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await HasAccessToEmployeeRecords(employeeId, cancellationToken))
        {
            return Forbid();
        }

        var result = await _skillService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific skill by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetSkill))]
    [RequirePermission(CareerPermissions.Trainings.ViewOwn)]
    [ProducesResponseType(typeof(EmployeeSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeSkillDto>> GetSkill(
        Guid employeeId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await HasAccessToEmployeeRecords(employeeId, cancellationToken))
        {
            return Forbid();
        }

        var result = await _skillService.GetByIdAsync(id, cancellationToken);

        if (result == null || result.EmployeeId != employeeId)
        {
            return NotFound(new { error = "Skill not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing skill record
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(typeof(EmployeeSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeSkillDto>> UpdateSkill(
        Guid employeeId,
        Guid id,
        [FromBody] UpdateEmployeeSkillRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { error = "User ID not found in claims" });
        }

        var result = await _skillService.UpdateAsync(id, request, currentUserId, cancellationToken);

        if (result == null || result.EmployeeId != employeeId)
        {
            return NotFound(new { error = "Skill not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes a skill record
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CareerPermissions.Trainings.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSkill(
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
        var skill = await _skillService.GetByIdAsync(id, cancellationToken);
        if (skill == null || skill.EmployeeId != employeeId)
        {
            return NotFound(new { error = "Skill not found" });
        }

        var deleted = await _skillService.DeleteAsync(id, currentUserId, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { error = "Skill not found" });
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
    /// Extracts authenticated user ID from JWT claims
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
