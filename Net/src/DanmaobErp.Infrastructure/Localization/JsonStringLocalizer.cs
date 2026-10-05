using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DanmaobErp.Infrastructure.Localization;

public sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly MessageCatalog _catalog;
    private readonly LocalizationSettings _settings;
    private readonly ILogger<JsonStringLocalizer> _logger;

    public JsonStringLocalizer(MessageCatalog catalog, LocalizationSettings settings, ILogger<JsonStringLocalizer> logger)
    {
        _catalog = catalog;
        _settings = settings;
        _logger = logger;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var culture = CultureInfo.CurrentUICulture;
            var value = _catalog.Find(culture.Name, name);
            if (value is null)
            {
                value = _catalog.Find(culture.TwoLetterISOLanguageName, name);
            }
            if (value is null)
            {
                value = _catalog.Find(_settings.DefaultCulture, name);
            }
            if (value is null)
            {
                _logger.LogWarning("Message key {MessageKey} was not found in any dictionary.", name);
                return new LocalizedString(name, name, true);
            }
            return new LocalizedString(name, value, false);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var message = this[name];
            var formatted = string.Format(CultureInfo.InvariantCulture, message.Value, arguments);
            return new LocalizedString(name, formatted, message.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return Enumerable.Empty<LocalizedString>();
    }
}
