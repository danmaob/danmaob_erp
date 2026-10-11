using DanmaobErp.Application.Events;
using DanmaobErp.Application.Persistence;
using DanmaobErp.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobErp.Infrastructure.Persistence;

public static class UnitOfWorkServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<PendingBusinessEvents>();
        services.AddScoped<IPendingBusinessEvents>(serviceProvider => serviceProvider.GetRequiredService<PendingBusinessEvents>());
        services.AddScoped<BusinessEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
