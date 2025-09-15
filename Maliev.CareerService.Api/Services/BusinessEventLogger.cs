using Microsoft.Extensions.Logging;

namespace Maliev.CareerService.Api.Services;

public interface IBusinessEventLogger
{
    void LogBusinessEvent(string eventName, object eventData);
    void LogAuditEvent(string action, string resource, string userId, object details);
    void LogSecurityEvent(string eventType, string description, object details);
}

public class BusinessEventLogger : IBusinessEventLogger
{
    private readonly ILogger<BusinessEventLogger> _logger;

    public BusinessEventLogger(ILogger<BusinessEventLogger> logger)
    {
        _logger = logger;
    }

    public void LogBusinessEvent(string eventName, object eventData)
    {
        _logger.LogInformation("BUSINESS_EVENT: {EventName} - {EventData}", eventName, eventData);
    }

    public void LogAuditEvent(string action, string resource, string userId, object details)
    {
        _logger.LogInformation(
            "AUDIT_EVENT: User {UserId} performed {Action} on {Resource} with details {Details}", 
            userId, action, resource, details);
    }
    
    public void LogSecurityEvent(string eventType, string description, object details)
    {
        _logger.LogWarning(
            "SECURITY_EVENT: {EventType} - {Description} with details {Details}", 
            eventType, description, details);
    }
}