using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Auditing;

public abstract class AuditEntry : Entity
{
    public DateTimeOffset OccurredAt { get; private set; }
    
    public ActorType ActorType { get; private set; }
    
    public Guid? ActorId { get; private set; }
    
    public string EntityType { get; private set; }
    
    public Guid EntityId { get; private set; }
    
    public AuditOperation Operation { get; private set; }
    
    public string? BeforeState { get; private set; }
    
    public string? AfterState { get; private set; }

    protected AuditEntry(DateTimeOffset occurredAt, ActorType actorType, Guid? actorId, string entityType, Guid entityId, AuditOperation operation, string? beforeState, string? afterState)
    {
        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The audit time must be in UTC.", nameof(occurredAt));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        OccurredAt = occurredAt;
        ActorType = actorType;
        ActorId = actorId;
        EntityType = entityType;
        EntityId = entityId;
        Operation = operation;
        BeforeState = beforeState;
        AfterState = afterState;
    }

    protected AuditEntry()
    {
        EntityType = string.Empty;
    }
}
