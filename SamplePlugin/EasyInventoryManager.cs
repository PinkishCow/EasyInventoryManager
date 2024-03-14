global using static EasyInventoryManager.EasyInventoryManager;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EasyInventoryManager.Windows;
using ECommons;
using ECommons.Logging;
using ECommons.Automation;


namespace EasyInventoryManager
{
    public sealed class EasyInventoryManager : IDalamudPlugin
    {
        public string Name => "Easy Inventory Manager";
        private const string CommandName = "/eim";
        private bool globalStop = false;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public static Configuration config { get; set; }
        public WindowSystem WindowSystem = new("EasyInventoryManager");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        internal static EasyInventoryManager Instance;
        internal TaskManager TaskManager;

        public EasyInventoryManager(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
            Instance = this;
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            config = Configuration.Load(this.PluginInterface);

            TaskManager = new() { AbortOnTimeout = true , TimeLimitMS = 20000, ShowDebug = true };

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            EzCmd.Add(CommandName, CommandHandler, "Easy Inventory Manager commands");

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            ECommonsMain.Dispose();

            this.CommandManager.RemoveHandler(CommandName);
        }

        public void StartMainLoop()
        {
            globalStop = false;
            //Don't bother going home if there is already a bell in interaction distance or open
            var bell = Helpers.GetReachableRetainerBell();


            if (!bell && (config.UsePersonalHouse || config.UseFCHouse) && !globalStop)
            {
                Tasks.GoHomeTask.Enqueue();
            } else
            {
                DuoLog.Error("No house to go to. Stand next to a bell, hobo");
            }

            if(!globalStop)
            {
                Tasks.SelectBellTask.Enqueue();
            }

            //Got to have a bell nearby if we aren't going home
            //Find the bell

            //Dingle it

            //Retain
        }

        public void StopAll()
        {
            globalStop = true;
            TaskManager.Abort();
        }

        private void CommandHandler(string command, string args)
        {
            if (args == "config")
            {
                ConfigWindow.IsOpen = true;
            } else if (args == "home")
            {
                StartMainLoop();
            } else
            {
                MainWindow.IsOpen = true;
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
