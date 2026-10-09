using DanmaobErp.Application.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobErp.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemDetailsService, IStringLocalizer localizer, ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _localizer = localizer;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var error = ApiExceptionMapper.Map(exception);

        if (error.StatusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else if (error.StatusCode == StatusCodes.Status403Forbidden)
        {
            _logger.LogWarning("Security event: a write to data of another tenant was rejected on {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else if (error.StatusCode == StatusCodes.Status401Unauthorized)
        {
            _logger.LogWarning("Tenant not resolved on {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogInformation("Request rejected with {StatusCode} {Code} on {Method} {Path}.", error.StatusCode, error.Code, httpContext.Request.Method, httpContext.Request.Path);
        }

        ProblemDetails problem;

        if (exception is ValidationFailedException validationException)
        {
            problem = new ValidationProblemDetails(TranslateErrors(validationException.Errors));
        }
        else
        {
            problem = new ProblemDetails();
        }

        problem.Status = error.StatusCode;

        problem.Title = _localizer[error.MessageKey].Value;

        problem.Extensions["code"] = error.Code;

        httpContext.Response.StatusCode = error.StatusCode;

        var problemContext = new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem };

        return await _problemDetailsService.TryWriteAsync(problemContext);
    }

    private Dictionary<string, string[]> TranslateErrors(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
    {
        var translated = new Dictionary<string, string[]>();

        foreach (var entry in errors)
        {
            translated[entry.Key] = entry.Value.Select(key => _localizer[key].Value).ToArray();
        }

        return translated;
    }
}
