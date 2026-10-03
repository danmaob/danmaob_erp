using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DanmaobErp.Api.Tests;

public sealed class ApiHostTests
{
    private const string ConfiguredOrigin = "https://app.example.com";
    private const string UnconfiguredOrigin = "https://unknown.example.com";

    private static async Task<HttpResponseMessage> SendPreflightAsync(string requestOrigin)
    {
        using var baseFactory = new WebApplicationFactory<Program>();
        var factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors:AllowedOrigins", ConfiguredOrigin);
        });

        var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", requestOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        return await client.SendAsync(request);
    }

    [Fact]
    public async Task Preflight_FromConfiguredOrigin_ReturnsAllowOriginHeader()
    {
        var response = await SendPreflightAsync(ConfiguredOrigin);

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Equal(ConfiguredOrigin, Assert.Single(values));
    }

    [Fact]
    public async Task Preflight_FromUnconfiguredOrigin_HasNoAllowOriginHeader()
    {
        var response = await SendPreflightAsync(UnconfiguredOrigin);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var baseFactory = new WebApplicationFactory<Program>();
        var factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors:AllowedOrigins", ConfiguredOrigin);
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Startup_Fails_WhenCorsOriginsAreMissing()
    {
        using var baseFactory = new WebApplicationFactory<Program>();

        var exception = Record.Exception(() => baseFactory.CreateClient());

        Assert.NotNull(exception);
        Assert.Contains("Cors__AllowedOrigins", exception.ToString());
    }
}
