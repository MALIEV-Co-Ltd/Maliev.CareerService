namespace Maliev.CareerService.Application.Services.External;

/// <summary>
/// Client for Email Service integration
/// </summary>
public interface IEmailServiceClient
{
    /// <summary>
    /// Sends application confirmation email to applicant
    /// </summary>
    Task SendApplicationConfirmationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string applicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends status change notification email to applicant
    /// </summary>
    Task SendStatusChangeNotificationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string newStatus,
        string? additionalMessage,
        CancellationToken cancellationToken = default);
}
