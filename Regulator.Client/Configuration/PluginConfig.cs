using System;
using Dalamud.Configuration;

namespace Regulator.Client.Configuration;

[Serializable]
public class PluginConfig : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; } 
    public DateTime? TokenExpiry { get; set; }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}