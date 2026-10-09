using DanmaobErp.Application.Errors;
using DanmaobErp.Application.Localization;
using DanmaobErp.Application.Tenancy;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobErp.Api.Tests.Errors.TestSupport;

[ApiController]
[Route("test/errors")]
public sealed class ErrorsTestController : ControllerBase
{
    [HttpGet("unhandled")]
    public IActionResult Unhandled()
    {
        throw new InvalidOperationException("Sensitive internal detail 12345");
    }

    [HttpGet("tenant")]
    public IActionResult Tenant()
    {
        throw new TenantNotResolvedException();
    }

    [HttpGet("wrapped-tenant")]
    public IActionResult WrappedTenant()
    {
        throw new InvalidOperationException("Wrapped", new TenantNotResolvedException());
    }

    [HttpGet("cross-tenant")]
    public IActionResult CrossTenant()
    {
        throw new CrossTenantWriteException("Customer");
    }

    [HttpGet("business-validation")]
    public IActionResult BusinessValidation()
    {
        var errors = new Dictionary<string, IReadOnlyList<string>>();
        errors["name"] = new string[] { MessageKeys.Validation.Required };
        throw new ValidationFailedException(errors);
    }

    [HttpPost("input")]
    public IActionResult Input(ErrorsTestInput input)
    {
        return Ok();
    }
}
