using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Maliev.CareerService.Domain.Entities;

namespace Maliev.CareerService.Infrastructure.Interceptors;

public class RowVersionUpdateInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateRowVersions(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateRowVersions(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void UpdateRowVersions(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                baseEntity.RowVersion = Guid.NewGuid().ToByteArray();
            }
        }
    }
}
