using DanmaobErp.Application.Tenancy;
using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DanmaobErp.Infrastructure.Persistence;

public static class TenantWriteGuard
{
    public static void Apply(ChangeTracker changeTracker, ITenantContext tenantContext)
    {
        var entries = changeTracker.Entries<ITenantOwned>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var tenantId = tenantContext.RequireTenantId();

        foreach (var entry in entries)
        {
            var tenantProperty = entry.Property(nameof(ITenantOwned.TenantId));

            if (entry.State is EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    tenantProperty.CurrentValue = tenantId;
                    continue;
                }

                if (entry.Entity.TenantId != tenantId)
                {
                    throw new CrossTenantWriteException(entry.Entity.GetType().Name);
                }
            }

            if (tenantProperty.IsModified is true)
            {
                throw new CrossTenantWriteException(entry.Entity.GetType().Name);
            }

            if (entry.Entity.TenantId != tenantId)
            {
                throw new CrossTenantWriteException(entry.Entity.GetType().Name);
            }
        }
    }
}
