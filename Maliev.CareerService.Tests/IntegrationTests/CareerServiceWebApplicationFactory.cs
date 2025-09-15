using Maliev.CareerService.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Maliev.CareerService.Data.DbContexts;

namespace Maliev.CareerService.Tests.IntegrationTests;

public class CareerServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    public async Task ClearDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CareerDbContext>();
        
        // Clear all data from the database
        context.JobPositionSkills.RemoveRange(context.JobPositionSkills);
        context.JobPositionLocations.RemoveRange(context.JobPositionLocations);
        context.JobApplications.RemoveRange(context.JobApplications);
        context.ApplicationDocuments.RemoveRange(context.ApplicationDocuments);
        context.JobPositions.RemoveRange(context.JobPositions);
        context.WorkLocations.RemoveRange(context.WorkLocations);
        context.Skills.RemoveRange(context.Skills);
        
        await context.SaveChangesAsync();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
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
            // database for testing with the same database name as the API uses in Testing environment.
            services.AddDbContext<CareerDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            
            // Remove the CacheWarmingService to prevent interference with tests
            var cacheWarmingDescriptor = services.SingleOrDefault(
                d => d.ImplementationType == typeof(CacheWarmingService));
                
            if (cacheWarmingDescriptor != null)
            {
                services.Remove(cacheWarmingDescriptor);
            }
        });
    }
}