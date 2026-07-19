using Maliev.CareerService.Application.Models.DevelopmentGoals;

namespace Maliev.CareerService.Application.Services;

/// <summary>
/// Service interface for managing Development Goals
/// </summary>
public interface IDevelopmentGoalService
{
    /// <summary>
    /// Creates a new development goal for an IDP
    /// </summary>
    /// <param name="idpId">The IDP identifier.</param>
    /// <param name="request">The creation request.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created development goal.</returns>
    Task<DevelopmentGoalResponse> CreateGoalAsync(
        Guid idpId,
        CreateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing development goal
    /// </summary>
    /// <param name="goalId">The goal identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated development goal.</returns>
    Task<DevelopmentGoalResponse> UpdateGoalAsync(
        Guid goalId,
        UpdateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a development goal
    /// </summary>
    /// <param name="goalId">The goal identifier.</param>
    /// <param name="request">The status update request.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated development goal.</returns>
    Task<DevelopmentGoalResponse> UpdateGoalStatusAsync(
        Guid goalId,
        UpdateGoalStatusRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all goals for a specific IDP
    /// </summary>
    /// <param name="idpId">The IDP identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of development goals.</returns>
    Task<List<DevelopmentGoalResponse>> GetGoalsByIDPAsync(
        Guid idpId,
        CancellationToken cancellationToken = default);
}
