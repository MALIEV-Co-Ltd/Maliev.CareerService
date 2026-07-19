using System.Net;
using System.Net.Http.Json;
using Maliev.CareerService.Application.Models.TrainingRecords;
using Maliev.CareerService.Domain.Entities;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Authorization;

public class TrainingRecordsAuthorizationTests : IntegrationTestBase
{
    public TrainingRecordsAuthorizationTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Employee_CanViewOwnRecords()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/training-records");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotViewOtherEmployeeRecords()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{otherEmployeeId}/training-records");

        // Assert
        // NOTE: ViewOwn permission usually requires the ID in the route to match the authenticated user ID.
        // We need to check if the controller enforces this.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HRStaff_CanManageAnyRecords()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new RecordTrainingCompletionRequest
        {
            CourseName = "HR Recorded Course",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/career/v1/employees/{employeeId}/training-records", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotRecordCompletion()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new RecordTrainingCompletionRequest
        {
            CourseName = "Attempted Self Recording",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.Online
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/career/v1/employees/{employeeId}/training-records", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
