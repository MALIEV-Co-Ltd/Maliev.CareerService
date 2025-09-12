using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/applications")]
[EnableRateLimiting("CareerPolicy")]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly ILogger<JobApplicationController> _logger;

    public JobApplicationController(
        IJobApplicationService jobApplicationService,
        ILogger<JobApplicationController> logger)
    {
        _jobApplicationService = jobApplicationService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<JobApplicationDto>> GetJobApplication(int id, CancellationToken cancellationToken = default)
    {
        var application = await _jobApplicationService.GetByIdAsync(id, cancellationToken);
        
        if (application == null)
        {
            return NotFound($"Job application with ID {id} not found");
        }

        return Ok(application);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<JobApplicationDto>>> GetAllJobApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _jobApplicationService.GetAllAsync(page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("position/{jobPositionId:int}")]
    [Authorize]
    public async Task<ActionResult<PagedResult<JobApplicationDto>>> GetApplicationsByJobPosition(
        int jobPositionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _jobApplicationService.GetByJobPositionIdAsync(jobPositionId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetApplicationsByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        var applications = await _jobApplicationService.GetByEmailAsync(email, cancellationToken);
        return Ok(applications);
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<JobApplicationDto>> CreateJobApplication(
        [FromBody] CreateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobApplicationService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Job application created with ID {Id} for position {JobPositionId} by {ApplicantEmail}", 
                result.Id, result.JobPositionId, result.ApplicantEmail);
            
            return CreatedAtAction(
                nameof(GetJobApplication), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application for position {JobPositionId}", request.JobPositionId);
            return StatusCode(500, "An error occurred while creating the job application");
        }
    }

    [HttpPut("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<JobApplicationDto>> UpdateApplicationStatus(
        int id,
        [FromBody] UpdateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _jobApplicationService.UpdateStatusAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Job application with ID {id} not found");
            }

            _logger.LogInformation("Job application {Id} status updated to {Status} by user", id, request.Status);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job application status {Id}", id);
            return StatusCode(500, "An error occurred while updating the job application status");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteJobApplication(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _jobApplicationService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Job application with ID {id} not found");
            }

            _logger.LogInformation("Job application {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application {Id}", id);
            return StatusCode(500, "An error occurred while deleting the job application");
        }
    }

    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckJobApplicationExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _jobApplicationService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("check-duplicate")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> CheckDuplicateApplication(
        [FromQuery] string email,
        [FromQuery] int jobPositionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || jobPositionId <= 0)
        {
            return BadRequest("Email and valid job position ID are required");
        }

        var hasExisting = await _jobApplicationService.HasExistingApplicationAsync(email, jobPositionId, cancellationToken);
        return Ok(hasExisting);
    }

    [HttpGet("statuses")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<string>> GetValidStatuses()
    {
        return Ok(ApplicationStatus.ValidStatuses);
    }
}