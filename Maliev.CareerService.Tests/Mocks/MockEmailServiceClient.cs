using Maliev.CareerService.Api.Services.External;

namespace Maliev.CareerService.Tests.Mocks;

/// <summary>
/// Mock implementation of IEmailServiceClient for testing
/// </summary>
public class MockEmailServiceClient : IEmailServiceClient
{
    public List<SentEmail> SentEmails { get; } = [];

    public Task SendApplicationConfirmationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string applicationId,
        CancellationToken cancellationToken = default)
    {
        SentEmails.Add(new SentEmail
        {
            To = recipientEmail,
            Type = "ApplicationConfirmation",
            Subject = $"Application Received: {positionTitle}",
            Data = new Dictionary<string, string>
            {
                { "ApplicantName", applicantName },
                { "PositionTitle", positionTitle },
                { "ApplicationId", applicationId }
            }
        });

        return Task.CompletedTask;
    }

    public Task SendStatusChangeNotificationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string newStatus,
        string? additionalMessage,
        CancellationToken cancellationToken = default)
    {
        SentEmails.Add(new SentEmail
        {
            To = recipientEmail,
            Type = "StatusChangeNotification",
            Subject = $"Application Update: {positionTitle}",
            Data = new Dictionary<string, string>
            {
                { "ApplicantName", applicantName },
                { "PositionTitle", positionTitle },
                { "NewStatus", newStatus },
                { "AdditionalMessage", additionalMessage ?? string.Empty }
            }
        });

        return Task.CompletedTask;
    }

    public void ClearSentEmails()
    {
        SentEmails.Clear();
    }
}

/// <summary>
/// Represents a sent email for testing purposes
/// </summary>
public class SentEmail
{
    public string To { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = [];
}
