using DanmaobErp.Application.Errors;
using DanmaobErp.Application.Localization;
using DanmaobErp.Application.Tenancy;

namespace DanmaobErp.Api.Errors;

public static class ApiExceptionMapper
{
    public static ApiError Map(Exception exception)
    {
        if (exception is ValidationFailedException)
        {
            return new ApiError(StatusCodes.Status422UnprocessableEntity, ApiErrorCodes.ValidationFailed, MessageKeys.Errors.ValidationFailed);
        }

        if (exception is TenantNotResolvedException || exception.InnerException is TenantNotResolvedException)
        {
            return new ApiError(StatusCodes.Status401Unauthorized, ApiErrorCodes.TenantNotResolved, MessageKeys.Errors.TenantNotResolved);
        }

        if (exception is CrossTenantWriteException)
        {
            return new ApiError(StatusCodes.Status403Forbidden, ApiErrorCodes.CrossTenantWrite, MessageKeys.Errors.CrossTenantWrite);
        }

        return new ApiError(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalError, MessageKeys.Errors.InternalError);
    }

    public static ApiError MapStatusCode(int statusCode)
    {
        if (statusCode == StatusCodes.Status404NotFound)
        {
            return new ApiError(statusCode, ApiErrorCodes.NotFound, MessageKeys.Errors.NotFound);
        }

        if (statusCode == StatusCodes.Status405MethodNotAllowed)
        {
            return new ApiError(statusCode, ApiErrorCodes.MethodNotAllowed, MessageKeys.Errors.MethodNotAllowed);
        }

        if (statusCode == StatusCodes.Status401Unauthorized)
        {
            return new ApiError(statusCode, ApiErrorCodes.TenantNotResolved, MessageKeys.Errors.TenantNotResolved);
        }

        if (statusCode == StatusCodes.Status403Forbidden)
        {
            return new ApiError(statusCode, ApiErrorCodes.CrossTenantWrite, MessageKeys.Errors.CrossTenantWrite);
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            return new ApiError(statusCode, ApiErrorCodes.InternalError, MessageKeys.Errors.InternalError);
        }

        return new ApiError(statusCode, ApiErrorCodes.ValidationFailed, MessageKeys.Errors.ValidationFailed);
    }
}
