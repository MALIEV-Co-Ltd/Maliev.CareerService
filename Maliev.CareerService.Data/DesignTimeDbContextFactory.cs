using Maliev.CareerService.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.CareerService.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CareerDbContext");

            optionsBuilder.UseNpgsql(connectionString);

            return new CareerDbContext(optionsBuilder.Options);
    }
}