using DanmaobErp.Application.Events;
using DanmaobErp.Application.Tenancy;
using DanmaobErp.Infrastructure.Events;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace DanmaobErp.Infrastructure.Tests.Events;

public sealed class UnitOfWorkTests
{
    private static readonly DateTimeOffset UtcTime = new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero);

    private static TestErpDbContext CreateContext(Guid? tenantId)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new TestErpDbContext(builder.Options, new FixedTenantContext(tenantId));
    }

    private static BusinessEventDispatcher CreateDispatcher(params IBusinessEventHandler<TestEventOccurred>[] handlers)
    {
        var services = new ServiceCollection();
        foreach (var handler in handlers)
        {
            services.AddSingleton(handler);
        }
        return new BusinessEventDispatcher(services.BuildServiceProvider(), NullLogger<BusinessEventDispatcher>.Instance);
    }

    [Fact]
    public async Task SaveChangesAsync_PublishesPendingEventOnce()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId);
        var recorder = new RecordingEventHandler();
        var pendingEvents = new PendingBusinessEvents();
        var unitOfWork = new UnitOfWork(context, pendingEvents, CreateDispatcher(recorder));
        context.Set<TestTenantItem>().Add(new TestTenantItem("Item"));
        var businessEvent = new TestEventOccurred(UtcTime, tenantId, null, Guid.CreateVersion7());
        pendingEvents.Add(businessEvent);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        Assert.Same(businessEvent, Assert.Single(recorder.Received));
    }

    [Fact]
    public async Task SaveChangesAsync_CalledAgain_DoesNotPublishTheSameEventTwice()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId);
        var recorder = new RecordingEventHandler();
        var pendingEvents = new PendingBusinessEvents();
        var unitOfWork = new UnitOfWork(context, pendingEvents, CreateDispatcher(recorder));
        context.Set<TestTenantItem>().Add(new TestTenantItem("First"));
        pendingEvents.Add(new TestEventOccurred(UtcTime, tenantId, null, Guid.CreateVersion7()));
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        context.Set<TestTenantItem>().Add(new TestTenantItem("Second"));
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        Assert.Single(recorder.Received);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenSaveFails_PublishesNothing()
    {
        await using var context = CreateContext(null);
        var recorder = new RecordingEventHandler();
        var pendingEvents = new PendingBusinessEvents();
        var unitOfWork = new UnitOfWork(context, pendingEvents, CreateDispatcher(recorder));
        context.Set<TestTenantItem>().Add(new TestTenantItem("Item"));
        pendingEvents.Add(new TestEventOccurred(UtcTime, null, null, Guid.CreateVersion7()));
        await Assert.ThrowsAsync<TenantNotResolvedException>(async () => await unitOfWork.SaveChangesAsync(CancellationToken.None));
        Assert.Empty(recorder.Received);
        Assert.Empty(pendingEvents.TakeAll());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenHandlerFails_OtherHandlersStillReceiveTheEvent()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId);
        var recorder = new RecordingEventHandler();
        var pendingEvents = new PendingBusinessEvents();
        var unitOfWork = new UnitOfWork(context, pendingEvents, CreateDispatcher(new FailingEventHandler(), recorder));
        context.Set<TestTenantItem>().Add(new TestTenantItem("Item"));
        pendingEvents.Add(new TestEventOccurred(UtcTime, tenantId, null, Guid.CreateVersion7()));
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        Assert.Single(recorder.Received);
    }
}
