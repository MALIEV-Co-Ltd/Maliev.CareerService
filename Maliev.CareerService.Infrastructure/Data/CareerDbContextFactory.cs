using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.CareerService.Infrastructure.Data;

public class CareerDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        return new CareerDbContext(optionsBuilder.Options);
    }
}
