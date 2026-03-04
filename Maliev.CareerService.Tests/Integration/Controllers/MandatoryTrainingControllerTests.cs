using System.Net;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Domain.Entities;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class MandatoryTrainingControllerTests : IntegrationTestBase
{
    public MandatoryTrainingControllerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Post_CreateRequirement_ShouldReturnCreated()
    {
        // Arrange
        var programId = Guid.NewGuid();
        // Seed a training program first
        await SeedDatabaseAsync(new TrainingProgram
        {
            Id = programId,
            ProgramName = "Mandatory Test Program",
            ProgramCode = "MAND-001",
            DurationHours = 1.0m,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        });

        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        var request = new CreateMandatoryRequirementRequest
        {
            TrainingProgramId = programId,
            CompletionDeadlineDays = 45,
            RecertificationMonths = 12
        };

        // Act
        var response = await Client.PostAsJsonAsync("/career/v1/mandatory-training", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MandatoryTrainingRequirementDto>();
        Assert.NotNull(created);
        Assert.Equal(programId, created!.TrainingProgramId);
        Assert.Equal(45, created.CompletionDeadlineDays);
    }

    [Fact]
    public async Task Get_GetAllRequirements_ShouldReturnList()
    {
        // Arrange
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        // Act
        var response = await Client.GetAsync("/career/v1/mandatory-training");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<MandatoryTrainingRequirementDto>>();
        Assert.NotNull(list);
    }
}
