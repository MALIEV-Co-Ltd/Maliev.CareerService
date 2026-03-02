using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.CareerService.Data;

/// <summary>
/// Design-time factory for creating CareerDbContext instances during migrations
/// </summary>
public class CareerDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();

        // Use hardcoded connection string for design-time operations
        var connectionString = "Host=localhost;Database=career_design;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new CareerDbContext(optionsBuilder.Options);
    }
}
