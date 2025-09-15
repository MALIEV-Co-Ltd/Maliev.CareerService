using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

/// <summary>
/// Controller for managing job positions in the career service.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/positions")]
[EnableRateLimiting("CareerPolicy")]
public class JobPositionController : ControllerBase
{
    private readonly IJobPositionService _jobPositionService;
    private readonly ILogger<JobPositionController> _logger;
    private readonly IBusinessEventLogger _businessEventLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobPositionController"/> class.
    /// </summary>
    /// <param name="jobPositionService">The job position service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="businessEventLogger">The business event logger.</param>
    public JobPositionController(
        IJobPositionService jobPositionService,
        ILogger<JobPositionController> logger,
        IBusinessEventLogger businessEventLogger)
    {
        _jobPositionService = jobPositionService;
        _logger = logger;
        _businessEventLogger = businessEventLogger;
    }

    /// <summary>
    /// Gets a job position by its ID.
    /// </summary>
    /// <param name="id">The ID of the job position to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job position with the specified ID, or NotFound if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/1
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "title": "Senior Software Engineer",
    ///         "department": "Engineering",
    ///         "description": "We are looking for an experienced Senior Software Engineer to join our team...",
    ///         "requirements": "5+ years of experience in C# and .NET Core",
    ///         "responsibilities": "Design, develop, and maintain high-quality software solutions",
    ///         "employmentType": "Full-time",
    ///         "experienceLevel": "Senior",
    ///         "salaryRangeMin": 80000,
    ///         "salaryRangeMax": 120000,
    ///         "currency": "USD",
    ///         "isActive": true,
    ///         "isPublic": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z",
    ///         "workLocations": [
    ///             {
    ///                 "id": 1,
    ///                 "name": "Bangkok Office",
    ///                 "address": "123 Tech Street",
    ///                 "city": "Bangkok",
    ///                 "countryId": 1,
    ///                 "isRemoteAllowed": true,
    ///                 "isHybrid": false,
    ///                 "isActive": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z"
    ///             }
    ///         ],
    ///         "skills": [
    ///             {
    ///                 "skillId": 1,
    ///                 "skillName": ".NET Core",
    ///                 "skillCategory": "Programming",
    ///                 "requiredLevel": "Expert",
    ///                 "isRequired": true
    ///             }
    ///         ],
    ///         "applicationCount": 5
    ///     }
    ///
    /// Error responses:
    ///
    /// 404 Not Found - When the job position with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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

