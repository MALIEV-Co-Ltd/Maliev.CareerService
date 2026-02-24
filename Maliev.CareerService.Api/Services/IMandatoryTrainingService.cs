using Maliev.CareerService.Api.Models.TrainingRecords;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing mandatory training requirements (Feature 003)
/// </summary>
public interface IMandatoryTrainingService
{
    /// <summary>
    /// Creates a new mandatory training requirement
    /// </summary>
    /// <param name="request">Creation request</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created requirement</returns>
    Task<MandatoryTrainingRequirementDto> CreateRequirementAsync(
        CreateMandatoryRequirementRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mandatory training requirements
    /// </summary>
    /// <param name="activeOnly">If true, only active requirements are returned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of requirements</returns>
    Task<List<MandatoryTrainingRequirementDto>> GetAllRequirementsAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific requirement by ID
    /// </summary>
    /// <param name="id">Requirement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requirement if found, otherwise null</returns>
    Task<MandatoryTrainingRequirementDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mandatory training requirement
    /// </summary>
    /// <param name="id">Requirement identifier</param>
    /// <param name="request">Update request</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated requirement, or null if not found</returns>
    Task<MandatoryTrainingRequirementDto?> UpdateRequirementAsync(
        Guid id,
        UpdateMandatoryRequirementRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a mandatory training requirement
    /// </summary>
    /// <param name="id">Requirement identifier</param>
    /// <param name="currentUserId">Authenticated user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivated, false if not found</returns>
    Task<bool> DeactivateRequirementAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates and assigns mandatory training for an employee based on their department and position.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="departmentId">Optional department ID if already known (e.g. from event).</param>
    /// <param name="positionId">Optional position ID if already known (e.g. from event).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignMandatoryTrainingAsync(
        Guid employeeId,
        Guid? departmentId = null,
        Guid? positionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates all mandatory training assignments for an employee (e.g., on termination)
    /// </summary>
    /// <param name="employeeId">Employee identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAssignmentsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
