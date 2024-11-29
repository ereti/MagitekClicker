using Dalamud.Configuration;
using Dalamud.Plugin;
using MagitekClicker;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace MagitekClicker.Classes;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = false;
    public List<AudioFile> AudioFiles = new();
    public List<Trigger> Triggers = new();

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
