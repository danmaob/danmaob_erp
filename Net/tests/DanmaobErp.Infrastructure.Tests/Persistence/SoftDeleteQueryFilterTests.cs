using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class SoftDeleteQueryFilterTests
{
    private static DbContextOptions<ErpDbContext> CreateOptions(string databaseName)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(databaseName);
        return builder.Options;
    }

    private static async Task SeedAsync(DbContextOptions<ErpDbContext> options)
    {
        await using var context = new TestErpDbContext(options);
        var active = new TestItem("Active");
        var inactive = new TestItem("Inactive");
        inactive.Deactivate();
        context.Set<TestItem>().Add(active);
        context.Set<TestItem>().Add(inactive);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task ActiveSet_ExcludesDeactivatedItems()
    {
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options);
        await using var context = new TestErpDbContext(options);
        var items = await context.Set<TestItem>().ToListAsync();
        var item = Assert.Single(items);
        Assert.Equal("Active", item.Name);
    }

    [Fact]
    public async Task IgnoringSoftDeleteFilter_ReturnsDeactivatedItemsToo()
    {
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options);
        await using var context = new TestErpDbContext(options);
        var items = await context.Set<TestItem>()
            .IgnoreQueryFilters(new[] { SoftDeleteQueryFilter.FilterName })
            .ToListAsync();
        Assert.Equal(2, items.Count);
        var deactivated = Assert.Single(items, i => i.IsActive is false);
        Assert.Equal("Inactive", deactivated.Name);
    }

    [Fact]
    public async Task ReactivatedItem_ReturnsToActiveSet()
    {
        var options = CreateOptions(Guid.NewGuid().ToString());
        await SeedAsync(options);
        await using var updateContext = new TestErpDbContext(options);
        var item = await updateContext.Set<TestItem>()
            .IgnoreQueryFilters(new[] { SoftDeleteQueryFilter.FilterName })
            .SingleAsync(i => i.Name == "Inactive");
        item.Reactivate();
        await updateContext.SaveChangesAsync();
        await using var queryContext = new TestErpDbContext(options);
        var items = await queryContext.Set<TestItem>().ToListAsync();
        Assert.Equal(2, items.Count);
    }
}
