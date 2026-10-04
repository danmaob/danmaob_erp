using DanmaobErp.Application.Tenancy;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class FixedTenantContext : ITenantContext
{
    private readonly Guid? _tenantId;

    public FixedTenantContext(Guid? tenantId)
    {
        _tenantId = tenantId;
    }

    public Guid RequireTenantId()
    {
        if (_tenantId is null)
        {
            throw new TenantNotResolvedException();
        }
        return _tenantId.Value;
    }
}
