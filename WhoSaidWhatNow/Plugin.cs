using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using WhoSaidWhatNow.Windows;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;
using System.Collections.Generic;
using WhoSaidWhatNow.Objects;
using System;
using System.Linq;

namespace WhoSaidWhatNow
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Who Said What Now";
        private const string COMMAND = "/whowhat";
        public static Configuration Config = null!;
        public static Player? SelectedPlayer = null;
        public static List<Player> Players = new List<Player>();
        public static List<Dictionary<Player, Boolean>> Groups = new List<Dictionary<Player, Boolean>> { Players.ToDictionary(p => p, p => false) };
        public static SortedList<DateTime, ChatEntry> ChatEntries = new SortedList<DateTime, ChatEntry>();
        public static TargetManager TargetManager = null!;
        private static ChatListener s_chatListener = null!;
        private static CommandManager s_commandManager = null!;
        public static ClientState ClientState = null!;

        private static DalamudPluginInterface s_pluginInterface = null!;
        private static WindowSystem WindowSystem = new("WhoSaidWhatNow");

        //TODO: Make sure we're only actually declaring stuff we need
        //I went a little ham because of what I thought was required by onmessagehandled
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] TargetManager targetManager,
            [RequiredVersion("1.0")] SigScanner sigScanner)
        {
            s_pluginInterface = pluginInterface;
            s_commandManager = commandManager;
            TargetManager = targetManager;

            // initiatize our configuration
            Config = s_pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(s_pluginInterface);

            // create the listener
            s_chatListener = new ChatListener(chatGui);

            //add our windows
            WindowSystem.AddWindow(new ConfigWindow());
            WindowSystem.AddWindow(new MainWindow());

            //TODO: add a command for the config window?
            //and one for on/off toggle
            s_commandManager.AddHandler(COMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            s_pluginInterface.UiBuilder.Draw += DrawUI;
            s_pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Plugin.ClientState = clientState;
            Plugin.ClientState.Login += onLogin;
        }

        void onLogin(object? sender, EventArgs e)
        {
            // TODO add self, Tier is doing
        }

        //TODO: make sure we're disposing of everything we need to appropriately
        public void Dispose()
        {
            s_chatListener.Dispose();
            WindowSystem.RemoveAllWindows();
            s_commandManager.RemoveHandler(COMMAND);
            Plugin.ClientState.Login -= onLogin;
        }

        private static void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow("Who Said What Now")!.IsOpen = true;
        }

        private static void DrawUI()
        {
            WindowSystem.Draw();
        }

        public static void DrawConfigUI()
        {
            WindowSystem.GetWindow("Who Said What Now - Settings")!.IsOpen = true;
        }
    }
}
