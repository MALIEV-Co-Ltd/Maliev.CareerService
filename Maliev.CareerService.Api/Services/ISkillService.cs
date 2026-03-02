using Maliev.CareerService.Api.Models;

namespace Maliev.CareerService.Api.Services;
/// <summary>
/// Service interface for Skill operations
/// </summary>

public interface ISkillService
{
    /// <summary>
    /// Retrieves a skill by its identifier.
    /// </summary>
    /// <param name="id">The skill identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The skill if found; otherwise, null.</returns>
    Task<SkillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all skills with optional filtering.
    /// </summary>
    /// <param name="activeOnly">If true, only active skills are returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of skills.</returns>
    Task<IEnumerable<SkillDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves skills by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <param name="activeOnly">If true, only active skills are returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of skills.</returns>
    Task<IEnumerable<SkillDto>> GetByCategoryAsync(string category, bool activeOnly = true, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a new skill.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created skill.</returns>
    Task<SkillDto> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="id">The skill identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated skill if found; otherwise, null.</returns>
    Task<SkillDto?> UpdateAsync(int id, UpdateSkillRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a skill.
    /// </summary>
    /// <param name="id">The skill identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a skill exists.
    /// </summary>
    /// <param name="id">The skill identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a skill with the same name exists.
    /// </summary>
    /// <param name="name">The skill name.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all unique skill categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of category names.</returns>
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Searches for skills by search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of matching skills.</returns>
    Task<IEnumerable<SkillDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
