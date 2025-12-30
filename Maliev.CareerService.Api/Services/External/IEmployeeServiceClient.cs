namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Client for Employee Service integration
/// </summary>
public interface IEmployeeServiceClient
{
    /// <summary>
    /// Gets employee details by employee ID
    /// </summary>
    Task<EmployeeResponse?> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if an employee ID exists
    /// </summary>
    Task<bool> ValidateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Employee response from Employee Service
/// </summary>
public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    string? Position,
    Guid? ManagerId = null,
    Guid? DepartmentId = null,
    Guid? PositionId = null);
