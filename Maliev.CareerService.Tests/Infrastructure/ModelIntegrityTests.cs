using Maliev.CareerService.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.CareerService.Tests.Infrastructure;

/// <summary>Integrity tests.</summary>
public class ModelIntegrityTests
{
    /// <summary>Check for pending migrations.</summary>
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseNpgsql("Host=localhost;Database=ModelCheck")
            .Options;

        using var context = new CareerDbContext(options);
        var hasChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasChanges, "Run 'dotnet ef migrations add <Name> --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api'");
    }
}
