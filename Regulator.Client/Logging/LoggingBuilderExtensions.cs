using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace Regulator.Client.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddDalamudLogger(this ILoggingBuilder builder, IPluginLog pluginLog)
    {
        builder.ClearProviders();
        builder.AddProvider(new DalamudLoggerProvider(pluginLog));
        return builder;
    }
}