using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using WhoSaidWhatNow;
using WhoSaidWhatNow.Windows;
using System.Linq;


public class TabGroups
{

    public TabGroups(MainWindow main)
    {

        if (ImGui.BeginTabItem("Groups"))
        {
            main.toggleWindow(true);

            ImGui.BeginTabBar("###groups");
            // populate the list of selectable groups.
            foreach (var g in Plugin.Groups)
            {
                var index = g.Key;
                var group = g.Value;
                var name = group.NAME;
                var players = group.PLAYERS;
                if (ImGui.BeginTabItem($"{name}###Tab_{index}"))
                {
                    if (ImGui.BeginPopupContextItem())
                    {
                        var input = String.Empty;
                        ImGui.InputTextWithHint($"##{index}", "Enter the group name...", ref name, 30);
                        Plugin.Groups[index] = (name, players);

                        if (ImGui.Button("Delete"))
                        {
                            Plugin.Groups.Remove(index);
                        }

                        ImGui.EndPopup();
                    }
                    ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
                    foreach (var p in Plugin.Players)
                    {
                        bool isActive;
                        players.TryGetValue(p, out isActive);
                        ImGui.Checkbox(p.Name, ref isActive);
                        players[p] = isActive;
                    }
                    ImGui.EndChild();
                    ImGui.SameLine();

                    // construct chatlog.
                    ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
                    ImGui.EndChild();

                    ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT);
                    ImGui.BeginGroup();
                    // for all chat entries;
                    foreach (var c in Plugin.ChatEntries)
                    {
                        // if we are displaying this type of message;
                        if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                        {
                            // and if the player is among the tracked;
                            var p = Plugin.Players.Find(p => c.Value.Sender.Name.Contains(p.Name));
                            if (players[p!])
                            {
                                MainWindow.ShowMessage(c);
                            }
                        }
                    }
                    ImGui.EndGroup();
                    ImGui.EndChild();


                    ImGui.EndTabItem();

                }
            }

            if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                Plugin.Groups.Add($"{Plugin.Groups.Count + 1}", ($"Group {Plugin.Groups.Count + 1}", Plugin.Players.ToDictionary(p => p, p => false)));
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndTabItem();

        }

    }

}
