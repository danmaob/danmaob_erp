namespace DanmaobErp.Application.Time;

public interface IBusinessClock
{
    public DateTimeOffset UtcNow { get; }
    public DateTimeOffset Now { get; }
    public DateOnly Today { get; }
}
