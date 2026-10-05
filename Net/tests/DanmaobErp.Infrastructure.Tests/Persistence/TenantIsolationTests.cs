using DanmaobErp.Application.Tenancy;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TenantIsolationTests
{
    private static DbContextOptions<ErpDbContext> CreateOptions(string databaseName)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(databaseName);
        return builder.Options;
    }

    private static async Task SeedAsync(DbContextOptions<ErpDbContext> options, Guid tenantId, string prefix)
    {
        await using var context = new TestErpDbContext(options, new FixedTenantContext(tenantId));
        var active = new TestTenantItem($"{prefix} active");
        var inactive = new TestTenantItem($"{prefix} inactive");
        inactive.Deactivate();
        context.Set<TestTenantItem>().Add(active);
        context.Set<TestTenantItem>().Add(inactive);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Query_ReturnsOnlyCurrentTenantItems()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options, tenantA, "A");
        await SeedAsync(options, tenantB, "B");
        await using var context = new TestErpDbContext(options, new FixedTenantContext(tenantA));
        var items = await context.Set<TestTenantItem>().ToListAsync();
        var item = Assert.Single(items);
        Assert.Equal("A active", item.Name);
        Assert.Equal(tenantA, item.TenantId);
    }

    [Fact]
    public async Task IgnoringSoftDeleteFilter_KeepsTenantFilter()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options, tenantA, "A");
        await SeedAsync(options, tenantB, "B");
        await using var context = new TestErpDbContext(options, new FixedTenantContext(tenantA));
        var items = await context.Set<TestTenantItem>()
            .IgnoreQueryFilters(new[] { SoftDeleteQueryFilter.FilterName })
            .ToListAsync();
        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(tenantA, i.TenantId));
    }

    [Fact]
    public async Task Query_Throws_WhenTenantIsNotResolved()
    {
        var tenantA = Guid.CreateVersion7();
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options, tenantA, "A");
        await using var context = new TestErpDbContext(options, new FixedTenantContext(null));
        var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await context.Set<TestTenantItem>().ToListAsync());
        Assert.True(exception is TenantNotResolvedException || exception.InnerException is TenantNotResolvedException);
    }

    [Fact]
    public async Task CatalogQuery_Works_WhenTenantIsNotResolved()
    {
        var options = CreateOptions(Guid.NewGuid().ToString());
        await using var context = new TestErpDbContext(options, new FixedTenantContext(null));
        context.Set<TestItem>().Add(new TestItem("Catalog"));
        await context.SaveChangesAsync();
        var items = await context.Set<TestItem>().ToListAsync();
        var item = Assert.Single(items);
        Assert.Equal("Catalog", item.Name);
    }
}
