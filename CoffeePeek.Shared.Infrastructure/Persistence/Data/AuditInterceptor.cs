using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoffeePeek.Shared.Infrastructure.Persistence.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var entries = eventData.Context!.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            
            if (entry.State == EntityState.Added) 
                entry.Entity.CreatedAtUtc = now;
            
            entry.Entity.UpdatedAtUtc = now;
        }
        return base.SavingChanges(eventData, result);
    }
}