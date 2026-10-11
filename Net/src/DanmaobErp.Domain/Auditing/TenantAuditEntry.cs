using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Auditing;

public sealed class TenantAuditEntry : AuditEntry, ITenantOwned
{
    public Guid TenantId { get; private set; }

    public TenantAuditEntry(Guid tenantId, DateTimeOffset occurredAt, ActorType actorType, Guid? actorId, string entityType, Guid entityId, AuditOperation operation, string? beforeState, string? afterState) : base(occurredAt, actorType, actorId, entityType, entityId, operation, beforeState, afterState)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("The tenant is required.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    private TenantAuditEntry()
    {
    }
}
