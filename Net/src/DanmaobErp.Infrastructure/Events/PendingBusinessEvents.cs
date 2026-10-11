using DanmaobErp.Application.Events;
using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Events;

public sealed class PendingBusinessEvents : IPendingBusinessEvents
{
    private readonly List<BusinessEvent> _events = new List<BusinessEvent>();

    public void Add(BusinessEvent businessEvent)
    {
        ArgumentNullException.ThrowIfNull(businessEvent);
        _events.Add(businessEvent);
    }

    public IReadOnlyList<BusinessEvent> TakeAll()
    {
        var events = _events.ToList();
        _events.Clear();
        return events;
    }

    public void Clear()
    {
        _events.Clear();
    }
}
