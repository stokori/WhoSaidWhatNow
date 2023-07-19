using Dalamud.DrunkenToad;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using WhoSaidWhatNow.Services;

namespace WhoSaidWhatNow.Windows;

public class ConfigWindow : Window, IDisposable
{
    private string newName = String.Empty;
    private int newServer = 0;
    internal const String ID_PANEL_LEFT = "###WhoSaidWhatNowConfig_LeftPanel_Child";
    private readonly Plugin plugin;
    private readonly string[] worldNames = DataManagerExtensions.WorldNames(Plugin.DataManager);

    public ConfigWindow(Plugin plugin) : base(
        "Who Said What Now - Settings", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.plugin = plugin;
        this.Size = new Vector2(600, 500);
        this.SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy for config variables

        // design philosophy for us right now is we save automatically
        // if we have more options we may change later, but honestly I think some larger plugins also do this so we're fine

        ImGui.BeginTabBar("###WhoSaidWhatNowConfig_Tab_Bar");
        if (ImGui.BeginTabItem("General"))
        {
            // replace the existing panels by using the same IDs.
            ImGui.BeginChild(ConfigWindow.ID_PANEL_LEFT, new Vector2(0,0), true);

            bool enabled = Plugin.Config.Enabled;
            if (ImGui.Checkbox("Plugin On/Off", ref enabled))
            {
                Plugin.Config.Enabled = enabled;
                Plugin.Config.Save();
            }
            bool autoscroll = Plugin.Config.AutoscrollOnOpen;
            if (ImGui.Checkbox("Autoscroll to bottom when opening log (may or may not be functional)", ref autoscroll))
            {
                Plugin.Config.AutoscrollOnOpen = autoscroll;
                Plugin.Config.Save();
            }

            ImGui.EndChild();
            ImGui.EndTabItem();

        }

        if (ImGui.BeginTabItem("Always Tracked Players"))
        {
            ImGui.BeginChild(ConfigWindow.ID_PANEL_LEFT, new Vector2(0, 0), true);

            ImGui.Text("Any players added here will always be tracked and marked with .");
            ImGui.Text("They can only be removed via this page.");
            ImGui.NewLine();

            if (ImGui.BeginTable("alwaysTrackedPlayers", 4, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("Player Name", ImGuiTableColumnFlags.WidthStretch, 150);
                ImGui.TableSetupColumn("Server", ImGuiTableColumnFlags.WidthStretch, 150);
                ImGui.TableSetupColumn("");

                ImGui.TableHeadersRow();
                ImGui.TableNextRow();

                List<Tuple<string, string>> templist = new List<Tuple<string, string>>(Plugin.Config.AlwaysTrackedPlayers);
                //build table of existing data
                foreach (var player in templist)
                {
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.Text(player.Item1);
                    ImGui.TableNextColumn();
                    ImGui.Text(player.Item2);
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Remove##" + player.Item1))
                    {
                        PlayerService.RemoveTrackedPlayer(player);
                        PlayerService.CheckTrackedPlayers();
                    }
                    ImGui.TableNextColumn();
                    ImGui.TableNextRow();
                }
            }

            //ui elements for adding new player
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##inputNewName", ref newName, 100);
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.Combo("##servers", ref newServer, worldNames, worldNames.Length);
            ImGui.TableNextColumn();
            if (ImGui.Button("Add"))
            {
                if (newName.IsValidCharacterName() && !newServer.Equals(""))
                {
                    PlayerService.AddTrackedPlayer(new Tuple<string,string>(newName, worldNames[newServer]));
                    newName = string.Empty;
                    newServer = 0;
                }
            }
            ImGui.EndTable();

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Channels"))
        {
            ImGui.BeginChild(ConfigWindow.ID_PANEL_LEFT, new Vector2(0, 0), true);

            foreach (var chan in Plugin.Config.ChannelToggles)
                {
                    bool val = chan.Value;
                    if (ImGui.Checkbox(chan.Key.ToString(), ref val))
                    {
                        Plugin.Config.ChannelToggles[chan.Key] = val;
                        Plugin.Config.Save();
                    }
                }
            
            
            ImGui.EndChild();
            ImGui.EndTabItem();
        }

    }
}
