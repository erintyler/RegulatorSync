using System;
using System.IO;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Configuration;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Services.Sync.Shared.Services.Interfaces;

namespace Regulator.Client;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private readonly IHost? _host;

    public Plugin()
    {
        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddUtilities();
            builder.Services.AddHandlers();
            builder.Services.AddInteropServices();
            builder.Services.AddProviders();
            builder.Services.AddLogging(Log);
            builder.Services.AddDalamudServices(PluginInterface);
            builder.Services.AddAuthenticationServices();
            builder.Services.AddSignalR();
            builder.Services.AddCommands();
            builder.Services.AddWindows();
            builder.Services.AddData();

            _host = builder.Build();

            Task.Run(async () =>
            {
                try
                {
                    await _host.StartAsync();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Failed to start host.");
                }
            });
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start host.");
        }
    }

    public void Dispose()
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        
    }
}
