using DanmaobErp.Application.Tenancy;
using DanmaobErp.Domain.Auditing;
using DanmaobErp.Infrastructure.Auditing;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Auditing;

public sealed class AuditImmutabilityTests
{
    private static TestErpDbContext CreateContext(Guid? tenantId)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new TestErpDbContext(builder.Options, new FixedTenantContext(tenantId), new FixedCurrentActor(ActorType.Anonymous, null), new FixedBusinessClock(new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task ModifyingAnAuditEntry_Throws()
    {
        await using var context = CreateContext(Guid.CreateVersion7());
        context.Set<TestAuditedItem>().Add(new TestAuditedItem("Item", "secret-value"));
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync();
        context.Entry(entry).State = EntityState.Modified;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync());
        Assert.Equal(AuditTrail.ImmutableMessage, exception.Message);
    }

    [Fact]
    public async Task DeletingAnAuditEntry_Throws()
    {
        await using var context = CreateContext(Guid.CreateVersion7());
        context.Set<TestAuditedItem>().Add(new TestAuditedItem("Item", "secret-value"));
        await context.SaveChangesAsync();
        var entry = await context.Set<TenantAuditEntry>().SingleAsync();
        context.Set<TenantAuditEntry>().Remove(entry);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync());
        Assert.Equal(AuditTrail.ImmutableMessage, exception.Message);
    }

    [Fact]
    public async Task SaveRejectedByTheTenantGuard_LeavesNoAuditEntry()
    {
        await using var context = CreateContext(null);
        context.Set<TestAuditedItem>().Add(new TestAuditedItem("Item", "secret-value"));
        await Assert.ThrowsAsync<TenantNotResolvedException>(async () => await context.SaveChangesAsync());
        Assert.Empty(context.ChangeTracker.Entries<AuditEntry>());
    }
}
