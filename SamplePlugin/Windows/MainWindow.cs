using System;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.Sheets;
using MagitekClicker.Classes;

namespace MagitekClicker.Windows;

public class MainWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Magitek Clicker##clickermain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Configuration = plugin.Configuration;
        Configuration.Save();

        Plugin = plugin;
    }

    public void Dispose() {
        if (this.IsOpen) this.Toggle();
        Configuration.Save();
    }

    public override void Draw()
    {
        if(ImGui.BeginTabBar("Tab Bar##clickertabmain", ImGuiTabBarFlags.None))
        {
            DrawGeneralTab();
            DrawSoundsTab();
            DrawTriggersTab();

            ImGui.EndTabBar();
        }
    }

    private void DrawGeneralTab()
    {
        if(ImGui.BeginTabItem("General"))
        {
            var isEnabled = Configuration.Enabled;
            if (ImGui.Checkbox("Enable clicker?", ref isEnabled))
            {
                Configuration.Enabled = isEnabled;
                Configuration.Save();
            }

            ImGui.EndTabItem();
        }
    }

    private void DrawSoundsTab()
    {
        if (ImGui.BeginTabItem("Sounds"))
        {
            if (ImGui.Button("New Sound"))
            {
                string name = $"Sound {Configuration.AudioFiles.Count}";
                AudioFile audioFile = new AudioFile(name);
                Configuration.AudioFiles.Add(audioFile);
                Configuration.Save();
            }
            if (ImGui.BeginTable("##Sounds", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Path");
                ImGui.TableSetupColumn("Delete");
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                for (int i = 0; i < Configuration.AudioFiles.Count; i++)
                {
                    AudioFile audioFile = Configuration.AudioFiles[i];

                    string name = audioFile.Name;

                    if (ImGui.InputTextWithHint($"##sound-name{i}", "", ref name, 100))
                    {
                        audioFile.Name = name;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    string path = audioFile.Path;
                    if (ImGui.InputTextWithHint($"##sound-path{i}", "", ref path, 100))
                    {
                        audioFile.Path = path;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    if(ImGui.Button($"Delete##sound-delete{i}"))
                    {
                        Configuration.AudioFiles.RemoveAt(i);
                        Configuration.Save();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                }

            }
            ImGui.EndTable();

            ImGui.EndTabItem();
        }
    }

    private void DrawTriggersTab()
    {
        if (ImGui.BeginTabItem("Triggers"))
        {
            if (ImGui.Button("New Trigger"))
            {
                string name = $"Trigger {Configuration.Triggers.Count}";
                Trigger trigger = new Trigger(name);
                Configuration.Triggers.Add(trigger);
                Configuration.Save();
            }
            if (ImGui.BeginTable("##Triggers", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Phrase");
                ImGui.TableSetupColumn("Sound");
                ImGui.TableSetupColumn("Enabled");
                ImGui.TableSetupColumn("Delete");
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                for (int i = 0; i < Configuration.Triggers.Count; i++)
                {
                    var trigger = Configuration.Triggers[i];

                    string name = trigger.Name;

                    if (ImGui.InputTextWithHint($"##trigger-name{i}", "", ref name, 100))
                    {
                        trigger.Name = name;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    string phrase = trigger.TriggerPhrases.Count > 0 ? trigger.TriggerPhrases[0] : "";
                    if (ImGui.InputTextWithHint($"##trigger-phrase{i}", "", ref phrase, 100))
                    {
                        if (trigger.TriggerPhrases.Count == 0) trigger.TriggerPhrases.Add(phrase);
                        else trigger.TriggerPhrases[0] = phrase;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    string sound = trigger.AudioIds.Count > 0 ? trigger.AudioIds[0] : "";
                    if (ImGui.InputTextWithHint($"##trigger-sound{i}", "", ref sound, 100))
                    {
                        if (trigger.AudioIds.Count == 0) trigger.AudioIds.Add(sound);
                        else trigger.AudioIds[0] = sound;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    bool enabled = trigger.Enabled;
                    if(ImGui.Checkbox($"##trigger-enabled{i}", ref enabled))
                    {
                        trigger.Enabled = enabled;
                        Configuration.Save();
                    }

                    ImGui.TableNextColumn();

                    if (ImGui.Button($"Delete##trigger-delete{i}"))
                    {
                        Configuration.Triggers.RemoveAt(i);
                        Configuration.Save();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                }

            }
            ImGui.EndTable();

            ImGui.EndTabItem();
        }
    }
}
