
namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Notification Service integration (Feature 003: Training Migration)
/// Sends training-related notifications with retry logic via Polly
/// </summary>
public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationServiceClient"/> class.
    /// </summary>
    public NotificationServiceClient(
        HttpClient httpClient,
        ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendCertificationReminderAsync(
        Guid employeeId,
        string courseName,
        DateTime expirationDate,
        int daysUntilExpiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateId = GetReminderTemplateId(daysUntilExpiration);

            var request = new SendNotificationRequest(
                To: employeeId,  // Notification service resolves employee email from ID
                TemplateId: templateId,
                TemplateData: new Dictionary<string, object>
                {
                    ["courseName"] = courseName,
                    ["expirationDate"] = expirationDate.ToString("yyyy-MM-dd"),
                    ["daysRemaining"] = daysUntilExpiration
                });

            var response = await _httpClient.PostAsJsonAsync("/notifications/v1/send", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Sent certification reminder to employee {EmployeeId} for course {CourseName} (expires in {Days} days)",
                employeeId,
                courseName,
                daysUntilExpiration);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send certification reminder to employee {EmployeeId} for course {CourseName}",
                employeeId,
                courseName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendTrainingCompletionNotificationAsync(
        Guid employeeId,
        Guid? managerId,
        string courseName,
        DateTime completionDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SendNotificationRequest(
                To: employeeId,
                TemplateId: "training-completed",
                TemplateData: new Dictionary<string, object>
                {
                    ["courseName"] = courseName,
                    ["completionDate"] = completionDate.ToString("yyyy-MM-dd")
                });

            var response = await _httpClient.PostAsJsonAsync("/notifications/v1/send", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Sent training completion notification to employee {EmployeeId} for course {CourseName}",
                employeeId,
                courseName);

            // Also notify manager if applicable
            if (managerId.HasValue)
            {
                var managerRequest = new SendNotificationRequest(
                    To: managerId.Value,
                    TemplateId: "training-completed-manager",
                    TemplateData: new Dictionary<string, object>
                    {
                        ["employeeId"] = employeeId,
                        ["courseName"] = courseName,
                        ["completionDate"] = completionDate.ToString("yyyy-MM-dd")
                    });

                var managerResponse = await _httpClient.PostAsJsonAsync("/notifications/v1/send", managerRequest, cancellationToken);
                managerResponse.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Sent training completion notification to manager {ManagerId} for employee {EmployeeId}",
                    managerId.Value,
                    employeeId);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send training completion notification for employee {EmployeeId}",
                employeeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendMandatoryTrainingReminderAsync(
        Guid employeeId,
        string trainingName,
        DateTime dueDate,
        int daysOverdue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SendNotificationRequest(
                To: employeeId,
                TemplateId: "mandatory-training-overdue",
                TemplateData: new Dictionary<string, object>
                {
                    ["trainingName"] = trainingName,
                    ["dueDate"] = dueDate.ToString("yyyy-MM-dd"),
                    ["daysOverdue"] = daysOverdue
                });

            var response = await _httpClient.PostAsJsonAsync("/notifications/v1/send", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Sent mandatory training reminder to employee {EmployeeId} for {TrainingName} ({DaysOverdue} days overdue)",
                employeeId,
                trainingName,
                daysOverdue);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send mandatory training reminder to employee {EmployeeId} for {TrainingName}",
                employeeId,
                trainingName);
            throw;
        }
    }

    /// <summary>
    /// Gets the appropriate template ID based on days until expiration
    /// </summary>
    private static string GetReminderTemplateId(int daysRemaining) => daysRemaining switch
    {
        <= 30 => "certification-expiring-30days",
        <= 60 => "certification-expiring-60days",
        _ => "certification-expiring-90days"
    };

    /// <summary>
    /// Request model for sending notifications
    /// </summary>
    private record SendNotificationRequest(
        Guid To,
        string TemplateId,
        Dictionary<string, object> TemplateData);
}
