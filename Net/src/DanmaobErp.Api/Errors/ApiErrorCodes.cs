namespace DanmaobErp.Api.Errors;

public static class ApiErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string TenantNotResolved = "tenant_not_resolved";
    public const string CrossTenantWrite = "cross_tenant_write";
    public const string NotFound = "not_found";
    public const string MethodNotAllowed = "method_not_allowed";
    public const string InternalError = "internal_error";
}
