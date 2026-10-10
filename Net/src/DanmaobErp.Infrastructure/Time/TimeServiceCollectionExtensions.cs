using DanmaobErp.Application.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DanmaobErp.Infrastructure.Time;

public static class TimeServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessClock(this IServiceCollection services)
    {
        {
            services.TryAddSingleton<TimeProvider>(TimeProvider.System);
            services.AddSingleton<IBusinessClock, BusinessClock>();
            return services;
        }
    }
}
