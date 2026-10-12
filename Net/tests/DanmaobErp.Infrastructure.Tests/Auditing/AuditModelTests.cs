using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Common;
using DanmaobErp.Infrastructure.Auditing;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DanmaobErp.Infrastructure.Tests.Auditing;

public sealed class AuditModelTests
{
    private static ErpDbContext CreateRealContext()
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new ErpDbContext(builder.Options, new FixedTenantContext(null), new FixedCurrentActor(ActorType.Anonymous, null), new FixedBusinessClock(new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void EveryEntityOfTheRealModel_IsAuditableOrExcluded()
    {
        using var context = CreateRealContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var offenders = model.GetEntityTypes().Where(t => typeof(IAuditable).IsAssignableFrom(t.ClrType) is false && AuditExclusions.EntityTypes.Contains(t.ClrType) is false).Select(t => t.ClrType.Name).ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void AuditTables_AreInTheirSchemas()
    {
        using var context = CreateRealContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(DatabaseSchemas.Erp, model.FindEntityType(typeof(TenantAuditEntry))?.GetSchema());
        Assert.Equal(DatabaseSchemas.Platform, model.FindEntityType(typeof(PlatformAuditEntry))?.GetSchema());
    }
}
