using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DanmaobErp.Api.Documentation;

public sealed class ProblemDetailsResponseTypesSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json"));
        options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json"));
    }
}
