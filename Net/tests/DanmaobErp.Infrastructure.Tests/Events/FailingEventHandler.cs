using DanmaobErp.Application.Events;

namespace DanmaobErp.Infrastructure.Tests.Events;

public sealed class FailingEventHandler : IBusinessEventHandler<TestEventOccurred>
{
    public Task HandleAsync(TestEventOccurred businessEvent, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Handler failure.");
    }
}
