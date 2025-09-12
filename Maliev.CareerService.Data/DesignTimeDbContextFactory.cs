using Maliev.CareerService.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.CareerService.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        
        // Use environment variable for design-time operations (for migrations)
        // The connection string should be set in environment for migrations:
        // set CareerDbContext="Server=localhost;Database=CareerService;..."
        var connectionString = Environment.GetEnvironmentVariable("CareerDbContext") 
            ?? throw new InvalidOperationException("CareerDbContext environment variable must be set for design-time operations");

        optionsBuilder.UseNpgsql(connectionString);

        return new CareerDbContext(optionsBuilder.Options);
    }
}