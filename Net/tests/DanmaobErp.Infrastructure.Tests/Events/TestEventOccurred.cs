using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Tests.Events;

public sealed class TestEventOccurred : BusinessEvent
{
    public Guid ItemId { get; }

    public TestEventOccurred(DateTimeOffset occurredAt, Guid? tenantId, Guid? userId, Guid itemId) : base(occurredAt, tenantId, userId)
    {
        ItemId = itemId;
    }
}
