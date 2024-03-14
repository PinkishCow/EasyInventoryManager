using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EasyInventoryManager.Windows;
using ECommons;
using ECommons.Logging;

namespace EasyInventoryManager
{
    public sealed class EasyInventoryManager : IDalamudPlugin
    {
        public string Name => "Easy Inventory Manager";
        private const string CommandName = "/em";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("EasyInventoryManager");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public EasyInventoryManager(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

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

        public void Start()
        {
            //Don't bother going home if there is already a bell in interaction distance or open
            var bell = Helpers.GetReachableRetainerBell();


            if (!bell && (Configuration.UsePersonalHouse || Configuration.UseFCHouse))
            {
                Tasks.GoHomeTask.Enqueue();
            } else
            {
                DuoLog.Error("No house to go to. Stand next to a bell, hobo");
            }

            //Got to have a bell nearby if we aren't going home
            //Find the bell

            //Dingle it

            //Retain
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
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
