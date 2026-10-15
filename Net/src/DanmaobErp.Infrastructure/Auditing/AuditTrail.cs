using DanmaobErp.Application.Identity;
using DanmaobErp.Application.Time;
using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DanmaobErp.Infrastructure.Auditing;

public static class AuditTrail
{
    public const string ImmutableMessage = "Audit entries cannot be modified or deleted.";

    public static void EnsureImmutable(ChangeTracker changeTracker)
    {
        var changed = changeTracker.Entries<AuditEntry>().Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (changed)
        {
            throw new InvalidOperationException(ImmutableMessage);
        }
    }

    public static void Capture(ChangeTracker changeTracker, ICurrentActor currentActor, IBusinessClock businessClock)
    {
        var entries = changeTracker.Entries<IAuditable>().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var entry in entries)
        {
            var operation = AuditOperation.Updated;
            string? beforeState = null;
            string? afterState = null;

            if (entry.State is EntityState.Added)
            {
                operation = AuditOperation.Created;
                afterState = AuditStateWriter.Write(entry, false, false);
            }
            else if (entry.State is EntityState.Deleted)
            {
                operation = AuditOperation.Deleted;
                beforeState = AuditStateWriter.Write(entry, true, false);
            }
            else
            {
                beforeState = AuditStateWriter.Write(entry, true, true);
                afterState = AuditStateWriter.Write(entry, false, true);
            }

            var occurredAt = businessClock.UtcNow;
            var entityType = entry.Metadata.ClrType.Name;
            var entityId = entry.Entity.Id;

            if (entry.Entity is ITenantOwned tenantOwned)
            {
                changeTracker.Context.Add(new TenantAuditEntry(tenantOwned.TenantId, occurredAt, currentActor.ActorType, currentActor.ActorId, entityType, entityId, operation, beforeState, afterState));
            }
            else
            {
                var scope = entry.Entity as IPlatformAuditScope;
                changeTracker.Context.Add(new PlatformAuditEntry(scope?.AuditClientAccountId, scope?.AuditTenantId, occurredAt, currentActor.ActorType, currentActor.ActorId, entityType, entityId, operation, beforeState, afterState));
            }
        }
    }
}
