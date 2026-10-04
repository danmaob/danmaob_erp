using DanmaobErp.Application.Tenancy;
using DanmaobErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestErpDbContext : ErpDbContext
{
    public TestErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext) : base(options, tenantContext)
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
        base.OnModelCreating(modelBuilder);
    }
}
