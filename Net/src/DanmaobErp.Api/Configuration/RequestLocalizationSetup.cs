using System.Globalization;
using DanmaobErp.Infrastructure.Localization;
using Microsoft.AspNetCore.Localization;

namespace DanmaobErp.Api.Configuration;

public static class RequestLocalizationSetup
{
    public static RequestLocalizationOptions Create(LocalizationSettings settings)
    {
        var options = new RequestLocalizationOptions();
        options.DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture, new CultureInfo(settings.DefaultCulture));
        options.SupportedCultures = new List<CultureInfo> { CultureInfo.InvariantCulture };
        options.SupportedUICultures = settings.SupportedCultures.Select(culture => new CultureInfo(culture)).ToList();
        return options;
    }
}
