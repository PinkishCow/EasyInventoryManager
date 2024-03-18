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
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using System.Collections.Generic;
using System.Configuration;
using DalaMock.Shared.Classes;
using Dalamud.Game.ClientState.Conditions;


namespace EasyInventoryManager
{
    public sealed class EasyInventoryManager : IDalamudPlugin
    {
        public string Name => "Easy Inventory Manager";
        private const string CommandName = "/eim";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public static Configuration config { get; set; }
        public WindowSystem WindowSystem = new("EasyInventoryManager");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        internal static EasyInventoryManager Instance;
        internal TaskManager TaskManager;

        //items for critical common lib
        internal static ICharacterMonitor characterMonitor;
        internal static IInventoryMonitor inventoryMonitor;
        internal static IInventoryScanner inventoryScanner;
        internal static OdrScanner odrScanner;
        internal static IGameInterface gameInterface;
        internal static IGameUiManager gameUiManager;
        internal static ICraftMonitor craftMonitor;

        public EasyInventoryManager(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            //Ecommons init
            ECommonsMain.Init(pluginInterface, this);

            //Critical Common init
            pluginInterface.Create<Service>();
            Service.Interface = new PluginInterfaceService(pluginInterface);



            Instance = this;
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            config = Configuration.Load(this.PluginInterface);

            TaskManager = new() { AbortOnTimeout = true, TimeLimitMS = 20000, ShowDebug = true };

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            EzCmd.Add(CommandName, CommandHandler, "Easy Inventory Manager commands");

            //Critical Common Lib
            Service.ExcelCache = new ExcelCache(Service.Data);
            Service.ExcelCache.PreCacheItemData();
            gameInterface = new GameInterface(Service.GameInteropProvider);
            gameUiManager = new GameUiManager(Service.GameInteropProvider, Service.GameGui);
            characterMonitor = new CharacterMonitor(Service.Framework, Service.ClientState, Service.ExcelCache);
            craftMonitor = new CraftMonitor(gameUiManager);
            odrScanner = new OdrScanner(characterMonitor);
            inventoryScanner = new InventoryScanner(characterMonitor, gameUiManager, gameInterface, odrScanner, Service.GameInteropProvider);
            inventoryMonitor = new InventoryMonitor(characterMonitor, craftMonitor, inventoryScanner, Service.Framework);
            inventoryScanner.Enable();

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            ECommonsMain.Dispose();
            inventoryScanner.Dispose();
            inventoryMonitor.Dispose();
            odrScanner.Dispose();
            craftMonitor.Dispose();
            characterMonitor.Dispose();
            gameUiManager.Dispose();
            gameInterface.Dispose();
            Service.Dereference();


            this.CommandManager.RemoveHandler(CommandName);
        }

        public unsafe void StartMainLoop()
        {
            // Can't trust that we know how many retainers there are, because it only loads once they click a bell one time.
            //Don't bother going home if there is already a bell in interaction distance or open
            var bell = Helpers.Helpers.GetReachableRetainerBell();

            if (!bell && (config.UsePersonalHouse || config.UseFCHouse))
            {
                Instance.TaskManager.Enqueue(() => DuoLog.Information("No bell nearby, going home"));
                Tasks.GoHomeTask.Enqueue();
                Instance.TaskManager.Enqueue(() => Player.Interactable && Svc.ClientState.TerritoryType.EqualsAny(Houses.List), 1000 * 60, "WaitUntilArrival");
                //Make sure we wait for loading, etc
                Instance.TaskManager.Enqueue(() => DuoLog.Information("Waiting for 5 seconds to arrive"));
                Instance.TaskManager.Enqueue(() => Helpers.Helpers.waitForSeconds(5), 1000 * 60, "WaitForTime");

            }
            else if (!bell || Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Helpers.Helpers.GetClosestRetainerBell().Position) > 20f)
            {
                Instance.TaskManager.Enqueue(() => DuoLog.Error("No house to go to. Stand next to a bell, hobo"));
            }
            
            Instance.TaskManager.Enqueue(() => !Instance.TaskManager.TaskStack.ContainsAny("WaitForTime"));
            Instance.TaskManager.Enqueue(() => AtBell());
            //Got to have a bell nearby if we aren't going home
            //Find the bell

            //Dingle it

            //Retain
        }

        public unsafe void AtBell()
        {
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Selecting bell"));
            Tasks.SelectBellTask.Enqueue();
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Waiting for 5 seconds for bell loading"));
            Instance.TaskManager.Enqueue(() => Helpers.Helpers.waitForSeconds(5), 1000 * 60, "WaitForTime");
            //Spawn in each retainer once to load their info into the inventory manager.
            Instance.TaskManager.Enqueue(() => !Instance.TaskManager.TaskStack.ContainsAny("WaitForTime"));
            Instance.TaskManager.Enqueue(() => AtRetainerList());

        }

        public unsafe void AtRetainerList()
        {
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Spawning retainers"));
            if (FFXIVClientStructs.FFXIV.Client.Game.RetainerManager.Instance()->GetRetainerCount() < 0)
            {
                Instance.TaskManager.Enqueue(() => DuoLog.Information("No retainers available"));
                return;
            }
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Spawning retainers"));
            for (var i = 0; i < FFXIVClientStructs.FFXIV.Client.Game.RetainerManager.Instance()->GetRetainerCount(); i++)
            {
                Tasks.SpawnRetainer.Enqueue(i);
            }
            Instance.TaskManager.Enqueue(() => !Instance.TaskManager.TaskStack.ContainsAny("SpawnRetainer", "SelectRetainer", "WaitForTime", "CheckRetainerAvailable", "CloseRetainer"));
            Instance.TaskManager.Enqueue(() => DuoLog.Information("-------------------Wowee----------------"));

            Instance.TaskManager.Enqueue(() => DuoLog.Information(inventoryMonitor.RetainerItemCounts.ToString()));
            Instance.TaskManager.Enqueue(() => !Instance.TaskManager.TaskStack.ContainsAny("WaitForTime"));
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Done!"));
        }

        public void StopAll()
        {
            TaskManager.Abort();
        }

        private void CommandHandler(string command, string args)
        {
            if (args == "config")
            {
                ConfigWindow.IsOpen = true;
            }
            else if (args == "home")
            {
                StartMainLoop();
            }
            else
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
            ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
        }

        public void DrawMainUI()
        {
            MainWindow.IsOpen = !MainWindow.IsOpen;
        }

    }
}
