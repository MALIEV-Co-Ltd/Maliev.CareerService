using Maliev.CareerService.Application.Models.Skills;

namespace Maliev.CareerService.Application.Services;

/// <summary>
/// Service interface for managing employee skills (Feature 003)
/// </summary>
public interface IEmployeeSkillService
{
    /// <summary>
    /// Adds a skill to an employee's profile
    /// </summary>
    /// <param name="employeeId">Employee identifier</param>
    /// <param name="request">Skill addition request</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created employee skill</returns>
    Task<EmployeeSkillDto> AddSkillAsync(
        Guid employeeId,
        AddSkillRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all skills for an employee
    /// </summary>
    /// <param name="employeeId">Employee identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of employee skills</returns>
    Task<List<EmployeeSkillDto>> GetByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an employee's skill proficiency or notes
    /// </summary>
    /// <param name="id">Skill record identifier</param>
    /// <param name="request">Skill update request</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated employee skill, or null if not found</returns>
    Task<EmployeeSkillDto?> UpdateAsync(
        Guid id,
        UpdateEmployeeSkillRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a skill record from an employee's profile
    /// </summary>
    /// <param name="id">Skill record identifier</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific employee skill by ID
    /// </summary>
    /// <param name="id">Skill record identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee skill if found, otherwise null</returns>
    Task<EmployeeSkillDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
