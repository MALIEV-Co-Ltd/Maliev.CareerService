using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Maliev.CareerService.Data.DbContexts;

namespace Maliev.CareerService.Tests.IntegrationTests;

public class CareerServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<CareerDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context (CareerDbContext) using an in-memory 
            // database for testing with a unique database name for each instance.
            services.AddDbContext<CareerDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        });
    }
}