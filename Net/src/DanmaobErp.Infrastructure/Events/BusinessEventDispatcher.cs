using DanmaobErp.Application.Events;
using DanmaobErp.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DanmaobErp.Infrastructure.Events;

public sealed class BusinessEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusinessEventDispatcher> _logger;

    public BusinessEventDispatcher(IServiceProvider serviceProvider, ILogger<BusinessEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyList<BusinessEvent> businessEvents, CancellationToken cancellationToken)
    {
        foreach (var businessEvent in businessEvents)
        {
            var handlerType = typeof(IBusinessEventHandler<>).MakeGenericType(businessEvent.GetType());
            var handleMethod = handlerType.GetMethod("HandleAsync");

            if (handleMethod is null)
            {
                continue;
            }

            foreach (var handler in _serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                try
                {
                    var result = handleMethod.Invoke(handler, new object[] { businessEvent, cancellationToken });

                    if (result is Task task)
                    {
                        await task;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Business event handler {HandlerType} failed for event {EventType} {EventId}.", handler.GetType().Name, businessEvent.GetType().Name, businessEvent.EventId);
                }
            }
        }
    }
}
