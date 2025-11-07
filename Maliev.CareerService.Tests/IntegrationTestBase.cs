using Maliev.CareerService.Tests.Factories;
using Xunit;

namespace Maliev.CareerService.Tests;

/// <summary>
/// Base class for integration tests using CareerServiceWebApplicationFactory
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CareerServiceWebApplicationFactory>
{
    protected HttpClient Client { get; }
    protected CareerServiceWebApplicationFactory Factory { get; }

    protected IntegrationTestBase(CareerServiceWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

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
        using var dbContext = Factory.CreateDbContext();
        foreach (var entity in entities)
        {
            dbContext.Add(entity);
        }
        await dbContext.SaveChangesAsync();
    }
}

/// <summary>
/// Type alias for TestWebApplicationFactory (for backward compatibility)
/// </summary>
public class CareerServiceWebApplicationFactory : TestWebApplicationFactory
{
}

/// <summary>
/// Type alias for TestWebApplicationFactory (for backward compatibility)
/// </summary>
public class CareerServiceFactory : TestWebApplicationFactory
{
}

/// <summary>
/// Base integration test class using CareerServiceFactory
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<CareerServiceFactory>
{
    protected HttpClient Client { get; }
    protected CareerServiceFactory Factory { get; }

    protected BaseIntegrationTest(CareerServiceFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

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
        using var dbContext = Factory.CreateDbContext();
        foreach (var entity in entities)
        {
            dbContext.Add(entity);
        }
        await dbContext.SaveChangesAsync();
    }
}
