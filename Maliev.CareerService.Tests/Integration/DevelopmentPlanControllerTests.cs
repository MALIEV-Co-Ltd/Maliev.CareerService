using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Data.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for Individual Development Plan (IDP) endpoints
/// </summary>
public class DevelopmentPlanControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DevelopmentPlanControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected HttpClient Client => _client;
    protected CustomWebApplicationFactory Factory => _factory;

    /// <summary>
    /// Generate JWT token for Employee role
    /// </summary>
    protected string GenerateEmployeeToken(Guid userId)
    {
        return $"Employee test@example.com {userId}";
    }

    /// <summary>
    /// Generate JWT token for HRStaff role
    /// </summary>
    protected string GenerateHRStaffToken(Guid userId)
    {
        return $"HRStaff hr@example.com {userId}";
    }

    /// <summary>
    /// Seed database with test data
    /// </summary>
    protected async Task SeedDatabaseAsync(params object[] entities)
    {
        using var dbContext = _factory.CreateDbContext();
        foreach (var entity in entities)
        {
            dbContext.Add(entity);
        }
        await dbContext.SaveChangesAsync();
    }
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
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp);

        // Act
        var response = await Client.GetAsync("/careers/v1/idps");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPListResponse>();
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(employeeId, result.Items[0].EmployeeId);
    }

    [DockerRequiredFact]
    public async Task GetIDPs_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/careers/v1/idps");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task CreateIDP_WithValidRequest_CreatesNewPlan()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Register employee in mock service
        Factory.MockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Test", "Employee", "test@maliev.com", "Engineering", "Engineer"));

        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year
        };

        // Act
        var response = await Client.PostAsJsonAsync("/careers/v1/idps", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        Assert.NotNull(result);
        Assert.Equal(employeeId, result!.EmployeeId);
        Assert.Equal(request.PlanYear, result.PlanYear);
        Assert.Equal("draft", result.Status);
    }

    [DockerRequiredFact]
    public async Task CreateIDP_DuplicatePlanYear_ReturnsConflict()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Register employee in mock service
        Factory.MockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Test", "Employee", "test@maliev.com", "Engineering", "Engineer"));

        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var existingIdp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "draft",
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
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task UpdateIDP_WithValidRequest_UpdatesPlan()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Register employee in mock service
        Factory.MockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Test", "Employee", "test@maliev.com", "Engineering", "Engineer"));

        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp);

        // Reload entity to get database-generated RowVersion
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var savedIdp = await dbContext.IndividualDevelopmentPlans.FindAsync(idp.Id);

        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(savedIdp!.RowVersion)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/careers/v1/idps/{idp.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task SubmitIDP_WithDraftStatus_SubmitsPlan()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Register employee in mock service
        Factory.MockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Test", "Employee", "test@maliev.com", "Engineering", "Engineer"));

        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SeedDatabaseAsync(idp);

        // Act
        var response = await Client.PatchAsync($"/careers/v1/idps/{idp.Id}/submit", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        Assert.NotNull(result);
        Assert.Equal("submitted", result!.Status);
        Assert.NotNull(result.SubmittedAt);
    }

    [DockerRequiredFact]
    public async Task ApproveIDP_AsHRStaff_ApprovesPlan()
    {
        // Arrange
        var hrUserId = Guid.NewGuid();
        var token = GenerateHRStaffToken(hrUserId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var employeeId = Guid.NewGuid();

        // Register employee in mock service
        Factory.MockEmployeeService.AddEmployee(new Api.Services.External.EmployeeResponse(
            employeeId, "Test", "Employee", "test@maliev.com", "Engineering", "Engineer"));

        var idp = new IndividualDevelopmentPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PlanYear = DateTime.UtcNow.Year,
            Status = "submitted",
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await SeedDatabaseAsync(idp);

        // Reload entity to get database-generated RowVersion
        using var scope2 = Factory.Services.CreateScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var savedIdp2 = await dbContext2.IndividualDevelopmentPlans.FindAsync(idp.Id);

        var request = new ApproveIDPRequest
        {
            ApprovalNotes = "Approved - looks good!",
            RowVersion = Convert.ToBase64String(savedIdp2!.RowVersion)
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/careers/v1/idps/{idp.Id}/approve", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IDPResponse>();
        Assert.NotNull(result);
        Assert.Equal("approved", result!.Status);
        Assert.NotNull(result.ApprovedAt);
        Assert.Equal(hrUserId, result.ApprovedBy);
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
            Status = "submitted",
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
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

/// <summary>
/// Custom factory that registers mock employee service to avoid external HTTP calls
/// </summary>
public class CustomWebApplicationFactory : CareerServiceWebApplicationFactory
{
    public Mocks.MockEmployeeServiceClient MockEmployeeService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // Replace real IEmployeeServiceClient with mock
            services.RemoveAll<Api.Services.External.IEmployeeServiceClient>();
            services.AddSingleton<Api.Services.External.IEmployeeServiceClient>(MockEmployeeService);
        });
    }
}
