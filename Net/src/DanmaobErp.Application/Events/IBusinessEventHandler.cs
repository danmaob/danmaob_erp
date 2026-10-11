using DanmaobErp.Domain.Common;

namespace DanmaobErp.Application.Events;

public interface IBusinessEventHandler<TEvent> where TEvent : BusinessEvent
{
    public Task HandleAsync(TEvent businessEvent, CancellationToken cancellationToken);
}
