namespace DanmaobErp.Application.Localization;

public static class MessageKeys
{
    public static class Errors
    {
        public const string ValidationFailed = "Errors.ValidationFailed";
        public const string TenantNotResolved = "Errors.TenantNotResolved";
        public const string CrossTenantWrite = "Errors.CrossTenantWrite";
        public const string NotFound = "Errors.NotFound";
        public const string MethodNotAllowed = "Errors.MethodNotAllowed";
        public const string InternalError = "Errors.InternalError";
    }

    public static class Validation
    {
        public const string Required = "Validation.Required";
        public const string MaxLength = "Validation.MaxLength";
        public const string InvalidValue = "Validation.InvalidValue";
    }
}
