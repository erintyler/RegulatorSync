using Dalamud.Configuration;
using System;

namespace Regulator.Client;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
