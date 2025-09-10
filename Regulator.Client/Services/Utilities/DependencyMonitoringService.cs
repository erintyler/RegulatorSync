using System;
using System.Linq;
using Dalamud.Plugin;
using Microsoft.Extensions.Logging;
using Regulator.Client.Constants;
using Regulator.Client.Enums;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Utilities;

public class DependencyMonitoringService : IDependencyMonitoringService, IDisposable
{
    private bool _glamourerLoaded;

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ILogger<DependencyMonitoringService> _logger;
    
    public bool IsGlamourerLoaded
    {
        get => _glamourerLoaded;
        private set
        {
            _glamourerLoaded = value;
            OnGlamourerStateChanged?.Invoke(_glamourerLoaded ? PluginState.Active : PluginState.Inactive);
            
            _logger.LogInformation("Glamourer plugin state changed: {State}", _glamourerLoaded ? "Active" : "Inactive");
        }
    }

    public DependencyMonitoringService(IDalamudPluginInterface pluginInterface, ILogger<DependencyMonitoringService> logger)
    {
        _pluginInterface = pluginInterface;
        _logger = logger;
        
        pluginInterface.ActivePluginsChanged += PluginInterfaceOnActivePluginsChanged;
        GetInitialState();
    }

    public void GetInitialState()
    {
        IsGlamourerLoaded = _pluginInterface.InstalledPlugins.Any(p => p.InternalName.Equals(Dependencies.Glamourer, StringComparison.Ordinal) && p.IsLoaded);
    }

    private void PluginInterfaceOnActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        if (args.AffectedInternalNames.Contains(Constants.Dependencies.Glamourer))
        {
            IsGlamourerLoaded = args.Kind is PluginListInvalidationKind.Loaded or PluginListInvalidationKind.Update or PluginListInvalidationKind.AutoUpdate;
        }
    }

    public event Action<PluginState>? OnGlamourerStateChanged;

    public void Dispose()
    {
        _pluginInterface.ActivePluginsChanged -= PluginInterfaceOnActivePluginsChanged;
    }
}