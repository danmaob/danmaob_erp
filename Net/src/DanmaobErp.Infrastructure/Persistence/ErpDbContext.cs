using DanmaobErp.Application.Identity;
using DanmaobErp.Application.Tenancy;
using DanmaobErp.Application.Time;
using DanmaobErp.Infrastructure.Auditing;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Persistence;

public class ErpDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentActor _currentActor;
    private readonly IBusinessClock _businessClock;

    public ErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext, ICurrentActor currentActor, IBusinessClock businessClock) : base(options)
    {
        _tenantContext = tenantContext;
        _currentActor = currentActor;
        _businessClock = businessClock;
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
        TenantQueryFilter.Apply(modelBuilder, this);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySaveRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplySaveRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplySaveRules()
    {
        AuditTrail.EnsureImmutable(ChangeTracker);
        TenantWriteGuard.Apply(ChangeTracker, _tenantContext);
        AuditTrail.Capture(ChangeTracker, _currentActor, _businessClock);
    }
}
