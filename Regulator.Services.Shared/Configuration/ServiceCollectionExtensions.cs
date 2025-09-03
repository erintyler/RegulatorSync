using Microsoft.Extensions.DependencyInjection;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Shared.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserCreationService, UserCreationService>();
        services.AddScoped<IUserContextService, UserContextService>();
        
        return services;
    }
}