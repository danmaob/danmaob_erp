namespace DanmaobErp.Domain.Auditing;

public sealed class PlatformAuditEntry : AuditEntry
{
    public Guid? TenantId { get; private set; }

    public PlatformAuditEntry(Guid? tenantId, DateTimeOffset occurredAt, ActorType actorType, Guid? actorId, string entityType, Guid entityId, AuditOperation operation, string? beforeState, string? afterState) : base(occurredAt, actorType, actorId, entityType, entityId, operation, beforeState, afterState)
    {
        TenantId = tenantId;
    }

    private PlatformAuditEntry()
    {
    }
}
