using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace DanmaobErp.Infrastructure.Localization;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddMessageLocalization(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var configuredPath = configuration["Localization:Path"];
        var path = Path.Combine(contentRootPath,
            string.IsNullOrWhiteSpace(configuredPath) ? "Localization" : configuredPath);

        var configuredCulture = configuration["Localization:DefaultCulture"];
        var defaultCulture = string.IsNullOrWhiteSpace(configuredCulture) ? "es" : configuredCulture;

        var supportedCultures = configuration.GetSection("Localization:SupportedCultures").Get<string[]>()!;

        if (supportedCultures is null || supportedCultures.Length == 0)
        {
            supportedCultures = new[] { defaultCulture };
        }

        services.AddSingleton(new LocalizationSettings(path, defaultCulture, supportedCultures));
        services.AddSingleton<MessageCatalog>();
        services.AddSingleton<JsonStringLocalizer>();
        services.AddSingleton<IStringLocalizer>(serviceProvider => serviceProvider.GetRequiredService<JsonStringLocalizer>());
        services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, JsonStringLocalizerFactory>());
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        return services;
    }
}
