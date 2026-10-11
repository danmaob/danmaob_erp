using DanmaobErp.Domain.Common;

namespace DanmaobErp.Application.Events;

public interface IPendingBusinessEvents
{
    public void Add(BusinessEvent businessEvent);
}
