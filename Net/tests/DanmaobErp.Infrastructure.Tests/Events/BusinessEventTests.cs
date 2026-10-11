using System.Text.Json;

namespace DanmaobErp.Infrastructure.Tests.Events;

public sealed class BusinessEventTests
{
    private static readonly DateTimeOffset UtcTime = new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Serialize_IncludesEveryField()
    {
        var tenantId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var itemId = Guid.CreateVersion7();
        var businessEvent = new TestEventOccurred(UtcTime, tenantId, userId, itemId);
        var json = JsonSerializer.Serialize(businessEvent);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(businessEvent.EventId, root.GetProperty("EventId").GetGuid());
        Assert.Equal(UtcTime, root.GetProperty("OccurredAt").GetDateTimeOffset());
        Assert.Equal(tenantId, root.GetProperty("TenantId").GetGuid());
        Assert.Equal(userId, root.GetProperty("UserId").GetGuid());
        Assert.Equal(itemId, root.GetProperty("ItemId").GetGuid());
    }

    [Fact]
    public void Constructor_RejectsTimeThatIsNotUtc()
    {
        var localTime = new DateTimeOffset(2026, 10, 11, 12, 30, 0, TimeSpan.FromHours(-6));
        var exception = Assert.Throws<ArgumentException>(() => new TestEventOccurred(localTime, null, null, Guid.CreateVersion7()));
        Assert.Equal("occurredAt", exception.ParamName);
    }
}
