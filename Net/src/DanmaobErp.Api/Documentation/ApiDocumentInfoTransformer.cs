using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace DanmaobErp.Api.Documentation;

public sealed class ApiDocumentInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var info = new OpenApiInfo();
        info.Title = "DANMAOB ERP API";
        info.Version = "v1";
        info.Description = "REST API of DANMAOB ERP. Error responses use the ProblemDetails format (application/problem+json).";
        document.Info = info;
        return Task.CompletedTask;
    }
}
