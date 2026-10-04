using DanmaobErp.Application.Tenancy;

namespace DanmaobErp.Api.Tenancy;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid RequireTenantId()
    {
        var claimValue = _httpContextAccessor.HttpContext?.User.FindFirst(TenantClaimTypes.TenantId)?.Value;

        if (Guid.TryParse(claimValue, out var tenantId) && tenantId != Guid.Empty)
        {
            return tenantId;
        }

        throw new TenantNotResolvedException();
    }
}
