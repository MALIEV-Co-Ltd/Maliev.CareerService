namespace Maliev.CareerService.Data.Events;

/// <summary>
/// Event published when a certification is about to expire (Feature 003)
/// </summary>
public record CertificationExpiringEvent(
    Guid EmployeeId,
    string CourseName,
    DateTime ExpirationDate,
    int DaysUntilExpiration);
