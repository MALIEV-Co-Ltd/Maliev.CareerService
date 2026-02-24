namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Client for Notification Service integration (Feature 003: Training Migration)
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// Sends certification expiration reminder to employee
    /// </summary>
    Task SendCertificationReminderAsync(
        Guid employeeId,
        string courseName,
        DateTime expirationDate,
        int daysUntilExpiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends training completion notification to employee and manager
    /// </summary>
    Task SendTrainingCompletionNotificationAsync(
        Guid employeeId,
        Guid? managerId,
        string courseName,
        DateTime completionDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends mandatory training overdue reminder
    /// </summary>
    Task SendMandatoryTrainingReminderAsync(
        Guid employeeId,
        string trainingName,
        DateTime dueDate,
        int daysOverdue,
        CancellationToken cancellationToken = default);
}
