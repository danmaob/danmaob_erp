using System.Text.Json;
using DanmaobErp.Api.Tests.Errors.TestSupport;
using Microsoft.AspNetCore.Hosting;

namespace DanmaobErp.Api.Tests.Documentation;

public sealed class OpenApiDocumentTests
{
    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        string text = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    [Fact]
    public async Task Document_DeclaresTitleAndVersion()
    {
        using var factory = TestApi.Create();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(200, (int)response.StatusCode);
        var root = await ReadJsonAsync(response);
        Assert.Equal("DANMAOB ERP API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());
    }

    [Fact]
    public async Task Document_ListsVersionedEndpointWithErrorResponses()
    {
        using var factory = TestApi.Create();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        var root = await ReadJsonAsync(response);
        var responses = root.GetProperty("paths").GetProperty("/api/v1/test/documentation").GetProperty("get").GetProperty("responses");
        Assert.True(responses.TryGetProperty("422", out _));
        Assert.True(responses.TryGetProperty("500", out _));
    }

    [Fact]
    public async Task Document_IsNotPublished_OutsideDevelopment()
    {
        using var baseFactory = TestApi.Create();
        using var factory = baseFactory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(404, (int)response.StatusCode);
    }
}
