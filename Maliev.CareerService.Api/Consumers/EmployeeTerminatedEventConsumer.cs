using Maliev.CareerService.Api.Services;
using Maliev.MessagingContracts.Generated;
using MassTransit;

namespace Maliev.CareerService.Api.Consumers;

/// <summary>
/// Consumes EmployeeTerminatedEvent to deactivate mandatory trainingAssignments (Feature 003).
/// </summary>
public class EmployeeTerminatedEventConsumer(
    IMandatoryTrainingService mandatoryTrainingService,
    ILogger<EmployeeTerminatedEventConsumer> logger) : IConsumer<EmployeeTerminatedEvent>
{
    private readonly IMandatoryTrainingService _mandatoryTrainingService = mandatoryTrainingService;
    private readonly ILogger<EmployeeTerminatedEventConsumer> _logger = logger;

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<EmployeeTerminatedEvent> context)
    {
        var @event = context.Message;
        var payload = @event.Payload; // Access payload

        _logger.LogInformation("Processing EmployeeTerminatedEvent for Employee {EmployeeId}", payload.EmployeeId);

        try
        {
            await _mandatoryTrainingService.DeactivateAssignmentsAsync(payload.EmployeeId, context.CancellationToken);
            _logger.LogInformation("Successfully deactivated mandatory training for terminated Employee {EmployeeId}", payload.EmployeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating mandatory training for Employee {EmployeeId} via event consumer", payload.EmployeeId);
            throw;
        }
    }
}
