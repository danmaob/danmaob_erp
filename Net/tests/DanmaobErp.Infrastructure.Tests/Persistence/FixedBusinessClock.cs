using DanmaobErp.Application.Time;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class FixedBusinessClock : IBusinessClock
{
    public DateTimeOffset UtcNow { get; }

    public DateTimeOffset Now { get; }

    public DateOnly Today { get; }

    public FixedBusinessClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
        Now = utcNow;
        Today = DateOnly.FromDateTime(utcNow.UtcDateTime);
    }
}
