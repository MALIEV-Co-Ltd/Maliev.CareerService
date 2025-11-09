using FluentAssertions;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for Development Goal endpoints
/// </summary>
public class DevelopmentGoalTests(CareerServiceWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [DockerRequiredFact]
    public async Task CreateGoal_WithValidRequest_CreatesNewGoal()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp);

        var request = new CreateDevelopmentGoalRequest
        {
            GoalTitle = "Master Kubernetes Administration",
            GoalDescription = "Become proficient in deploying and managing containerized applications",
            Category = "Technical",
            TargetDate = DateTime.UtcNow.AddMonths(6),
            ActionItems = "1. Complete CKA certification\n2. Deploy 3 production projects\n3. Mentor 2 junior developers"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/careers/v1/idps/{idp.Id}/goals", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        result.Should().NotBeNull();
        result!.GoalTitle.Should().Be(request.GoalTitle);
        result.Status.Should().Be("NotStarted");
        result.IdpId.Should().Be(idp.Id);
    }

    [DockerRequiredFact]
    public async Task CreateGoal_ForOtherEmployeesIDP_ReturnsForbidden()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = otherEmployeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp);

        var request = new CreateDevelopmentGoalRequest
        {
            GoalTitle = "Learn Python",
            GoalDescription = "Master Python programming",
            Category = "Technical",
            TargetDate = DateTime.UtcNow.AddMonths(3),
            ActionItems = "Complete online course"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/careers/v1/idps/{idp.Id}/goals", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerRequiredFact]
    public async Task UpdateGoal_WithValidRequest_UpdatesGoal()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var goal = new EmployeeDevelopmentGoal
        {
            Id = Guid.NewGuid(),
            IdpId = idp.Id,
            GoalTitle = "Original Title",
            GoalDescription = "Original Description",
            Category = "Technical",
            TargetDate = DateTime.UtcNow.AddMonths(6),
            Status = "NotStarted",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp, goal);

        var request = new UpdateDevelopmentGoalRequest
        {
            GoalTitle = "Updated Title",
            GoalDescription = "Updated Description",
            Category = "Leadership",
            TargetDate = DateTime.UtcNow.AddMonths(12),
            ActionItems = "New action items",
            ProgressNotes = "Making good progress",
            RowVersion = Convert.ToBase64String(goal.RowVersion)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/careers/v1/goals/{goal.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        result.Should().NotBeNull();
        result!.GoalTitle.Should().Be(request.GoalTitle);
        result.Category.Should().Be(request.Category);
    }

    [DockerRequiredFact]
    public async Task UpdateGoal_WithStaleRowVersion_ReturnsConflict()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var goal = new EmployeeDevelopmentGoal
        {
            Id = Guid.NewGuid(),
            IdpId = idp.Id,
            GoalTitle = "Original Title",
            GoalDescription = "Original Description",
            Category = "Technical",
            TargetDate = DateTime.UtcNow.AddMonths(6),
            Status = "NotStarted",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp, goal);

        var request = new UpdateDevelopmentGoalRequest
        {
            GoalTitle = "Updated Title",
            GoalDescription = "Updated Description",
            Category = "Technical",
            TargetDate = DateTime.UtcNow.AddMonths(6),
            ActionItems = "New action items",
            ProgressNotes = "Making progress",
            RowVersion = "InvalidRowVersion=="
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/careers/v1/goals/{goal.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [DockerRequiredFact]
    public async Task UpdateGoalStatus_ToCompleted_UpdatesStatusAndCompletionDate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "approved",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var goal = new EmployeeDevelopmentGoal
        {
            Id = Guid.NewGuid(),
            IdpId = idp.Id,
            GoalTitle = "Complete Certification",
            GoalDescription = "Get certified",
            Category = "Certification",
            TargetDate = DateTime.UtcNow.AddMonths(3),
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp, goal);

        var request = new UpdateGoalStatusRequest
        {
            Status = "Completed",
            CompletionDate = DateTime.UtcNow,
            ProgressNotes = "Successfully completed certification exam",
            RowVersion = Convert.ToBase64String(goal.RowVersion)
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/careers/v1/goals/{goal.Id}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.CompletionDate.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task UpdateGoalStatus_ToCompletedWithoutDate_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "approved",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var goal = new EmployeeDevelopmentGoal
        {
            Id = Guid.NewGuid(),
            IdpId = idp.Id,
            GoalTitle = "Complete Certification",
            GoalDescription = "Get certified",
            Category = "Certification",
            TargetDate = DateTime.UtcNow.AddMonths(3),
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp, goal);

        var request = new UpdateGoalStatusRequest
        {
            Status = "Completed",
            CompletionDate = null, // Missing completion date
            ProgressNotes = "Done",
            RowVersion = Convert.ToBase64String(goal.RowVersion)
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/careers/v1/goals/{goal.Id}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
