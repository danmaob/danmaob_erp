using DanmaobErp.Application.Events;

namespace DanmaobErp.Infrastructure.Tests.Events;

public sealed class RecordingEventHandler : IBusinessEventHandler<TestEventOccurred>
{
    public List<TestEventOccurred> Received { get; } = new List<TestEventOccurred>();

    public Task HandleAsync(TestEventOccurred businessEvent, CancellationToken cancellationToken)
    {
        Received.Add(businessEvent);
        return Task.CompletedTask;
    }
}
