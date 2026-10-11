namespace DanmaobErp.Domain.Common;

public abstract class BusinessEvent
{
    public Guid EventId { get; }

    public DateTimeOffset OccurredAt { get; }

    public Guid? TenantId { get; }

    public Guid? UserId { get; }

    protected BusinessEvent(DateTimeOffset occurredAt, Guid? tenantId, Guid? userId)
    {
        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The event time must be in UTC.", nameof(occurredAt));
        }

        EventId = Guid.CreateVersion7();
        OccurredAt = occurredAt;
        TenantId = tenantId;
        UserId = userId;
    }
}
