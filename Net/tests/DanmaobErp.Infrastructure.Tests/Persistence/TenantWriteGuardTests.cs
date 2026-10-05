using DanmaobErp.Application.Tenancy;
using DanmaobErp.Domain.Common;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TenantWriteGuardTests
{
    private static DbContextOptions<ErpDbContext> CreateOptions(string databaseName)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(databaseName);
        return builder.Options;
    }

    [Fact]
    public async Task NewItem_ReceivesCurrentTenant()
    {
        var tenantA = Guid.CreateVersion7();
        await using var context = new TestErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(tenantA));
        var item = new TestTenantItem("New");
        context.Set<TestTenantItem>().Add(item);
        await context.SaveChangesAsync();
        Assert.Equal(tenantA, item.TenantId);
    }

    [Fact]
    public void SaveChanges_Synchronous_AlsoAssignsTenant()
    {
        var tenantA = Guid.CreateVersion7();
        using var context = new TestErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(tenantA));
        var item = new TestTenantItem("New");
        context.Set<TestTenantItem>().Add(item);
        context.SaveChanges();
        Assert.Equal(tenantA, item.TenantId);
    }

    [Fact]
    public async Task NewItem_FromAnotherTenant_IsRejected()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        await using var context = new TestErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(tenantA));
        var item = new TestTenantItem("New");
        context.Set<TestTenantItem>().Add(item);
        context.Entry(item).Property(nameof(ITenantOwned.TenantId)).CurrentValue = tenantB;
        await Assert.ThrowsAsync<CrossTenantWriteException>(async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task ChangingTenantOfExistingItem_IsRejected()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        var options = CreateOptions(Guid.NewGuid().ToString());
        await using var seedContext = new TestErpDbContext(options, new FixedTenantContext(tenantA));
        seedContext.Set<TestTenantItem>().Add(new TestTenantItem("Existing"));
        await seedContext.SaveChangesAsync();
        await using var context = new TestErpDbContext(options, new FixedTenantContext(tenantA));
        var item = await context.Set<TestTenantItem>().SingleAsync();
        context.Entry(item).Property(nameof(ITenantOwned.TenantId)).CurrentValue = tenantB;
        await Assert.ThrowsAsync<CrossTenantWriteException>(async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task ModifyingItemOfAnotherTenant_IsRejected()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        var options = CreateOptions(Guid.NewGuid().ToString());
        await using var seedContext = new TestErpDbContext(options, new FixedTenantContext(tenantB));
        seedContext.Set<TestTenantItem>().Add(new TestTenantItem("Other"));
        await seedContext.SaveChangesAsync();
        await using var context = new TestErpDbContext(options, new FixedTenantContext(tenantA));
        var item = await context.Set<TestTenantItem>()
            .IgnoreQueryFilters(new[] { TenantQueryFilter.FilterName })
            .SingleAsync();
        item.Deactivate();
        await Assert.ThrowsAsync<CrossTenantWriteException>(async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task SavingTenantOwnedItem_Throws_WhenTenantIsNotResolved()
    {
        await using var context = new TestErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(null));
        context.Set<TestTenantItem>().Add(new TestTenantItem("New"));
        await Assert.ThrowsAsync<TenantNotResolvedException>(async () => await context.SaveChangesAsync());
    }
}
