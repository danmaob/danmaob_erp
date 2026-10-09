using System.Diagnostics;
using DanmaobErp.Application.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobErp.Api.Errors;

public static class InvalidModelStateResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer>();
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in context.ModelState)
        {
            if (entry.Value.Errors.Count == 0)
            {
                continue;
            }
            errors[entry.Key] = entry.Value.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? localizer[MessageKeys.Validation.InvalidValue].Value : e.ErrorMessage).ToArray();
        }
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = localizer[MessageKeys.Errors.ValidationFailed].Value,
            Extensions =
            {
                ["code"] = ApiErrorCodes.ValidationFailed,
                ["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
            }
        };
        var result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }
}
