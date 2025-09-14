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

public class SkillControllerTests : IClassFixture<CareerServiceWebApplicationFactory>
{
    private readonly CareerServiceWebApplicationFactory _factory;

    public SkillControllerTests(CareerServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSkills_ReturnsAllSkills()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Clear database before seeding
        await _factory.ClearDatabaseAsync();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var skill1 = new Skill
        {
            Id = 1,
            Name = ".NET Core",
            Category = "Programming",
            IsActive = true
        };

        var skill2 = new Skill
        {
            Id = 2,
            Name = "React",
            Category = "Frontend",
            IsActive = true
        };

        context.Skills.AddRange(skill1, skill2);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/skills");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var skills = await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>();
        skills.Should().NotBeNull();
        skills!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSkillCategories_ReturnsUniqueCategories()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Clear database before seeding
        await _factory.ClearDatabaseAsync();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Seed test data
        var skill1 = new Skill
        {
            Id = 1,
            Name = ".NET Core",
            Category = "Programming",
            IsActive = true
        };

        var skill2 = new Skill
        {
            Id = 2,
            Name = "React",
            Category = "Frontend",
            IsActive = true
        };

        var skill3 = new Skill
        {
            Id = 3,
            Name = "Java",
            Category = "Programming", // Same category
            IsActive = true
        };

        context.Skills.AddRange(skill1, skill2, skill3);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/careers/v1.0/skills/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        categories.Should().NotBeNull();
        categories!.Should().HaveCount(2);
        categories.Should().Contain(new[] { "Programming", "Frontend" });
    }
}