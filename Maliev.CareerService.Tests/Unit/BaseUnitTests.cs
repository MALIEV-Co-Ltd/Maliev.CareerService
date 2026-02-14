using Maliev.CareerService.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

public abstract class BaseUnitTests : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().WithName("postgres:18")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected CareerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        var context = new CareerDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
