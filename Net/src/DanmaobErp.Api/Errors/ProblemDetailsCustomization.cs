using Microsoft.Extensions.Localization;

namespace DanmaobErp.Api.Errors;

public static class ProblemDetailsCustomization
{
    public static void Apply(ProblemDetailsContext context)
    {
        if (context.ProblemDetails.Extensions.ContainsKey("code"))
        {
            return;
        }

        var statusCode = context.ProblemDetails.Status ?? context.HttpContext.Response.StatusCode;
        var error = ApiExceptionMapper.MapStatusCode(statusCode);
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer>();
        context.ProblemDetails.Title = localizer[error.MessageKey].Value;
        context.ProblemDetails.Extensions["code"] = error.Code;
    }
}
