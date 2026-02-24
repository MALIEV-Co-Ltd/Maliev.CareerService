using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Data.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for Development Goal endpoints
/// </summary>
public class DevelopmentGoalTests(CareerServiceWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
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
        var response = await Client.PostAsJsonAsync($"/career/v1/idps/{idp.Id}/goals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.GoalTitle, result!.GoalTitle);
        Assert.Equal("NotStarted", result.Status);
        Assert.Equal(idp.Id, result.IdpId);
    }

    [Fact]
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
        var response = await Client.PostAsJsonAsync($"/career/v1/idps/{idp.Id}/goals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
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
        var response = await Client.PutAsJsonAsync($"/career/v1/goals/{goal.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.GoalTitle, result!.GoalTitle);
        Assert.Equal(request.Category, result.Category);
    }

    [Fact]
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
        var response = await Client.PutAsJsonAsync($"/career/v1/goals/{goal.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
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
        var response = await Client.PatchAsJsonAsync($"/career/v1/goals/{goal.Id}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DevelopmentGoalResponse>();
        Assert.NotNull(result);
        Assert.Equal("Completed", result!.Status);
        Assert.NotNull(result.CompletionDate);
    }

    [Fact]
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
        var response = await Client.PatchAsJsonAsync($"/career/v1/goals/{goal.Id}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
