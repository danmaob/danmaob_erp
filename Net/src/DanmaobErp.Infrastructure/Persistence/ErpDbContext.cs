using DanmaobErp.Application.Tenancy;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Persistence;

public class ErpDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public Guid CurrentTenantId
    {
        get
        {
            return _tenantContext.RequireTenantId();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);
        EntityKeyConfiguration.Apply(modelBuilder);
        SoftDeleteQueryFilter.Apply(modelBuilder);
    }
}
