using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Maliev.CareerService.Infrastructure.Data;

public class CareerDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        var connectionString = args.Length > 0 ? args[0] : "Host=localhost;Database=careerdb;Username=test;Password=test";
        optionsBuilder.UseNpgsql(connectionString);
        return new CareerDbContext(optionsBuilder.Options);
    }
}
