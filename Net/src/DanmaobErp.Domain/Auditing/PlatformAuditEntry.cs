namespace DanmaobErp.Domain.Auditing;

public sealed class PlatformAuditEntry : AuditEntry
{
    public Guid? ClientAccountId { get; private set; }

    public Guid? TenantId { get; private set; }

    public PlatformAuditEntry(Guid? clientAccountId, Guid? tenantId, DateTimeOffset occurredAt, ActorType actorType, Guid? actorId, string entityType, Guid entityId, AuditOperation operation, string? beforeState, string? afterState) : base(occurredAt, actorType, actorId, entityType, entityId, operation, beforeState, afterState)
    {
        ClientAccountId = clientAccountId;

        TenantId = tenantId;
    }

    private PlatformAuditEntry()
    {
    }
}
