using DanmaobErp.Application.Email;
using DanmaobErp.Application.Security;
using DanmaobErp.Application.Storage;
using DanmaobErp.Infrastructure.Email;
using DanmaobErp.Infrastructure.Security;
using DanmaobErp.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobErp.Infrastructure.ExternalServices;

public static class ExternalServicesServiceCollectionExtensions
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration, string contentRootPath, bool isDevelopment)
    {
        var configuredOutbox = configuration["Email:OutboxPath"];
        var outboxPath = Path.Combine(contentRootPath, string.IsNullOrWhiteSpace(configuredOutbox) ? "App_Data/mail-outbox" : configuredOutbox);
        var configuredFrom = configuration["Email:From"];
        var from = string.IsNullOrWhiteSpace(configuredFrom) ? "no-responder@danmaob.com.mx" : configuredFrom;
        var configuredRoot = configuration["FileStorage:RootPath"];
        var rootPath = Path.Combine(contentRootPath, string.IsNullOrWhiteSpace(configuredRoot) ? "App_Data/files" : configuredRoot);
        services.AddSingleton(new EmailSettings(outboxPath, from));
        services.AddSingleton<IEmailSender, FolderEmailSender>();
        services.AddSingleton(new FileStorageSettings(rootPath));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        if (isDevelopment)
        {
            services.AddSingleton<IHumanVerifier, DevelopmentHumanVerifier>();
        }
        else
        {
            services.AddSingleton<IHumanVerifier, RejectingHumanVerifier>();
        }
        return services;
    }
}
