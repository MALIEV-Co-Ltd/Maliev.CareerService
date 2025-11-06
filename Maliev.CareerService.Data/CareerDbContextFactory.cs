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

        // Use environment variable for connection string during migrations
        // Example: export CareerDbContext="Server=localhost;Port=5432;Database=career_db;User Id=postgres;Password=your_password;"
        var connectionString = Environment.GetEnvironmentVariable("CareerDbContext");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "CareerDbContext environment variable not set. " +
                "Set it before running migrations: export CareerDbContext=\"Server=localhost;Port=5432;Database=career_db;User Id=postgres;Password=your_password;\"");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new CareerDbContext(optionsBuilder.Options);
    }
}
