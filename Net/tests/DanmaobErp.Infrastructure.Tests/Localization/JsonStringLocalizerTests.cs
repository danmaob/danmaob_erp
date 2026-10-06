using System.Globalization;
using DanmaobErp.Infrastructure.Localization;
using Microsoft.Extensions.Logging.Abstractions;

namespace DanmaobErp.Infrastructure.Tests.Localization;

public sealed class JsonStringLocalizerTests
{
    private const string SpanishJson = "{\"Common.Greeting\":\"Hola\",\"Validation.MaxLength\":\"Como maximo {1} caracteres.\"}";

    private static JsonStringLocalizer CreateLocalizer()
    {
        var settings = new LocalizationSettings(MessageDirectory.Create(SpanishJson), "es", new[] { "es" });
        var catalog = new MessageCatalog(settings, NullLogger<MessageCatalog>.Instance);
        return new JsonStringLocalizer(catalog, settings, NullLogger<JsonStringLocalizer>.Instance);
    }

    [Fact]
    public void Indexer_ReturnsMessage_ForCurrentUICulture()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("es");
        var localizer = CreateLocalizer();
        var message = localizer["Common.Greeting"];
        Assert.Equal("Hola", message.Value);
        Assert.False(message.ResourceNotFound);
    }

    [Fact]
    public void Indexer_FallsBackToDefaultCulture_WhenCultureHasNoDictionary()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        var localizer = CreateLocalizer();
        Assert.Equal("Hola", localizer["Common.Greeting"].Value);
    }

    [Fact]
    public void Indexer_ReturnsKey_WhenMessageIsMissing()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("es");
        var localizer = CreateLocalizer();
        var message = localizer["Common.Missing"];
        Assert.Equal("Common.Missing", message.Value);
        Assert.True(message.ResourceNotFound);
    }

    [Fact]
    public void Indexer_FormatsPositionalArguments()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("es");
        var localizer = CreateLocalizer();
        Assert.Equal("Como maximo 5 caracteres.", localizer["Validation.MaxLength", "Name", 5].Value);
    }
}
