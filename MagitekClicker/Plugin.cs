using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MagitekClicker.Windows;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using MagitekClicker.Classes;
using System.Collections.Generic;

namespace MagitekClicker;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/mclicker";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("MagitekClicker");
    private MainWindow MainWindow { get; init; }


    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open configuration"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUI;

        ChatGui.ChatMessage += HandleChatMessage;
    }

    public void HandleChatMessage(XivChatType type, int timestamp, ref SeString senderE, ref SeString message, ref bool isHandled)
    {
        if (!Configuration.Enabled) return;
        if (!Configuration.AllowedChannels.Contains(type)) return;
        if (message == null) return;

        foreach(var trigger in Configuration.Triggers)
        {
            if (!trigger.Enabled) continue;
            if (trigger.TriggerPhrases.Count == 0) continue;
            if (message.ToString().ToLower().Contains(trigger.TriggerPhrases[0].ToLower()))
            {
                if (trigger.AudioIds.Count == 0) continue;
                string soundId = trigger.AudioIds[0];
                foreach(var audio in Configuration.AudioFiles)
                {
                    if(audio.Name == soundId && audio.Path != "")
                    {
                        SoundPlayer.Instance.SetVolume(Configuration.Volume);
                        SoundPlayer.Instance.PlaySound(audio.Path);
                        break;
                    }
                }
       
            }
        }
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
        SoundPlayer.Instance.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
