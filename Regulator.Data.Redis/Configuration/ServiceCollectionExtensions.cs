using Microsoft.Extensions.DependencyInjection;
using Regulator.Data.Redis.Services;
using Regulator.Data.Redis.Services.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Regulator.Data.Redis.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisDataServices(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddCache();

        return services;
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ISyncRequestRepository, SyncRequestRepository>();

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(1),
                DistributedCacheDuration = TimeSpan.FromHours(1)
            })
            .WithNeueccMessagePackSerializer();
        
        return services;
    }
}