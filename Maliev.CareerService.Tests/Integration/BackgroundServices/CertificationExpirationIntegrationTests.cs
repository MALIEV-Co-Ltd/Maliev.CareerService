using System.Net.Http.Json;
using Maliev.CareerService.Api.BackgroundServices;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Data.Enums;
using Maliev.CareerService.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.BackgroundServices;

public class CertificationExpirationIntegrationTests : IntegrationTestBase
{
    public CertificationExpirationIntegrationTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task BackgroundService_ShouldMarkRecordsAsExpired()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        // Seed DB directly with an expired record
        var expiredRecord = new TrainingRecord
        {
            Id = recordId,
            EmployeeId = employeeId,
            CourseName = "Soon to be Expired",
            CompletionDate = DateTime.UtcNow.AddDays(-366),
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            Status = TrainingStatus.Completed,
            TrainingType = TrainingType.InPerson,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        await SeedDatabaseAsync(expiredRecord);

        // Resolve background service
        var backgroundService = Factory.Services.GetService<CertificationExpirationReminderBackgroundService>();
        Assert.NotNull(backgroundService);

        // Act - Trigger processing
        // We use the public method I added for testing
        await backgroundService!.ProcessExpirationsAsync(CancellationToken.None);

        // Assert - Verify via API or DB
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/training-records/{recordId}");
        response.EnsureSuccessStatusCode();
        var record = await response.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        Assert.Equal(TrainingStatus.Expired, record!.Status);
    }
}
