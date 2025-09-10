using Microsoft.Extensions.DependencyInjection;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Extensions;
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

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, TokenSettings tokenSettings)
    {
        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = RegulatorAuthenticationSchemes.Token;
                o.DefaultChallengeScheme = RegulatorAuthenticationSchemes.Token;
            })
            .AddJwtBearer(RegulatorAuthenticationSchemes.Token, o => o.TokenValidationParameters = tokenSettings.ToTokenValidationParameters());

        services.AddAuthorizationBuilder()
            .AddPolicy(RegulatorAuthenticationSchemes.Token, o =>
            {
                o.AddAuthenticationSchemes(RegulatorAuthenticationSchemes.Token);
                o.RequireAuthenticatedUser();
            });
        
        return services;
    }
}