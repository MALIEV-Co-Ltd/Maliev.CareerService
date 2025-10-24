using FluentAssertions;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Data.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for Individual Development Plan (IDP) endpoints
/// </summary>
public class DevelopmentPlanControllerTests(CareerServiceWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [DockerRequiredFact]
    public async Task GetIDPs_AsEmployee_ReturnsOwnPlans()
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

        // Act
        var response = await Client.GetAsync("/careers/v1/idps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IDPListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].EmployeeId.Should().Be(employeeId);
    }

    [DockerRequiredFact]
    public async Task GetIDPs_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/careers/v1/idps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerRequiredFact]
    public async Task CreateIDP_WithValidRequest_CreatesNewPlan()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year
        };

        // Act
        var response = await Client.PostAsJsonAsync("/careers/v1/idps", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employeeId);
        result.PlanYear.Should().Be(request.PlanYear);
        result.Status.Should().Be("Draft");
    }

    [DockerRequiredFact]
    public async Task CreateIDP_DuplicatePlanYear_ReturnsConflict()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var existingIdp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(existingIdp);

        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year
        };

        // Act
        var response = await Client.PostAsJsonAsync("/careers/v1/idps", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [DockerRequiredFact]
    public async Task UpdateIDP_WithValidRequest_UpdatesPlan()
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

        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/careers/v1/idps/{idp.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerRequiredFact]
    public async Task SubmitIDP_WithDraftStatus_SubmitsPlan()
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

        // Act
        var response = await Client.PatchAsync($"/careers/v1/idps/{idp.Id}/submit", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Submitted");
        result.SubmittedAt.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task ApproveIDP_AsHRStaff_ApprovesPlan()
    {
        // Arrange
        var hrUserId = Guid.NewGuid();
        var token = GenerateHRStaffToken(hrUserId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var employeeId = Guid.NewGuid();
        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await SeedDatabaseAsync(idp);

        var request = new ApproveIDPRequest
        {
            ApprovalNotes = "Approved - looks good!",
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/careers/v1/idps/{idp.Id}/approve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Approved");
        result.ApprovedAt.Should().NotBeNull();
        result.ApprovedBy.Should().Be(hrUserId);
    }

    [DockerRequiredFact]
    public async Task ApproveIDP_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            PlanYear = DateTime.UtcNow.Year,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await SeedDatabaseAsync(idp);

        var request = new ApproveIDPRequest
        {
            ApprovalNotes = "Trying to approve",
            RowVersion = Convert.ToBase64String(idp.RowVersion)
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/careers/v1/idps/{idp.Id}/approve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
