using System;
using Regulator.Client.Enums;

namespace Regulator.Client.Services.Utilities.Interfaces;

public interface IDependencyMonitoringService
{
    event Action<PluginState> OnGlamourerStateChanged; 
}