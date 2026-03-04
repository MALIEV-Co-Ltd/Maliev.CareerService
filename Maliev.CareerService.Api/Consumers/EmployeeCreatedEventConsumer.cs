using Maliev.CareerService.Api.Services;
using Maliev.MessagingContracts.Contracts.Employee;
using MassTransit;

namespace Maliev.CareerService.Api.Consumers;

/// <summary>
/// Consumes EmployeeCreatedEvent to automatically assign mandatory training (Feature 003).
/// </summary>
public class EmployeeCreatedEventConsumer(
    IMandatoryTrainingService mandatoryTrainingService,
    ILogger<EmployeeCreatedEventConsumer> logger) : IConsumer<EmployeeCreatedEvent>
{
    private readonly IMandatoryTrainingService _mandatoryTrainingService = mandatoryTrainingService;
    private readonly ILogger<EmployeeCreatedEventConsumer> _logger = logger;

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;
        var payload = @event.Payload; // Access payload

        _logger.LogInformation("Processing EmployeeCreatedEvent for Employee {EmployeeId}", payload.EmployeeId);

        try
        {
            await _mandatoryTrainingService.AssignMandatoryTrainingAsync(
                payload.EmployeeId,
                payload.DepartmentId,
                payload.PositionId,
                context.CancellationToken);
            _logger.LogInformation("Successfully assigned mandatory training for Employee {EmployeeId}", payload.EmployeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning mandatory training for Employee {EmployeeId} via event consumer", payload.EmployeeId);
            throw;
        }
    }
}
