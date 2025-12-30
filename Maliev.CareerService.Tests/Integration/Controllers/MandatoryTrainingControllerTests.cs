using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Data.Models;
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<MandatoryTrainingRequirementDto>();
        created.Should().NotBeNull();
        created!.TrainingProgramId.Should().Be(programId);
        created.CompletionDeadlineDays.Should().Be(45);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<MandatoryTrainingRequirementDto>>();
        list.Should().NotBeNull();
    }
}
