using eCommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eCommerce.Persistence.Interceptors;

public class SoftDeletionInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditing(eventData);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAuditing(DbContextEventData eventData)
    {
        var context = eventData.Context;
        if (context is null) return;

        
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if(entry.Entity == null || entry.State != EntityState.Deleted || entry.Entity is not ISoftDeletable softDeletableEntity)
                continue;

            entry.State = EntityState.Modified;
            softDeletableEntity.IsDeleted = true;
        }
    }
}
