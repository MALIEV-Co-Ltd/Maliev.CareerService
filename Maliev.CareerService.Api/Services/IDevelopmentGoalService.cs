using Maliev.CareerService.Api.Models.DevelopmentGoals;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service interface for managing Development Goals
/// </summary>
public interface IDevelopmentGoalService
{
    /// <summary>
    /// Creates a new development goal for an IDP
    /// </summary>
    Task<DevelopmentGoalResponse> CreateGoalAsync(
        Guid idpId,
        CreateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing development goal
    /// </summary>
    Task<DevelopmentGoalResponse> UpdateGoalAsync(
        Guid goalId,
        UpdateDevelopmentGoalRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a development goal
    /// </summary>
    Task<DevelopmentGoalResponse> UpdateGoalStatusAsync(
        Guid goalId,
        UpdateGoalStatusRequest request,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all goals for a specific IDP
    /// </summary>
    Task<List<DevelopmentGoalResponse>> GetGoalsByIDPAsync(
        Guid idpId,
        CancellationToken cancellationToken = default);
}
