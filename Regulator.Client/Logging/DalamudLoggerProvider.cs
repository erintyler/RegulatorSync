using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace Regulator.Client.Logging;

public class DalamudLoggerProvider(IPluginLog pluginLog) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new DalamudLogger(pluginLog, categoryName);
    }
    
    public void Dispose()
    {
        // No resources to dispose
    }
}