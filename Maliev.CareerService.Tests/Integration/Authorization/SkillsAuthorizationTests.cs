using System.Net;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Authentication;
using Maliev.CareerService.Api.Models.Skills;
using Maliev.CareerService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Authorization;

public class SkillsAuthorizationTests : IntegrationTestBase
{
    public SkillsAuthorizationTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Employee_CanViewOwnSkills()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/skills");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotViewOtherEmployeeSkills()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{otherEmployeeId}/skills");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CanViewTeamSkills()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        // Setup manager relationship in mock
        var mockEmployeeService = (Mocks.MockEmployeeServiceClient)Factory.Services.GetRequiredService<Api.Services.External.IEmployeeServiceClient>();
        mockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Team", "Member", "team@maliev.com", "Engineering", "Developer", managerId));

        var token = Factory.CreateTestJwtToken(managerId.ToString(), new[] { CareerPredefinedRoles.Manager }, new[] { CareerPermissions.Trainings.ViewTeam, CareerPermissions.Trainings.ViewOwn });
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/skills");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotAddSkill()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new AddSkillRequest { SkillName = "Forbidden", ProficiencyLevel = ProficiencyLevel.Intermediate };

        // Act
        var response = await Client.PostAsJsonAsync($"/career/v1/employees/{employeeId}/skills", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
