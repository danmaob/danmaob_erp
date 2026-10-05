using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DanmaobErp.Infrastructure.Localization;

public sealed class MessageCatalog : IDisposable
{
    private readonly LocalizationSettings _settings;
    private readonly ILogger<MessageCatalog> _logger;
    private readonly PhysicalFileProvider _fileProvider;
    private readonly IDisposable _changeRegistration;
    private volatile IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _messages;

    public MessageCatalog(LocalizationSettings settings, ILogger<MessageCatalog> logger)
    {
        _settings = settings;
        _logger = logger;
        _messages = MessageFileReader.ReadDirectory(settings.Path);
        if (_messages.ContainsKey(settings.DefaultCulture) is false)
        {
            throw new InvalidOperationException($"The message dictionary for the default culture '{settings.DefaultCulture}' was not found in '{settings.Path}')");
        }
        _fileProvider = new PhysicalFileProvider(settings.Path);
        _changeRegistration = ChangeToken.OnChange(() => _fileProvider.Watch("*.json"), Reload);
    }

    public string? Find(string culture, string key)
    {
        if (_messages.TryGetValue(culture, out var messages) && messages.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    public void Reload()
    {
        try
        {
            var messages = MessageFileReader.ReadDirectory(_settings.Path);
            if (messages.ContainsKey(_settings.DefaultCulture) is false)
            {
                _logger.LogError("The message dictionary for the default culture {Culture} is missing. The previous messages are kept.", _settings.DefaultCulture);
                return;
            }
            _messages = messages;
            _logger.LogInformation("Message dictionaries reloaded from {Path}.", _settings.Path);
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            _logger.LogError(exception, "The message dictionaries could not be reloaded. The previous messages are kept.");
        }
    }

    public void Dispose()
    {
        _changeRegistration.Dispose();
        _fileProvider.Dispose();
    }
}
