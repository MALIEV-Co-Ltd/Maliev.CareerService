using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/positions")]
[EnableRateLimiting("CareerPolicy")]
public class JobPositionController : ControllerBase
{
    private readonly IJobPositionService _jobPositionService;
    private readonly ILogger<JobPositionController> _logger;

    public JobPositionController(
        IJobPositionService jobPositionService,
        ILogger<JobPositionController> logger)
    {
        _jobPositionService = jobPositionService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobPositionDto>> GetJobPosition(int id, CancellationToken cancellationToken = default)
    {
        var position = await _jobPositionService.GetByIdAsync(id, cancellationToken);
        
        if (position == null)
        {
            return NotFound($"Job position with ID {id} not found");
        }

        return Ok(position);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> SearchJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> GetPublicJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.GetPublicPositionsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("departments")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments(CancellationToken cancellationToken = default)
    {
        var departments = await _jobPositionService.GetDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> CreateJobPosition(
        [FromBody] CreateJobPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobPositionService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Job position created with ID {Id} by user", result.Id);
            
            return CreatedAtAction(
                nameof(GetJobPosition), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job position");
            return StatusCode(500, "An error occurred while creating the job position");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> UpdateJobPosition(
        int id,
        [FromBody] UpdateJobPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobPositionService.UpdateAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Job position with ID {id} not found");
            }

            _logger.LogInformation("Job position {Id} updated by user", id);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job position {Id}", id);
            return StatusCode(500, "An error occurred while updating the job position");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteJobPosition(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _jobPositionService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Job position with ID {id} not found");
            }

            _logger.LogInformation("Job position {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job position {Id}", id);
            return StatusCode(500, "An error occurred while deleting the job position");
        }
    }

    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckJobPositionExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _jobPositionService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateJobPosition(
        [FromQuery] string title,
        [FromQuery] string department,
        [FromQuery] int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(department))
        {
            return BadRequest("Title and department are required");
        }

        var exists = await _jobPositionService.ExistsByTitleAndDepartmentAsync(title, department, excludeId, cancellationToken);
        return Ok(!exists); // Return true if valid (doesn't exist)
    }
}