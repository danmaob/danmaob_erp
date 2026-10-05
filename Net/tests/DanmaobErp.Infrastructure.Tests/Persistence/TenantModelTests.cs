using DanmaobErp.Domain.Common;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TenantModelTests
{
    private static DbContextOptions<ErpDbContext> CreateOptions(string databaseName)
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(databaseName);
        return builder.Options;
    }

    [Fact]
    public void EveryTenantOwnedEntity_HasTenantFilter()
    {
        using var context = new TestErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(null));
        var model = context.GetService<IDesignTimeModel>().Model;
        var tenantOwnedTypes = model.GetEntityTypes()
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType) && t.BaseType is null)
            .ToList();
        Assert.NotEmpty(tenantOwnedTypes);
        foreach (var entityType in tenantOwnedTypes)
        {
            Assert.NotNull(entityType.FindDeclaredQueryFilter(TenantQueryFilter.FilterName));
        }
    }

    [Fact]
    public void EveryEntityOfTheRealModel_IsTenantOwnedOrPlatformCatalog()
    {
        using var context = new ErpDbContext(CreateOptions(Guid.NewGuid().ToString()), new FixedTenantContext(null));
        var model = context.GetService<IDesignTimeModel>().Model;
        var offenders = model.GetEntityTypes()
            .Where(t => !t.IsOwned() && typeof(ITenantOwned).IsAssignableFrom(t.ClrType) is false && PlatformCatalog.EntityTypes.Contains(t.ClrType) is false)
            .Select(t => t.ClrType.Name)
            .ToList();
        Assert.Empty(offenders);
    }
}
