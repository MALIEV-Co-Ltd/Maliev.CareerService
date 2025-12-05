using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Data.Models;
using Maliev.CareerService.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.CareerService.Tests.Integration;

/// <summary>
/// Integration tests for training enrollment functionality
/// </summary>
public class TrainingEnrollmentTests : IClassFixture<TrainingEnrollmentTests.CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _testEmployeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public TrainingEnrollmentTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Add Employee authorization header with specific employee ID
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Employee employee@example.com {_testEmployeeId}");
    }

    [DockerRequiredFact]
    public async Task EnrollInTraining_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var programId = await SeedTrainingProgramAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = programId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-enrollments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingEnrollmentResponse>();
        Assert.NotNull(result);
        Assert.Equal(programId, result!.TrainingProgramId);
        Assert.Equal(_testEmployeeId, result.EmployeeId);
        Assert.Equal("enrolled", result.Status);
        Assert.True(Math.Abs((result.EnrolledAt - DateTime.UtcNow).TotalMinutes) <= 1);
    }

    [DockerRequiredFact]
    public async Task EnrollInTraining_DuplicateEnrollment_ReturnsConflict()
    {
        // Arrange
        var programId = await SeedTrainingProgramAsync();
        await EnrollEmployeeInProgramAsync(programId, _testEmployeeId);

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = programId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-enrollments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task EnrollInTraining_CapacityExceeded_ReturnsBadRequest()
    {
        // Arrange
        var programId = await SeedTrainingProgramWithCapacityAsync(maxParticipants: 2);

        // Fill up capacity
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        await EnrollEmployeeInProgramAsync(programId, employee1);
        await EnrollEmployeeInProgramAsync(programId, employee2);

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = programId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-enrollments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("capacity", content);
    }

    [DockerRequiredFact]
    public async Task EnrollInTraining_InactiveProgram_ReturnsBadRequest()
    {
        // Arrange
        var programId = await SeedInactiveTrainingProgramAsync();

        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = programId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/careers/v1/training-enrollments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not active", content);
    }

    [DockerRequiredFact]
    public async Task GetEnrollments_ViewOwnEnrollments_ReturnsEmployeeEnrollments()
    {
        // Arrange - Use unique employee ID for test isolation
        var testEmployeeId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Employee employee@example.com {testEmployeeId}");

        var program1 = await SeedTrainingProgramAsync("PROG-001");
        var program2 = await SeedTrainingProgramAsync("PROG-002");
        await EnrollEmployeeInProgramAsync(program1, testEmployeeId);
        await EnrollEmployeeInProgramAsync(program2, testEmployeeId);

        // Enroll another employee (should not be returned)
        var otherEmployee = Guid.NewGuid();
        await EnrollEmployeeInProgramAsync(program1, otherEmployee);

        // Act
        var response = await _client.GetAsync("/careers/v1/training-enrollments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingEnrollmentListResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Items.Count);
        foreach (var item in result.Items)
        {
            Assert.True(item.EmployeeId == testEmployeeId);
        }
    }

    [DockerRequiredFact]
    public async Task GetEnrollments_WithStatusFilter_ReturnsFilteredEnrollments()
    {
        // Arrange - Use unique employee ID for test isolation
        var testEmployeeId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Employee employee@example.com {testEmployeeId}");

        var program1 = await SeedTrainingProgramAsync("PROG-003");
        var program2 = await SeedTrainingProgramAsync("PROG-004");
        await EnrollEmployeeInProgramAsync(program1, testEmployeeId);
        var enrollmentId = await EnrollEmployeeInProgramAsync(program2, testEmployeeId);

        // Mark one as completed
        await MarkEnrollmentCompletedAsync(enrollmentId);

        // Act
        var response = await _client.GetAsync("/careers/v1/training-enrollments?status=completed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingEnrollmentListResponse>();
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        foreach (var item in result.Items)
        {
            Assert.True(item.Status == "completed");
        }
    }

    [DockerRequiredFact]
    public async Task MarkTrainingComplete_AsHRStaff_ReturnsOk()
    {
        // Arrange
        var programId = await SeedTrainingProgramAsync("PROG-005");
        var enrollmentId = await EnrollEmployeeInProgramAsync(programId, _testEmployeeId);

        _client.DefaultRequestHeaders.Remove("Authorization");
        var hrStaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer HRStaff hr@example.com {hrStaffId}");

        // Get enrollment to get RowVersion
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var enrollment = await dbContext.EmployeeTrainingEnrollments.FindAsync(enrollmentId);

        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Completed successfully",
            RowVersion = Convert.ToBase64String(enrollment!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/careers/v1/training-enrollments/{enrollmentId}/complete", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TrainingEnrollmentResponse>();
        Assert.NotNull(result);
        Assert.Equal("completed", result!.Status);
        Assert.NotNull(result.CompletedAt);
        Assert.Equal(hrStaffId, result.MarkedCompleteBy);
        Assert.Equal("Completed successfully", result.CompletionNotes);
    }

    [DockerRequiredFact]
    public async Task MarkTrainingComplete_AsEmployee_ReturnsForbidden()
    {
        // Arrange
        var programId = await SeedTrainingProgramAsync("PROG-006");
        var enrollmentId = await EnrollEmployeeInProgramAsync(programId, _testEmployeeId);

        // Reset to Employee authorization
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer Employee employee@example.com {_testEmployeeId}");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();
        var enrollment = await dbContext.EmployeeTrainingEnrollments.FindAsync(enrollmentId);

        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Trying to mark complete as employee",
            RowVersion = Convert.ToBase64String(enrollment!.RowVersion)
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/careers/v1/training-enrollments/{enrollmentId}/complete", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedTrainingProgramAsync(string? programCode = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = programCode ?? $"TEST-{Guid.NewGuid().ToString()[..8]}",
            ProgramName = "Test Training Program",
            Description = "Test program for enrollment tests",
            Category = "Testing",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingPrograms.Add(program);
        await dbContext.SaveChangesAsync();

        return program.Id;
    }

    private async Task<Guid> SeedTrainingProgramWithCapacityAsync(int maxParticipants)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = $"CAP-{Guid.NewGuid().ToString()[..8]}",
            ProgramName = "Limited Capacity Program",
            Description = "Program with capacity limit",
            Category = "Testing",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            MaxParticipants = maxParticipants,
            IsActive = true,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingPrograms.Add(program);
        await dbContext.SaveChangesAsync();

        return program.Id;
    }

    private async Task<Guid> SeedInactiveTrainingProgramAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var program = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = $"INACTIVE-{Guid.NewGuid().ToString()[..8]}",
            ProgramName = "Inactive Program",
            Description = "Inactive program",
            Category = "Testing",
            DurationHours = 10m,
            Provider = "Internal",
            IsMandatory = false,
            TargetRoles = [],
            IsActive = false,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        dbContext.TrainingPrograms.Add(program);
        await dbContext.SaveChangesAsync();

        return program.Id;
    }

    private async Task<Guid> EnrollEmployeeInProgramAsync(Guid programId, Guid employeeId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var enrollment = new EmployeeTrainingEnrollment
        {
            Id = Guid.NewGuid(),
            TrainingProgramId = programId,
            EmployeeId = employeeId,
            EnrolledAt = DateTime.UtcNow,
            EnrollmentType = Data.Models.EnrollmentType.Voluntary,
            Status = TrainingEnrollmentStatus.Enrolled,
            CreatedBy = employeeId,
            UpdatedBy = employeeId
        };

        dbContext.EmployeeTrainingEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        return enrollment.Id;
    }

    private async Task MarkEnrollmentCompletedAsync(Guid enrollmentId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.CareerDbContext>();

        var enrollment = await dbContext.EmployeeTrainingEnrollments.FindAsync(enrollmentId);
        if (enrollment != null)
        {
            enrollment.Status = TrainingEnrollmentStatus.Completed;
            enrollment.StartedAt = DateTime.UtcNow.AddDays(-7);
            enrollment.CompletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Custom WebApplicationFactory that registers mock external services
    /// </summary>
    public class CustomWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            // IMPORTANT: Set environment BEFORE calling base to ensure Program.cs sees Testing environment
            builder.UseEnvironment("Testing");

            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
                // Replace external services with mocks
                services.RemoveAll<Api.Services.External.IEmployeeServiceClient>();
                services.AddSingleton<Api.Services.External.IEmployeeServiceClient, Mocks.MockEmployeeServiceClient>();
            });
        }
    }
}
