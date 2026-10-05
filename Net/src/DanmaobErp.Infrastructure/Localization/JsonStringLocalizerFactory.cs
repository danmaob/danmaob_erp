using Microsoft.Extensions.Localization;

namespace DanmaobErp.Infrastructure.Localization;

public sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly JsonStringLocalizer _localizer;

    public JsonStringLocalizerFactory(JsonStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return _localizer;
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return _localizer;
    }
}
