using DanmaobErp.Api.Configuration;

namespace DanmaobErp.Api.Tests.Configuration;

public sealed class CorsOriginsParserTests
{
    [Fact]
    public void Parse_ReturnsOrigins_InInputOrder()
    {
        var origins = CorsOriginsParser.Parse("https://app.example.com, http://localhost:5173");

        Assert.Equal(new[] { "https://app.example.com", "http://localhost:5173" }, origins);
    }

    [Fact]
    public void Parse_IgnoresEmptyEntries()
    {
        var origins = CorsOriginsParser.Parse("https://app.example.com,,  ,");

        Assert.Equal(new[] { "https://app.example.com" }, origins);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ftp://files.example.com")]
    [InlineData("app.example.com")]
    [InlineData("https://app.example.com/")]
    [InlineData("https://app.example.com/path")]
    [InlineData("https://ok.example.com,not-a-url")]
    public void Parse_Throws_ForInvalidValue(string? rawValue)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => CorsOriginsParser.Parse(rawValue));
        Assert.Equal("Invalid configuration: Cors__AllowedOrigins must contain one or more absolute http or https origins, separated by commas, without paths or trailing slashes.", exception.Message);
    }
}
