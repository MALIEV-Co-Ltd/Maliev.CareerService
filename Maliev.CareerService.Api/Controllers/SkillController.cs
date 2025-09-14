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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<SkillDto>> GetSkill(int id, CancellationToken cancellationToken = default)
    {
        var skill = await _skillService.GetByIdAsync(id, cancellationToken);
        
        if (skill == null)
        {
            return NotFound($"Skill with ID {id} not found");
        }

        return Ok(skill);
    }

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

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories(CancellationToken cancellationToken = default)
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
                nameof(GetSkill), 
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

    [HttpGet("{id:int}/exists")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckSkillExists(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _skillService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateSkillName(
        [FromQuery] string name,
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