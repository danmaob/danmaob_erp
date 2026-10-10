using DanmaobErp.Application.Time;

namespace DanmaobErp.Infrastructure.Time;

public sealed class BusinessClock : IBusinessClock
{
    public const string TimeZoneId = "America/Mexico_City";

    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public BusinessClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    public DateTimeOffset UtcNow
    {
        get { return _timeProvider.GetUtcNow(); }
    }

    public DateTimeOffset Now
    {
        get { return TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _timeZone); }
    }

    public DateOnly Today
    {
        get { return DateOnly.FromDateTime(Now.DateTime); }
    }
}
