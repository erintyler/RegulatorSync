using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Regulator.Client.Configuration;

public class DalamudServices
{
    public DalamudServices(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);
    }
    
    [PluginService]
    public IDalamudPluginInterface PluginInterface { get; set; } = null!;
    
    [PluginService]
    public IClientState ClientState { get; set; } = null!;
    
    [PluginService]
    public IFramework Framework { get; set; } = null!;
    
    [PluginService]
    public IPluginLog PluginLog { get; set; } = null!;
    
    [PluginService]
    public IObjectTable ObjectTable { get; set; } = null!;
    
    [PluginService]
    public ICommandManager CommandManager { get; set; } = null!;
    
    [PluginService]
    public INotificationManager NotificationManager { get; set; } = null!;
}