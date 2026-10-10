using DanmaobErp.Api.Routing;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobErp.Api.Tests.Documentation;

[ApiController]
[Route(ApiRoutes.V1 + "/test/documentation")]
public sealed class DocumentationTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
