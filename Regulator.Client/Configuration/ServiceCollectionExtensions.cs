using System;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Regulator.Client.Commands;
using Regulator.Client.Handlers.Client.Glamourer;
using Regulator.Client.Handlers.Client.Management;
using Regulator.Client.Handlers.Client.Notifications;
using Regulator.Client.Handlers.Server.Connection;
using Regulator.Client.Handlers.Server.Glamourer;
using Regulator.Client.Handlers.Server.Management;
using Regulator.Client.Logging;
using Regulator.Client.Models.Configuration;
using Regulator.Client.Services.Authentication;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Client.Services.Data;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Hubs;
using Regulator.Client.Services.Interop;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Providers;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Ui;
using Regulator.Client.Services.Ui.Interfaces;
using Regulator.Client.Services.Utilities;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Client.Windows;
using Regulator.Services.Sync.Shared.Hubs;
using Regulator.Services.Sync.Shared.Services;
using Regulator.Services.Sync.Shared.Services.Interfaces;
using Serilog;
using ClientHandlers = Regulator.Client.Handlers.Client;
using ServerHandlers = Regulator.Client.Handlers.Server;
using Task = System.Threading.Tasks.Task;

namespace Regulator.Client.Configuration;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddSingleton<IDebounceService, DebounceService>();
        services.AddSingleton<IDependencyMonitoringService, DependencyMonitoringService>();
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IThreadService, ThreadService>();
        services.AddSingleton<IHashService, HashService>();
        
        return services;
    }
    
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        // Client event handlers
        services.AddHostedService<ClientHandlers.Glamourer.CustomizationsResetHandler>();
        services.AddHostedService<CustomizationsUpdatedHandler>();
        services.AddHostedService<RequestCustomizationsHandler>();
        services.AddHostedService<NotificationMessageHandler>();
        services.AddHostedService<AddSyncCodeHandler>();
        services.AddHostedService<ReceiveSyncRequestHandler>();
        services.AddHostedService<OnConnectedHandler>();
        services.AddHostedService<SyncRequestResponseHandler>();
        services.AddHostedService<SyncRequestFinalizedHandler>();
        services.AddHostedService<SendOnlineDataHandler>();
        
        // Server event handlers
        services.AddHostedService<ClientOnlineHandler>();
        services.AddHostedService<CustomizationsRequestHandler>();
        services.AddHostedService<ServerHandlers.Glamourer.CustomizationsResetHandler>();
        services.AddHostedService<ReceiveCustomizationsHandler>();
        
        return services;
    }

    public static IServiceCollection AddInteropServices(this IServiceCollection services)
    {
        services.AddSingleton<IGlamourerApiClient, GlamourerApiClient>();

        return services;
    }

    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerProvider, PlayerProvider>();
        services.AddSingleton<ICharacterHashProvider, CharacterHashProvider>();
        services.AddSingleton<ISyncCodeProvider, SyncCodeProvider>();
        
        services.AddHostedService<PlayerProvider>(p => (PlayerProvider)p.GetRequiredService<IPlayerProvider>());

        return services;
    }

    public static IServiceCollection AddLogging(this IServiceCollection services, IPluginLog pluginLog)
    {
        // Add serilog to the service collection
        services.AddLogging(builder =>
        {
            builder.AddDalamudLogger(pluginLog);
        });

        return services;
    }

    public static IServiceCollection AddDalamudServices(this IServiceCollection services, IDalamudPluginInterface pluginInterface)
    {
        var dalamudServices = new DalamudServices(pluginInterface);

        services.AddSingleton<IDalamudPluginInterface>(_ => pluginInterface);
        services.AddSingleton<IClientState>(_ => dalamudServices.ClientState);
        services.AddSingleton<IFramework>(_ => dalamudServices.Framework);
        services.AddSingleton<IPluginLog>(_ => dalamudServices.PluginLog);
        services.AddSingleton<IObjectTable>(_ => dalamudServices.ObjectTable);
        services.AddSingleton<ICommandManager>(_ => dalamudServices.CommandManager);
        services.AddSingleton<INotificationManager>(_ => dalamudServices.NotificationManager);
        
        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddOptions<AuthenticationSettings>()
            .Configure(o =>
            {
#if DEBUG                
                o.OAuthUrl = "http://localhost:5296/auth/token";
#else
                o.OAuthUrl = "https://auth.neurilink.app/auth/token";
#endif
            });
        
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        services.AddSingleton<ICallbackService, CallbackService>();

        return services;
    }

    public static IServiceCollection AddSignalR(this IServiceCollection services)
    { 
#if DEBUG
        const string hubUrl = "http://localhost:5016/sync";
#else
        const string hubUrl = "https://sync.neurilink.app/sync";
#endif
        
        services.AddSingleton<HubConnection>(p => new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () =>
                {
                    var tokenProvider = p.GetRequiredService<IAccessTokenProvider>();
                    return Task.FromResult(tokenProvider.GetAccessToken());
                };
            })
            .WithAutomaticReconnect()
            .Build());
        
        services.AddSingleton<IRegulatorServerMethods, RegulatorServerClient>();
        services.AddSingleton<IRegulatorClientMethods, RegulatorClientMethods>();
        services.AddHostedService<RegulatorServerClient>(p => (RegulatorServerClient)p.GetRequiredService<IRegulatorServerMethods>());
        services.AddHostedService<RegulatorClientMethods>(p => (RegulatorClientMethods)p.GetRequiredService<IRegulatorClientMethods>());
        
        return services;
    }
    
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddHostedCommand<AddSyncCodeCommand>();
        services.AddHostedCommand<LoginCommand>();
        services.AddHostedCommand<ShowWindowCommand>();
        
        return services;
    }

    public static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddSingleton(new WindowSystem("Regulator"));
        services.AddSingleton<Window, MainWindow>();
        services.AddSingleton<Window, NewSyncRequestWindow>();

        services.AddSingleton<IWindowService, WindowService>();
        services.AddHostedService<WindowService>(p => (WindowService)p.GetRequiredService<IWindowService>());
        
        return services;
    }

    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services.AddSingleton<IClientDataService, ClientDataService>();
        services.AddSingleton<ISyncRequestService, SyncRequestService>();

        return services;
    }
    
    private static IServiceCollection AddHostedCommand<T>(this IServiceCollection services) where T : class, IHostedService
    {
        services.AddSingleton<T>();
        services.AddHostedService(provider => provider.GetRequiredService<T>());
        return services;
    }
}