using System.Net.Http.Json;
using System.Text.Json;
using DanmaobErp.Api.Tests.Errors.TestSupport;

namespace DanmaobErp.Api.Tests.Errors;

public sealed class ErrorContractTests
{
    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var text = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    [Fact]
    public async Task Unhandled_Returns500WithoutInternalDetails()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/test/errors/unhandled");
        var text = await response.Content.ReadAsStringAsync();
        var problem = await ReadProblemAsync(response);

        Assert.Equal(500, (int)response.StatusCode);
        Assert.Equal("internal_error", problem.GetProperty("code").GetString());
        Assert.True(problem.TryGetProperty("traceId", out _));
        Assert.DoesNotContain("Sensitive internal detail", text);
        Assert.DoesNotContain("InvalidOperationException", text);
    }

    [Theory]
    [InlineData("/test/errors/tenant")]
    [InlineData("/test/errors/wrapped-tenant")]
    public async Task TenantNotResolved_Returns401(string route)
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.GetAsync(route);
        var problem = await ReadProblemAsync(response);

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("tenant_not_resolved", problem.GetProperty("code").GetString());
        Assert.Equal("No se identificó la empresa de tu sesión. Inicia sesión de nuevo.", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CrossTenantWrite_Returns403()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/test/errors/cross-tenant");
        var problem = await ReadProblemAsync(response);

        Assert.Equal(403, (int)response.StatusCode);
        Assert.Equal("cross_tenant_write", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task BusinessValidation_Returns422WithFieldErrors()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/test/errors/business-validation");
        var problem = await ReadProblemAsync(response);

        Assert.Equal(422, (int)response.StatusCode);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.Equal("Este campo es obligatorio.", problem.GetProperty("errors").GetProperty("name")[0].GetString());
    }

    [Fact]
    public async Task MissingName_Returns422WithRequiredMessage()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/test/errors/input", new { quantity = 1 });
        var problem = await ReadProblemAsync(response);

        Assert.Equal(422, (int)response.StatusCode);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.Equal("Este campo es obligatorio.", problem.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task LongName_Returns422WithMaxLengthMessage()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/test/errors/input", new { name = "abcdefgh", quantity = 1 });
        var problem = await ReadProblemAsync(response);

        Assert.Equal(422, (int)response.StatusCode);
        Assert.Equal("Este campo debe tener como máximo 5 caracteres.", problem.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task InvalidQuantity_Returns422WithInvalidValueMessage()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/test/errors/input", new { name = "abc", quantity = "not-a-number" });
        var text = await response.Content.ReadAsStringAsync();
        var problem = await ReadProblemAsync(response);

        Assert.Equal(422, (int)response.StatusCode);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.Equal("El valor de este campo no es válido.", problem.GetProperty("errors").GetProperty("$.quantity")[0].GetString());
        Assert.DoesNotContain("could not be converted", text);
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/does-not-exist");
        var problem = await ReadProblemAsync(response);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("not_found", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task WrongMethod_Returns405()
    {
        using var factory = TestApi.Create();
        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/test/errors/unhandled");
        var problem = await ReadProblemAsync(response);

        Assert.Equal(405, (int)response.StatusCode);
        Assert.Equal("method_not_allowed", problem.GetProperty("code").GetString());
    }
}
