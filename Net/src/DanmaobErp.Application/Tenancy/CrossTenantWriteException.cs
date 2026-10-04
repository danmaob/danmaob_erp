namespace DanmaobErp.Application.Tenancy;

public sealed class CrossTenantWriteException : Exception
{
    public CrossTenantWriteException(string entityTypeName) : base($"The entity {entityTypeName} belongs to a different tenant than the current request. No changes were saved.")
    {
    }
}
