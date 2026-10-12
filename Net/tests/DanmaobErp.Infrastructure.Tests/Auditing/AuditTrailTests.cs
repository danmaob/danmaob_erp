using DanmaobErp.Domain.Auditing;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Auditing;

public sealed class AuditTrailTests
{
    private static readonly DateTimeOffset UtcTime = new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero);

    private static TestErpDbContext CreateContext(Guid? tenantId, ActorType actorType, Guid? actorId)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new TestErpDbContext(builder.Options, new FixedTenantContext(tenantId), new FixedCurrentActor(actorType, actorId), new FixedBusinessClock(UtcTime));
    }

    [Fact]
    public async Task Created_RecordsActorTimeTenantAndAfterState()
    {
        var tenantId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId, ActorType.User, userId);
        var item = new TestAuditedItem("Item", "secret-value");
        context.Set<TestAuditedItem>().Add(item);
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync();
        Assert.Equal(tenantId, entry.TenantId);
        Assert.Equal(ActorType.User, entry.ActorType);
        Assert.Equal(userId, entry.ActorId);
        Assert.Equal(UtcTime, entry.OccurredAt);
        Assert.Equal("TestAuditedItem", entry.EntityType);
        Assert.Equal(item.Id, entry.EntityId);
        Assert.Equal(AuditOperation.Created, entry.Operation);
        Assert.Null(entry.BeforeState);
        Assert.Contains("\"Name\":\"Item\"", entry.AfterState);
    }

    [Fact]
    public async Task Updated_RecordsOnlyTheChangedProperties()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId, ActorType.User, Guid.CreateVersion7());
        var item = new TestAuditedItem("Item", "secret-value");
        context.Set<TestAuditedItem>().Add(item);
        await context.SaveChangesAsync();
        item.Rename("Renamed");
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync(e => e.Operation == AuditOperation.Updated);
        Assert.Equal("{\"Name\":\"Item\"}", entry.BeforeState);
        Assert.Equal("{\"Name\":\"Renamed\"}", entry.AfterState);
    }

    [Fact]
    public async Task Deactivated_IsAnUpdateOfIsActive()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId, ActorType.User, Guid.CreateVersion7());
        var item = new TestAuditedItem("Item", "secret-value");
        context.Set<TestAuditedItem>().Add(item);
        await context.SaveChangesAsync();
        item.Deactivate();
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync(e => e.Operation == AuditOperation.Updated);
        Assert.Equal("{\"IsActive\":true}", entry.BeforeState);
        Assert.Equal("{\"IsActive\":false}", entry.AfterState);
    }

    [Fact]
    public async Task SensitiveValue_IsMasked()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId, ActorType.User, Guid.CreateVersion7());
        context.Set<TestAuditedItem>().Add(new TestAuditedItem("Item", "secret-value"));
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync();
        Assert.Contains("\"Secret\":\"***\"", entry.AfterState);
        Assert.DoesNotContain("secret-value", entry.AfterState);
    }

    [Fact]
    public async Task CatalogEntity_IsAuditedOnPlatformWithoutTenant()
    {
        await using var context = CreateContext(null, ActorType.Anonymous, null);
        var item = new TestAuditedCatalogItem("Item", "secret-value");
        context.Set<TestAuditedCatalogItem>().Add(item);
        await context.SaveChangesAsync();
        var entry = await context.Set<PlatformAuditEntry>().SingleAsync();
        Assert.Null(entry.TenantId);
        Assert.Equal(ActorType.Anonymous, entry.ActorType);
        Assert.Null(entry.ActorId);
        Assert.Equal(item.Id, entry.EntityId);
        Assert.Equal(AuditOperation.Created, entry.Operation);
        Assert.Empty(context.ChangeTracker.Entries<TenantAuditEntry>());
    }

    [Fact]
    public async Task NonAuditableEntity_LeavesNoEntry()
    {
        var tenantId = Guid.CreateVersion7();
        await using var context = CreateContext(tenantId, ActorType.User, Guid.CreateVersion7());
        context.Set<TestTenantItem>().Add(new TestTenantItem("Item"));
        await context.SaveChangesAsync();
        Assert.Empty(await context.Set<TenantAuditEntry>().ToListAsync());
        Assert.Empty(await context.Set<PlatformAuditEntry>().ToListAsync());
    }
}
