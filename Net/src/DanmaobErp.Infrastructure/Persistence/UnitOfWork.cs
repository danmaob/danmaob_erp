using DanmaobErp.Application.Persistence;
using DanmaobErp.Infrastructure.Events;

namespace DanmaobErp.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ErpDbContext _dbContext;
    private readonly PendingBusinessEvents _pendingEvents;
    private readonly BusinessEventDispatcher _dispatcher;

    public UnitOfWork(ErpDbContext dbContext, PendingBusinessEvents pendingEvents, BusinessEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _pendingEvents = pendingEvents;
        _dispatcher = dispatcher;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _pendingEvents.Clear();
            throw;
        }

        var events = _pendingEvents.TakeAll();
        await _dispatcher.DispatchAsync(events, cancellationToken);
    }
}