    /// <summary>
    /// Searches for job positions based on the provided search criteria.
    /// </summary>
    /// <param name="request">The search request containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of job positions matching the search criteria.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/search?page=1&amp;pageSize=20&amp;title=engineer&amp;department=engineering&amp;employmentType=full-time&amp;experienceLevel=mid-level
    ///
    /// Sample response:
    ///
    ///     {
    ///         "items": [
    ///             {
    ///                 "id": 1,
    ///                 "title": "Senior Software Engineer",
    ///                 "department": "Engineering",
    ///                 "description": "We are looking for an experienced Senior Software Engineer to join our team...",
    ///                 "requirements": "5+ years of experience in C# and .NET Core",
    ///                 "responsibilities": "Design, develop, and maintain high-quality software solutions",
    ///                 "employmentType": "Full-time",
    ///                 "experienceLevel": "Senior",
    ///                 "salaryRangeMin": 80000,
    ///                 "salaryRangeMax": 120000,
    ///                 "currency": "USD",
    ///                 "isActive": true,
    ///                 "isPublic": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z",
    ///                 "workLocations": [
    ///                     {
    ///                         "id": 1,
    ///                         "name": "Bangkok Office",
    ///                         "address": "123 Tech Street",
    ///                         "city": "Bangkok",
    ///                         "countryId": 1,
    ///                         "isRemoteAllowed": true,
    ///                         "isHybrid": false,
    ///                         "isActive": true,
    ///                         "createdDate": "2025-09-15T10:30:00Z",
    ///                         "modifiedDate": "2025-09-15T10:30:00Z"
    ///                     }
    ///                 ],
    ///                 "skills": [
    ///                     {
    ///                         "skillId": 1,
    ///                         "skillName": ".NET Core",
    ///                         "skillCategory": "Programming",
    ///                         "requiredLevel": "Expert",
    ///                         "isRequired": true
    ///                     }
    ///                 ],
    ///                 "applicationCount": 5
    ///             }
    ///         ],
    ///         "totalCount": 1,
    ///         "page": 1,
    ///         "pageSize": 20,
    ///         "totalPages": 1,
    ///         "hasPrevious": false,
    ///         "hasNext": false
    ///     }
    ///
    /// Query parameters:
    ///
    /// - page: Page number (default: 1)
    /// - pageSize: Number of items per page (default: 20, max: 100)
    /// - title: Filter by job title (case-insensitive partial match)
    /// - department: Filter by department (case-insensitive partial match)
    /// - employmentType: Filter by employment type (Full-time, Part-time, Contract, Internship)
    /// - experienceLevel: Filter by experience level (Entry-level, Mid-level, Senior, Executive)
    /// - workLocationIds: Filter by work location IDs (comma-separated list)
    /// - skillIds: Filter by skill IDs (comma-separated list)
    /// - minSalary: Filter by minimum salary
    /// - maxSalary: Filter by maximum salary
    /// - currency: Filter by currency code (e.g., USD, THB)
    /// - isActive: Filter by active status (true/false)
    /// - isPublic: Filter by public status (true/false)
    /// - searchTerm: General search term (searches in title, description, requirements)
    /// - sortBy: Field to sort by (default: CreatedDate)
    /// - sortDescending: Sort direction (default: true)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request parameters are invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> SearchJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets publicly available job positions based on the provided search criteria.
    /// </summary>
    /// <param name="request">The search request containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of public job positions matching the search criteria.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/public?page=1&amp;pageSize=20&amp;title=engineer&amp;department=engineering
    ///
    /// Sample response:
    ///
    ///     {
    ///         "items": [
    ///             {
    ///                 "id": 1,
    ///                 "title": "Senior Software Engineer",
    ///                 "department": "Engineering",
    ///                 "description": "We are looking for an experienced Senior Software Engineer to join our team...",
    ///                 "requirements": "5+ years of experience in C# and .NET Core",
    ///                 "responsibilities": "Design, develop, and maintain high-quality software solutions",
    ///                 "employmentType": "Full-time",
    ///                 "experienceLevel": "Senior",
    ///                 "salaryRangeMin": 80000,
    ///                 "salaryRangeMax": 120000,
    ///                 "currency": "USD",
    ///                 "isActive": true,
    ///                 "isPublic": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z",
    ///                 "workLocations": [
    ///                     {
    ///                         "id": 1,
    ///                         "name": "Bangkok Office",
    ///                         "address": "123 Tech Street",
    ///                         "city": "Bangkok",
    ///                         "countryId": 1,
    ///                         "isRemoteAllowed": true,
    ///                         "isHybrid": false,
    ///                         "isActive": true,
    ///                         "createdDate": "2025-09-15T10:30:00Z",
    ///                         "modifiedDate": "2025-09-15T10:30:00Z"
    ///                     }
    ///                 ],
    ///                 "skills": [
    ///                     {
    ///                         "skillId": 1,
    ///                         "skillName": ".NET Core",
    ///                         "skillCategory": "Programming",
    ///                         "requiredLevel": "Expert",
    ///                         "isRequired": true
    ///                     }
    ///                 ],
    ///                 "applicationCount": 5
    ///             }
    ///         ],
    ///         "totalCount": 1,
    ///         "page": 1,
    ///         "pageSize": 20,
    ///         "totalPages": 1,
    ///         "hasPrevious": false,
    ///         "hasNext": false
    ///     }
    ///
    /// This endpoint only returns job positions that are marked as public (isPublic = true) and active (isActive = true).
    ///
    /// Query parameters:
    ///
    /// - page: Page number (default: 1)
    /// - pageSize: Number of items per page (default: 20, max: 100)
    /// - title: Filter by job title (case-insensitive partial match)
    /// - department: Filter by department (case-insensitive partial match)
    /// - employmentType: Filter by employment type (Full-time, Part-time, Contract, Internship)
    /// - experienceLevel: Filter by experience level (Entry-level, Mid-level, Senior, Executive)
    /// - workLocationIds: Filter by work location IDs (comma-separated list)
    /// - skillIds: Filter by skill IDs (comma-separated list)
    /// - minSalary: Filter by minimum salary
    /// - maxSalary: Filter by maximum salary
    /// - currency: Filter by currency code (e.g., USD, THB)
    /// - searchTerm: General search term (searches in title, description, requirements)
    /// - sortBy: Field to sort by (default: CreatedDate)
    /// - sortDescending: Sort direction (default: true)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request parameters are invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<JobPositionDto>>> GetPublicJobPositions(
        [FromQuery] JobPositionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _jobPositionService.GetPublicPositionsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a list of unique departments from all job positions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of department names.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/departments
    ///
    /// Sample response:
    ///
    ///     [
    ///         "Engineering",
    ///         "Marketing",
    ///         "Sales",
    ///         "Human Resources",
    ///         "Finance"
    ///     ]
    ///
    /// This endpoint returns all unique department names from active job positions, sorted alphabetically.
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("departments")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments(CancellationToken cancellationToken = default)
    {
        var departments = await _jobPositionService.GetDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    /// <summary>
    /// Creates a new job position.
    /// </summary>
    /// <param name="request">The request containing job position details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created job position.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST careers/v1.0/positions
    ///     {
    ///         "title": "Senior Software Engineer",
    ///         "department": "Engineering",
    ///         "description": "We are looking for an experienced Senior Software Engineer to join our team...",
    ///         "requirements": "5+ years of experience in C# and .NET Core",
    ///         "responsibilities": "Design, develop, and maintain high-quality software solutions",
    ///         "employmentType": "Full-time",
    ///         "experienceLevel": "Senior",
    ///         "salaryRangeMin": 80000,
    ///         "salaryRangeMax": 120000,
    ///         "currency": "USD",
    ///         "isActive": true,
    ///         "isPublic": true,
    ///         "workLocationIds": [1],
    ///         "skills": [
    ///             {
    ///                 "skillId": 1,
    ///                 "requiredLevel": "Expert",
    ///                 "isRequired": true
    ///             }
    ///         ],
    ///         "displayOrder": 1
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "title": "Senior Software Engineer",
    ///         "department": "Engineering",
    ///         "description": "We are looking for an experienced Senior Software Engineer to join our team...",
    ///         "requirements": "5+ years of experience in C# and .NET Core",
    ///         "responsibilities": "Design, develop, and maintain high-quality software solutions",
    ///         "employmentType": "Full-time",
    ///         "experienceLevel": "Senior",
    ///         "salaryRangeMin": 80000,
    ///         "salaryRangeMax": 120000,
    ///         "currency": "USD",
    ///         "isActive": true,
    ///         "isPublic": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z",
    ///         "workLocations": [
    ///             {
    ///                 "id": 1,
    ///                 "name": "Bangkok Office",
    ///                 "address": "123 Tech Street",
    ///                 "city": "Bangkok",
    ///                 "countryId": 1,
    ///                 "isRemoteAllowed": true,
    ///                 "isHybrid": false,
    ///                 "isActive": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z"
    ///             }
    ///         ],
    ///         "skills": [
    ///             {
    ///                 "skillId": 1,
    ///                 "skillName": ".NET Core",
    ///                 "skillCategory": "Programming",
    ///                 "requiredLevel": "Expert",
    ///                 "isRequired": true
    ///             }
    ///         ],
    ///         "applicationCount": 0
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - title: Required. Job position title (max 200 characters)
    /// - department: Required. Department name (max 100 characters)
    /// - description: Required. Detailed job description
    /// - requirements: Optional. Job requirements
    /// - responsibilities: Optional. Job responsibilities
    /// - employmentType: Required. Employment type (Full-time, Part-time, Contract, Internship)
    /// - experienceLevel: Required. Experience level (Entry-level, Mid-level, Senior, Executive)
    /// - salaryRangeMin: Optional. Minimum salary range
    /// - salaryRangeMax: Optional. Maximum salary range
    /// - currency: Optional. Currency code (max 3 characters)
    /// - isActive: Optional. Whether the position is active (default: true)
    /// - isPublic: Optional. Whether the position is public (default: true)
    /// - workLocationIds: Optional. List of work location IDs
    /// - skills: Optional. List of required skills
    /// - displayOrder: Optional. Display order (default: 0)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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
            
            // Log business event
            _businessEventLogger.LogBusinessEvent("JobPositionCreated", new { 
                JobPositionId = result.Id, 
                Title = result.Title, 
                Department = result.Department,
                CreatedBy = "Unknown" // In a real implementation, this would be the authenticated user
            });
            
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

    /// <summary>
    /// Updates an existing job position.
    /// </summary>
    /// <param name="id">The ID of the job position to update.</param>
    /// <param name="request">The request containing updated job position details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated job position, or NotFound if the position doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT careers/v1.0/positions/1
    ///     {
    ///         "title": "Lead Software Engineer",
    ///         "department": "Engineering",
    ///         "description": "We are looking for an experienced Lead Software Engineer to lead our team...",
    ///         "requirements": "8+ years of experience in C# and .NET Core",
    ///         "responsibilities": "Lead, design, develop, and maintain high-quality software solutions",
    ///         "employmentType": "Full-time",
    ///         "experienceLevel": "Senior",
    ///         "salaryRangeMin": 100000,
    ///         "salaryRangeMax": 150000,
    ///         "currency": "USD",
    ///         "isActive": true,
    ///         "isPublic": true,
    ///         "workLocationIds": [1, 2],
    ///         "skills": [
    ///             {
    ///                 "skillId": 1,
    ///                 "requiredLevel": "Expert",
    ///                 "isRequired": true
    ///             },
    ///             {
    ///                 "skillId": 2,
    ///                 "requiredLevel": "Intermediate",
    ///                 "isRequired": false
    ///             }
    ///         ],
    ///         "displayOrder": 1
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "title": "Lead Software Engineer",
    ///         "department": "Engineering",
    ///         "description": "We are looking for an experienced Lead Software Engineer to lead our team...",
    ///         "requirements": "8+ years of experience in C# and .NET Core",
    ///         "responsibilities": "Lead, design, develop, and maintain high-quality software solutions",
    ///         "employmentType": "Full-time",
    ///         "experienceLevel": "Senior",
    ///         "salaryRangeMin": 100000,
    ///         "salaryRangeMax": 150000,
    ///         "currency": "USD",
    ///         "isActive": true,
    ///         "isPublic": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T11:30:00Z",
    ///         "workLocations": [
    ///             {
    ///                 "id": 1,
    ///                 "name": "Bangkok Office",
    ///                 "address": "123 Tech Street",
    ///                 "city": "Bangkok",
    ///                 "countryId": 1,
    ///                 "isRemoteAllowed": true,
    ///                 "isHybrid": false,
    ///                 "isActive": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z"
    ///             },
    ///             {
    ///                 "id": 2,
    ///                 "name": "Chiang Mai Office",
    ///                 "address": "456 Innovation Avenue",
    ///                 "city": "Chiang Mai",
    ///                 "countryId": 1,
    ///                 "isRemoteAllowed": false,
    ///                 "isHybrid": true,
    ///                 "isActive": true,
    ///                 "createdDate": "2025-09-15T10:30:00Z",
    ///                 "modifiedDate": "2025-09-15T10:30:00Z"
    ///             }
    ///         ],
    ///         "skills": [
    ///             {
    ///                 "skillId": 1,
    ///                 "skillName": ".NET Core",
    ///                 "skillCategory": "Programming",
    ///                 "requiredLevel": "Expert",
    ///                 "isRequired": true
    ///             },
    ///             {
    ///                 "skillId": 2,
    ///                 "skillName": "Azure",
    ///                 "skillCategory": "Cloud",
    ///                 "requiredLevel": "Intermediate",
    ///                 "isRequired": false
    ///             }
    ///         ],
    ///         "applicationCount": 5
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - title: Required. Job position title (max 200 characters)
    /// - department: Required. Department name (max 100 characters)
    /// - description: Required. Detailed job description
    /// - requirements: Optional. Job requirements
    /// - responsibilities: Optional. Job responsibilities
    /// - employmentType: Required. Employment type (Full-time, Part-time, Contract, Internship)
    /// - experienceLevel: Required. Experience level (Entry-level, Mid-level, Senior, Executive)
    /// - salaryRangeMin: Optional. Minimum salary range
    /// - salaryRangeMax: Optional. Maximum salary range
    /// - currency: Optional. Currency code (max 3 characters)
    /// - isActive: Optional. Whether the position is active
    /// - isPublic: Optional. Whether the position is public
    /// - workLocationIds: Optional. List of work location IDs
    /// - skills: Optional. List of required skills
    /// - displayOrder: Optional. Display order
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the job position with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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
            
            // Log business event
            _businessEventLogger.LogBusinessEvent("JobPositionUpdated", new { 
                JobPositionId = result.Id, 
                Title = result.Title, 
                Department = result.Department,
                UpdatedBy = "Unknown" // In a real implementation, this would be the authenticated user
            });
            
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

    /// <summary>
    /// Deletes a job position (marks it as inactive).
    /// </summary>
    /// <param name="id">The ID of the job position to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the position doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE careers/v1.0/positions/1
    ///
    /// Sample response:
    ///
    ///     204 No Content
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Error responses:
    ///
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the job position with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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
            
            // Log business event
            _businessEventLogger.LogBusinessEvent("JobPositionDeleted", new { 
                JobPositionId = id,
                DeletedBy = "Unknown" // In a real implementation, this would be the authenticated user
            });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job position {Id}", id);
            return StatusCode(500, "An error occurred while deleting the job position");
        }
    }

    /// <summary>
    /// Checks if a job position with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the job position to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job position exists, false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/1/exists
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckJobPositionExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _jobPositionService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    /// <summary>
    /// Validates if a job position with the specified title and department already exists.
    /// </summary>
    /// <param name="title">The title to validate.</param>
    /// <param name="department">The department to validate.</param>
    /// <param name="excludeId">Optional ID to exclude from validation (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job position is valid (doesn't exist), false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/positions/validate?title=Senior%20Software%20Engineer&amp;department=Engineering
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When title or department is missing or invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
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