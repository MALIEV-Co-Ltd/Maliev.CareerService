namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// HTTP client implementation for Email Service integration
/// </summary>
public class EmailServiceClient : IEmailServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailServiceClient"/> class.
    /// </summary>

    public EmailServiceClient(
        HttpClient httpClient,
        ILogger<EmailServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendApplicationConfirmationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string applicationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SendEmailRequest(
                To: recipientEmail,
                TemplateId: "application-confirmation",
                TemplateData: new Dictionary<string, object>
                {
                    ["applicantName"] = applicantName,
                    ["positionTitle"] = positionTitle,
                    ["applicationId"] = applicationId
                });

            var response = await _httpClient.PostAsJsonAsync("/emails/v1/send", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Application confirmation email sent to {Email} for application {ApplicationId}",
                recipientEmail,
                applicationId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send application confirmation email to {Email} for application {ApplicationId}",
                recipientEmail,
                applicationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendStatusChangeNotificationAsync(
        string recipientEmail,
        string applicantName,
        string positionTitle,
        string newStatus,
        string? additionalMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateData = new Dictionary<string, object>
            {
                ["applicantName"] = applicantName,
                ["positionTitle"] = positionTitle,
                ["newStatus"] = newStatus
            };

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                templateData["additionalMessage"] = additionalMessage;
            }

            var request = new SendEmailRequest(
                To: recipientEmail,
                TemplateId: "application-status-change",
                TemplateData: templateData);

            var response = await _httpClient.PostAsJsonAsync("/emails/v1/send", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Status change notification email sent to {Email} for new status {Status}",
                recipientEmail,
                newStatus);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send status change notification email to {Email}",
                recipientEmail);
            throw;
        }
    }
}

/// <summary>
/// Send email request to Email Service
/// </summary>
internal record SendEmailRequest(
    string To,
    string TemplateId,
    Dictionary<string, object> TemplateData);
