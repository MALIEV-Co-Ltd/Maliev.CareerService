using System.Net;
using System.Net.Http.Json;
using Maliev.CareerService.Api.Models.Skills;
using Maliev.CareerService.Domain.Entities;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class SkillsControllerTests : IntegrationTestBase
{
    public SkillsControllerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Post_AddSkill_ShouldReturnCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        var request = new AddSkillRequest
        {
            SkillName = "Unit Testing",
            ProficiencyLevel = ProficiencyLevel.Advanced,
            Notes = "Expert in xUnit and Moq"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/career/v1/employees/{employeeId}/skills", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdSkill = await response.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        Assert.NotNull(createdSkill);
        Assert.Equal("Unit Testing", createdSkill!.SkillName);
        Assert.Equal(employeeId, createdSkill.EmployeeId);
    }

    [Fact]
    public async Task Get_GetEmployeeSkills_ShouldReturnList()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Add a skill first
        var createRequest = new AddSkillRequest
        {
            SkillName = "C# Programming",
            ProficiencyLevel = ProficiencyLevel.Expert
        };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/skills");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(createRequest);
        await Client.SendAsync(createMsg);

        var employeeToken = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", employeeToken);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/skills");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var skills = await response.Content.ReadFromJsonAsync<List<EmployeeSkillDto>>();
        Assert.NotNull(skills);
        Assert.Contains(skills, s => s.SkillName == "C# Programming");
    }

    [Fact]
    public async Task Put_UpdateSkill_ShouldUpdateAndReturnOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Add skill
        var createRequest = new AddSkillRequest { SkillName = "Legacy Skill", ProficiencyLevel = ProficiencyLevel.Beginner };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/skills");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(createRequest);
        var createResponse = await Client.SendAsync(createMsg);
        var createdSkill = await createResponse.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        var skillId = createdSkill!.Id;

        // Update skill
        var updateRequest = new UpdateEmployeeSkillRequest
        {
            ProficiencyLevel = ProficiencyLevel.Expert,
            Notes = "Massive improvement",
            IsDevelopmentArea = false
        };
        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/career/v1/employees/{employeeId}/skills/{skillId}");
        updateMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        updateMsg.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await Client.SendAsync(updateMsg);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedSkill = await response.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        Assert.Equal(ProficiencyLevel.Expert, updatedSkill!.ProficiencyLevel);
        Assert.Equal("Massive improvement", updatedSkill.Notes);
    }

    [Fact]
    public async Task Delete_DeleteSkill_ShouldReturnNoContent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Add skill
        var createRequest = new AddSkillRequest { SkillName = "Temporary Skill", ProficiencyLevel = ProficiencyLevel.Beginner };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/skills");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(createRequest);
        var createResponse = await Client.SendAsync(createMsg);
        var createdSkill = await createResponse.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        var skillId = createdSkill!.Id;

        // Act
        var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/career/v1/employees/{employeeId}/skills/{skillId}");
        deleteMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        var response = await Client.SendAsync(deleteMsg);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/career/v1/employees/{employeeId}/skills");
        getMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        var getResponse = await Client.SendAsync(getMsg);
        var skills = await getResponse.Content.ReadFromJsonAsync<List<EmployeeSkillDto>>();
        Assert.DoesNotContain(skills!, s => s.Id == skillId);
    }
}
