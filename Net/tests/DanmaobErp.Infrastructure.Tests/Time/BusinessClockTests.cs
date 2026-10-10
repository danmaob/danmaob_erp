using DanmaobErp.Infrastructure.Time;

namespace DanmaobErp.Infrastructure.Tests.Time;

public sealed class BusinessClockTests
{
    [Fact]
    public void Today_IsStillCurrentMonth_At2330OnLastDayInCentralMexico()
    {
        var utcNow = new DateTimeOffset(2026, 11, 1, 5, 30, 0, TimeSpan.Zero);
        var clock = new BusinessClock(new FixedTimeProvider(utcNow));

        Assert.Equal(new DateOnly(2026, 10, 31), clock.Today);
        Assert.Equal(10, clock.Today.Month);
    }

    [Fact]
    public void Today_IsNextMonth_AtMidnightInCentralMexico()
    {
        var utcNow = new DateTimeOffset(2026, 11, 1, 6, 0, 0, TimeSpan.Zero);
        var clock = new BusinessClock(new FixedTimeProvider(utcNow));

        Assert.Equal(new DateOnly(2026, 11, 1), clock.Today);
    }

    [Fact]
    public void Now_UsesCentralMexicoOffsetWithoutDaylightSaving()
    {
        var utcNow = new DateTimeOffset(2026, 7, 15, 18, 0, 0, TimeSpan.Zero);
        var clock = new BusinessClock(new FixedTimeProvider(utcNow));

        Assert.Equal(TimeSpan.FromHours(-6), clock.Now.Offset);
        Assert.Equal(12, clock.Now.Hour);
    }

    [Fact]
    public void UtcNow_ReturnsProviderInstant()
    {
        var utcNow = new DateTimeOffset(2026, 3, 10, 15, 45, 0, TimeSpan.Zero);
        var clock = new BusinessClock(new FixedTimeProvider(utcNow));

        Assert.Equal(utcNow, clock.UtcNow);
    }
}
