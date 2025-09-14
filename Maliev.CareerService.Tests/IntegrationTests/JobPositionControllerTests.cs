using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Maliev.CareerService.Tests.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.CareerService.Tests.IntegrationTests;

public class JobPositionControllerTests : IClassFixture<CareerServiceWebApplicationFactory>
{
    private readonly CareerServiceWebApplicationFactory _factory;

    public JobPositionControllerTests(CareerServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetJobPosition_ExistingId_ReturnsJobPosition()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var workLocation = new WorkLocation
        {
            Id = 1,
            Name = "Bangkok Office",
            Address = "123 Test Street",
            City = "Bangkok",
            CountryId = 1,
            IsActive = true
        };

        var skill = new Skill
        {
            Id = 1,
            Name = ".NET Core",
            Category = "Programming",
            IsActive = true
        };

        var jobPosition = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            SalaryRangeMin = 50000,
            SalaryRangeMax = 80000,
            Currency = "THB",
            IsActive = true,
            IsPublic = true
        };

        context.WorkLocations.Add(workLocation);
        context.Skills.Add(skill);
        context.JobPositions.Add(jobPosition);

        var jobPositionLocation = new JobPositionLocation
        {
            JobPositionId = 1,
            WorkLocationId = 1
        };

        var jobPositionSkill = new JobPositionSkill
        {
            JobPositionId = 1,
            SkillId = 1,
            IsRequired = true,
            RequiredLevel = "Intermediate"
        };

        context.JobPositionLocations.Add(jobPositionLocation);
        context.JobPositionSkills.Add(jobPositionSkill);

        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobPositionDto = await response.Content.ReadFromJsonAsync<JobPositionDto>();
        jobPositionDto.Should().NotBeNull();
        jobPositionDto!.Id.Should().Be(1);
        jobPositionDto.Title.Should().Be("Software Engineer");
        jobPositionDto.Department.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetJobPosition_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchJobPositions_ReturnsPagedResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var jobPosition1 = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            IsActive = true,
            IsPublic = true
        };

        var jobPosition2 = new JobPosition
        {
            Id = 2,
            Title = "Data Scientist",
            Department = "Analytics",
            Description = "Analyze data",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            IsActive = true,
            IsPublic = true
        };

        context.JobPositions.AddRange(jobPosition1, jobPosition2);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<JobPositionDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(2);
        pagedResult.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPublicJobPositions_ReturnsOnlyPublicPositions()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var publicPosition = new JobPosition
        {
            Id = 1,
            Title = "Public Position",
            Department = "Engineering",
            Description = "Public position description",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            IsActive = true,
            IsPublic = true
        };

        var privatePosition = new JobPosition
        {
            Id = 2,
            Title = "Private Position",
            Department = "HR",
            Description = "Private position description",
            EmploymentType = "Full-time",
            ExperienceLevel = "Entry",
            IsActive = true,
            IsPublic = false
        };

        context.JobPositions.AddRange(publicPosition, privatePosition);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/public");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<JobPositionDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(1);
        pagedResult.Items.First().Title.Should().Be("Public Position");
    }

    [Fact]
    public async Task GetDepartments_ReturnsUniqueDepartments()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var jobPosition1 = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            IsActive = true,
            IsPublic = true
        };

        var jobPosition2 = new JobPosition
        {
            Id = 2,
            Title = "Data Scientist",
            Department = "Analytics",
            Description = "Analyze data",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            IsActive = true,
            IsPublic = true
        };

        var jobPosition3 = new JobPosition
        {
            Id = 3,
            Title = "Senior Engineer",
            Department = "Engineering", // Same department
            Description = "Lead development",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            IsActive = true,
            IsPublic = true
        };

        context.JobPositions.AddRange(jobPosition1, jobPosition2, jobPosition3);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/positions/departments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        departments.Should().NotBeNull();
        departments!.Should().HaveCount(2);
        departments.Should().Contain(new[] { "Engineering", "Analytics" });
    }
}