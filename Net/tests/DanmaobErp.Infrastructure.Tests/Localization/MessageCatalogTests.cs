using DanmaobErp.Infrastructure.Localization;
using Microsoft.Extensions.Logging.Abstractions;

namespace DanmaobErp.Infrastructure.Tests.Localization;

public sealed class MessageCatalogTests
{
    private const string ValidJson = "{\"Common.Greeting\":\"Hola\"}";

    private static MessageCatalog CreateCatalog(string path)
    {
        var settings = new LocalizationSettings(path, "es", new[] { "es" });
        return new MessageCatalog(settings, NullLogger<MessageCatalog>.Instance);
    }

    [Fact]
    public void Find_ReturnsMessage_FromDefaultDictionary()
    {
        using var catalog = CreateCatalog(MessageDirectory.Create(ValidJson));
        Assert.Equal("Hola", catalog.Find("es", "Common.Greeting"));
    }

    [Fact]
    public void Constructor_Throws_WhenDirectoryDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), "danmaob-erp-tests", Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => CreateCatalog(path));
    }

    [Fact]
    public void Constructor_Throws_WhenDefaultDictionaryIsMissing()
    {
        var path = MessageDirectory.Create(ValidJson);
        File.Delete(Path.Combine(path, "es.json"));
        Assert.Throws<InvalidOperationException>(() => CreateCatalog(path));
    }

    [Fact]
    public void Reload_ReadsChangedMessages()
    {
        var path = MessageDirectory.Create(ValidJson);
        using var catalog = CreateCatalog(path);
        File.WriteAllText(Path.Combine(path, "es.json"), "{\"Common.Greeting\":\"Buenos dias\"}");
        catalog.Reload();
        Assert.Equal("Buenos dias", catalog.Find("es", "Common.Greeting"));
    }

    [Fact]
    public void Reload_KeepsPreviousMessages_WhenFileIsInvalid()
    {
        var path = MessageDirectory.Create(ValidJson);
        using var catalog = CreateCatalog(path);
        File.WriteAllText(Path.Combine(path, "es.json"), "{ not valid json");
        catalog.Reload();
        Assert.Equal("Hola", catalog.Find("es", "Common.Greeting"));
    }
}
