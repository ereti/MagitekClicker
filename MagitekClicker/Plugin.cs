using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MagitekClicker.Classes;
using MagitekClicker.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MagitekClicker;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mclicker";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("MagitekClicker");
    private MainWindow MainWindow { get; init; }

    private Random random = new();

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open configuration"
        });

        Service.PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui

        // Adds another button that is doing the same but for the main ui of the plugin
        Service.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUI;

        Service.ChatGui.ChatMessage += HandleChatMessage;
    }

    public void HandleChatMessage(IHandleableChatMessage message)
    {
        if (!Configuration.Enabled) return;
        if (message == null) return;

        foreach(var trigger in Configuration.Triggers)
        {
            if (!trigger.Enabled) continue;
            if (trigger.TriggerPhrases.Count == 0) continue;

            var allowedChannels = trigger.AllowedChannels.Count > 0 ? trigger.AllowedChannels : Configuration.AllowedChannels;
            if (!allowedChannels.Contains(message.LogKind)) continue;

            var allowedPlayers = trigger.WhitelistedPlayers.Count > 0 ? trigger.WhitelistedPlayers : [];
            if (allowedPlayers.Count > 0)
            {
                var sender = message.Sender;
                var senderPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
                var senderName = senderPayload?.PlayerName ?? "";
                var senderWorld = senderPayload?.World.Value.Name.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderWorld)) continue;
                if (!allowedPlayers.Any(x => string.Equals(x.PlayerName, senderName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.PlayerWorld, senderWorld, StringComparison.OrdinalIgnoreCase))) continue;
            }

            if (message.Message.ToString().ToLower().Contains(trigger.TriggerPhrases[0].ToLower()))
            {
                if (trigger.AudioIds.Count == 0) continue;
                string soundId = trigger.AudioIds.Count == 1 ? trigger.AudioIds.First() : trigger.AudioIds[random.Next(0, trigger.AudioIds.Count)];

                foreach (var audio in Configuration.AudioFiles)
                {
                    if(audio.Name == soundId && audio.Path != "")
                    {
                        //SoundPlayer.PlaySound will apply the XIV SFX Volume if the user has enabled that option,
                        //otherwise it will use the volume set in the plugin configuration.
                        var baseVolume = Configuration.UseXIVSFXVolume ? 100 : Convert.ToInt32(Configuration.Volume * 100);

                        SoundPlayer.PlaySound(audio.Path, Configuration.UseXIVSFXVolume, baseVolume, soundId);
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

        Service.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
