namespace DanmaobErp.Application.Tenancy;

public sealed class TenantNotResolvedException : Exception
{
    public TenantNotResolvedException() : base("No tenant is resolved for the current request. Operational data cannot be accessed without a tenant.")
    {
    }
}
