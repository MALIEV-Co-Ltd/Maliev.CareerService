using Maliev.CareerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.CareerService.Tests.Unit;

public abstract class BaseUnitTests : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _dbContainer = 
#pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18")
        .Build();
#pragma warning restore CS0618

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



