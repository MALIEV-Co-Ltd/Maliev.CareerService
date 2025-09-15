using Asp.Versioning;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.CareerService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("careers/v{version:apiVersion}/skills")]
[EnableRateLimiting("CareerPolicy")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ILogger<SkillController> _logger;

    public SkillController(
        ISkillService skillService,
        ILogger<SkillController> logger)
    {
        _skillService = skillService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a skill by its ID.
    /// </summary>
    /// <param name="id">The ID of the skill to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The skill with the specified ID, or NotFound if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills/1
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": ".NET Core",
    ///         "category": "Programming",
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z"
    ///     }
    ///
    /// Error responses:
    ///
    /// 404 Not Found - When the skill with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<SkillDto>> GetSkillById(int id, CancellationToken cancellationToken = default)
    {
        var skill = await _skillService.GetByIdAsync(id, cancellationToken);
        
        if (skill == null)
        {
            return NotFound($"Skill with ID {id} not found");
        }

        return Ok(skill);
    }

    /// <summary>
    /// Gets all skills.
    /// </summary>
    /// <param name="activeOnly">Whether to return only active skills (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of skills.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills?activeOnly=true
    ///
    /// Sample response:
    ///
    ///     [
    ///         {
    ///             "id": 1,
    ///             "name": ".NET Core",
    ///             "category": "Programming",
    ///             "isActive": true,
    ///             "createdDate": "2025-09-15T10:30:00Z",
    ///             "modifiedDate": "2025-09-15T10:30:00Z"
    ///         },
    ///         {
    ///             "id": 2,
    ///             "name": "Azure",
    ///             "category": "Cloud",
    ///             "isActive": true,
    ///             "createdDate": "2025-09-15T10:30:00Z",
    ///             "modifiedDate": "2025-09-15T10:30:00Z"
    ///         }
    ///     ]
    ///
    /// Query parameters:
    ///
    /// - activeOnly: Optional. When true, returns only active skills. When false, returns all skills (both active and inactive).
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var skills = await _skillService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(skills);
    }

    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByCategory(
        string category,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest("Category is required");
        }

        var skills = await _skillService.GetByCategoryAsync(category, activeOnly, cancellationToken);
        return Ok(skills);
    }

        /// <summary>
    /// Gets unique skill categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of unique skill categories.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills/categories
    ///
    /// Sample response:
    ///
    ///     [
    ///         "Programming",
    ///         "Cloud",
    ///         "Database",
    ///         "DevOps",
    ///         "UI/UX"
    ///     ]
    ///
    /// This endpoint returns all unique skill categories from active skills, sorted alphabetically.
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    /// <summary>
    /// Gets unique skill categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of unique skill categories.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills/categories
    ///
    /// Sample response:
    ///
    ///     [
    ///         "Programming",
    ///         "Cloud",
    ///         "Database",
    ///         "DevOps",
    ///         "UI/UX"
    ///     ]
    ///
    /// This endpoint returns all unique skill categories from active skills, sorted alphabetically.
    ///
    /// Error responses:
    ///
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetSkillCategories(CancellationToken cancellationToken = default)
    {
        var categories = await _skillService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SkillDto>>> SearchSkills(
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var skills = await _skillService.SearchAsync(searchTerm, cancellationToken);
        return Ok(skills);
    }

    /// <summary>
    /// Creates a new skill.
    /// </summary>
    /// <param name="request">The request containing skill details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created skill.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST careers/v1.0/skills
    ///     {
    ///         "name": "React",
    ///         "category": "Frontend",
    ///         "isActive": true
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": "React",
    ///         "category": "Frontend",
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T10:30:00Z"
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - name: Required. Skill name (max 100 characters)
    /// - category: Required. Skill category (max 100 characters)
    /// - isActive: Optional. Whether the skill is active (default: true)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SkillDto>> CreateSkill(
        [FromBody] CreateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _skillService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("Skill created with ID {Id} by user", result.Id);
            
            return CreatedAtAction(
                nameof(GetSkillById), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            return StatusCode(500, "An error occurred while creating the skill");
        }
    }

    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="id">The ID of the skill to update.</param>
    /// <param name="request">The request containing updated skill details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated skill, or NotFound if the skill doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT careers/v1.0/skills/1
    ///     {
    ///         "name": "React Native",
    ///         "category": "Mobile",
    ///         "isActive": true
    ///     }
    ///
    /// Sample response:
    ///
    ///     {
    ///         "id": 1,
    ///         "name": "React Native",
    ///         "category": "Mobile",
    ///         "isActive": true,
    ///         "createdDate": "2025-09-15T10:30:00Z",
    ///         "modifiedDate": "2025-09-15T11:30:00Z"
    ///     }
    ///
    /// Authentication:
    ///
    /// This endpoint requires authentication with a valid JWT token.
    ///
    /// Request body parameters:
    ///
    /// - name: Required. Skill name (max 100 characters)
    /// - category: Required. Skill category (max 100 characters)
    /// - isActive: Optional. Whether the skill is active
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When the request body is invalid or missing required fields
    /// 401 Unauthorized - When the request is not authenticated
    /// 404 Not Found - When the skill with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<SkillDto>> UpdateSkill(
        int id,
        [FromBody] UpdateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _skillService.UpdateAsync(id, request, cancellationToken);
            
            if (result == null)
            {
                return NotFound($"Skill with ID {id} not found");
            }

            _logger.LogInformation("Skill {Id} updated by user", id);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill {Id}", id);
            return StatusCode(500, "An error occurred while updating the skill");
        }
    }

    /// <summary>
    /// Deletes a skill (marks it as inactive).
    /// </summary>
    /// <param name="id">The ID of the skill to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>NoContent if successful, or NotFound if the skill doesn't exist.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE careers/v1.0/skills/1
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
    /// 404 Not Found - When the skill with the specified ID does not exist
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteSkill(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _skillService.DeleteAsync(id, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Skill with ID {id} not found");
            }

            _logger.LogInformation("Skill {Id} deleted by user", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill {Id}", id);
            return StatusCode(500, "An error occurred while deleting the skill");
        }
    }

    /// <summary>
    /// Checks if a skill with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the skill to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the skill exists, false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills/1/exists
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
    public async Task<ActionResult<bool>> CheckSkillExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _skillService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    /// <summary>
    /// Validates if a skill with the specified name and category already exists.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="category">The category to validate.</param>
    /// <param name="excludeId">Optional ID to exclude from validation (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the skill is valid (doesn't exist), false otherwise.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET careers/v1.0/skills/validate?name=React&amp;category=Frontend
    ///
    /// Sample response:
    ///
    ///     true
    ///
    /// Query parameters:
    ///
    /// - name: Required. Skill name to validate
    /// - category: Required. Skill category to validate
    /// - excludeId: Optional. ID to exclude from validation (for updates)
    ///
    /// Error responses:
    ///
    /// 400 Bad Request - When name or category is missing or invalid
    /// 500 Internal Server Error - When there is an unexpected error
    /// </remarks>
    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateSkill(
        [FromQuery] string name,
        [FromQuery] string category,
        [FromQuery] int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Name is required");
        }

        var exists = await _skillService.ExistsByNameAsync(name, excludeId, cancellationToken);
        return Ok(!exists); // Return true if valid (doesn't exist)
    }
}