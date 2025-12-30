namespace Maliev.CareerService.Data.Events;

/// <summary>
/// Event published by Employee Service when a new employee is created (Feature 003)
/// </summary>
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string Email,
    Guid? DepartmentId,
    Guid? PositionId,
    DateTime HireDate);
