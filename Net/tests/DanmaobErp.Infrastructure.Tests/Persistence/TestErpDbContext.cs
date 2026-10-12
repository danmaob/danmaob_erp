using DanmaobErp.Application.Identity;
using DanmaobErp.Application.Tenancy;
using DanmaobErp.Application.Time;
using DanmaobErp.Domain.Auditing;
using DanmaobErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestErpDbContext : ErpDbContext
{
    public TestErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext) : base(options, tenantContext, new FixedCurrentActor(ActorType.Anonymous, null), new FixedBusinessClock(new DateTimeOffset(2026, 10, 11, 18, 30, 0, TimeSpan.Zero)))
    {
    }

    public TestErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext, ICurrentActor currentActor, IBusinessClock businessClock) : base(options, tenantContext, currentActor, businessClock)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestItem>(entity =>
        {
            entity.ToTable("TestItems", DatabaseSchemas.Erp);
            entity.Property(e => e.Id);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.Name).HasMaxLength(100);
        });
        modelBuilder.Entity<TestTenantItem>(entity =>
        {
            entity.ToTable("TestTenantItems", DatabaseSchemas.Erp);
            entity.Property(e => e.Id);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.Name).HasMaxLength(100);
        });
        modelBuilder.Entity<TestAuditedItem>(entity =>
        {
            entity.ToTable("TestAuditedItems", DatabaseSchemas.Erp);
            entity.Property(e => e.Id);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Secret).HasMaxLength(100);
        });
        modelBuilder.Entity<TestAuditedCatalogItem>(entity =>
        {
            entity.ToTable("TestAuditedCatalogItems", DatabaseSchemas.Platform);
            entity.Property(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Secret).HasMaxLength(100);
        });
        base.OnModelCreating(modelBuilder);
    }
}
