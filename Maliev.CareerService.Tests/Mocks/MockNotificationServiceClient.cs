using Maliev.CareerService.Api.Services.External;

namespace Maliev.CareerService.Tests.Mocks;

/// <summary>
/// Mock implementation of INotificationServiceClient for testing
/// </summary>
public class MockNotificationServiceClient : INotificationServiceClient
{
    private readonly List<NotificationRecord> _notifications = [];

    public IReadOnlyList<NotificationRecord> SentNotifications => _notifications;

    public Task SendCertificationReminderAsync(
        Guid employeeId,
        string courseName,
        DateTime expirationDate,
        int daysUntilExpiration,
        CancellationToken cancellationToken = default)
    {
        _notifications.Add(new NotificationRecord(
            employeeId,
            "CertificationReminder",
            new Dictionary<string, object>
            {
                ["courseName"] = courseName,
                ["expirationDate"] = expirationDate,
                ["daysUntilExpiration"] = daysUntilExpiration
            }));
        return Task.CompletedTask;
    }

    public Task SendTrainingCompletionNotificationAsync(
        Guid employeeId,
        Guid? managerId,
        string courseName,
        DateTime completionDate,
        CancellationToken cancellationToken = default)
    {
        _notifications.Add(new NotificationRecord(
            employeeId,
            "TrainingCompletion",
            new Dictionary<string, object>
            {
                ["courseName"] = courseName,
                ["completionDate"] = completionDate,
                ["managerId"] = managerId ?? Guid.Empty
            }));
        return Task.CompletedTask;
    }

    public Task SendMandatoryTrainingReminderAsync(
        Guid employeeId,
        string trainingName,
        DateTime dueDate,
        int daysOverdue,
        CancellationToken cancellationToken = default)
    {
        _notifications.Add(new NotificationRecord(
            employeeId,
            "MandatoryTrainingReminder",
            new Dictionary<string, object>
            {
                ["trainingName"] = trainingName,
                ["dueDate"] = dueDate,
                ["daysOverdue"] = daysOverdue
            }));
        return Task.CompletedTask;
    }

    public record NotificationRecord(Guid To, string Type, Dictionary<string, object> Data);
}
