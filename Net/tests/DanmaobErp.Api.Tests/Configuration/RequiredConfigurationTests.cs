using DanmaobErp.Api.Configuration;
using Microsoft.Extensions.Configuration;

namespace DanmaobErp.Api.Tests.Configuration;

public sealed class RequiredConfigurationTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(values);

        return builder.Build();
    }

    [Fact]
    public void EnsurePresent_Throws_WhenCorsOriginsAreMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        var exception = Assert.Throws<InvalidOperationException>(
            () => RequiredConfiguration.EnsurePresent(configuration, RequiredConfigurationKeys.All)
        );

        Assert.Equal("Missing required configuration. Set these environment variables: Cors__AllowedOrigins, ConnectionStrings__Erp", exception.Message);
    }

    [Fact]
    public void EnsurePresent_ListsOnlyMissingVariables_WithoutValues()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Present:Key"] = "secret-value-123"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => RequiredConfiguration.EnsurePresent(configuration, new[] { "First:Key", "Present:Key", "Second:Key" })
        );

        Assert.Equal("Missing required configuration. Set these environment variables: First__Key, Second__Key", exception.Message);
        Assert.DoesNotContain("secret-value-123", exception.Message);
    }

    [Fact]
    public void EnsurePresent_Throws_WhenValueIsWhitespace()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins"] = "   "
        });

        Assert.Throws<InvalidOperationException>(
            () => RequiredConfiguration.EnsurePresent(configuration, RequiredConfigurationKeys.All)
        );
    }

    [Fact]
    public void EnsurePresent_DoesNotThrow_WhenAllKeysHaveValues()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins"] = "https://app.example.com",
            ["ConnectionStrings:Erp"] = "Server=localhost;Database=DanmaobErpTests;TrustServerCertificate=True"
        });

        Assert.Null(Record.Exception(() => RequiredConfiguration.EnsurePresent(configuration, RequiredConfigurationKeys.All)));
    }
}
