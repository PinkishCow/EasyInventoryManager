using Dalamud.Game.ClientState.Statuses;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.Events;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Tasks
{
    internal unsafe class GoHomeTask
    {
        // You only have one of these if you have a house there, and it will plonk you outside of it :)
        static readonly uint[] FCAetherytes = [56, 57, 58, 96, 164];
        static readonly uint[] PrivateAetherytes = [59, 60, 61, 97, 165];

        internal static void Enqueue()
        {
            Instance.TaskManager.Enqueue(() =>
            {
                if (config.UsePersonalHouse)
                {
                    Instance.TaskManager.EnqueueImmediate(() => TryTeleportToMultiple(PrivateAetherytes), $"Teleporting to personal house");
                }
                else if (config.UseFCHouse)
                {
                    Instance.TaskManager.EnqueueImmediate(() => TryTeleportToMultiple(FCAetherytes), $"Teleporting to FC house");
                } else
                {
                    DuoLog.Error("No house type selected");
                    Instance.TaskManager.Abort();
                }
                Instance.TaskManager.EnqueueImmediate(() => Player.Interactable && Svc.ClientState.TerritoryType.EqualsAny(ResidentalAreas.List), 1000 * 60, "WaitUntilArrival");
                
            });
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Arrived at house"));

            Instance.TaskManager.Enqueue(() =>
            {
                if(Helpers.GetReachableRetainerBell() == null)
                {
                    var entrance = 
                }
            });


        }

        internal static bool? TryTeleportToMultiple(uint[] Aetherytes)
        {
            if (!Player.Available) return null;
            if (AgentMap.Instance()->IsPlayerMoving == 0 && !GenericHelpers.IsOccupied() && !Player.Object.IsCasting && EzThrottler.Throttle("GoHomeTP", 5000))
            {
                try
                {
                    foreach(var aetheryte in Aetherytes)
                    {
                        if(Svc.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport").InvokeFunc(aetheryte, 0))
                        {
                            return true;
                        }
                    }
                } catch(Exception e)
                {
                    e.Log();
                    DuoLog.Error("Failed to teleport: " + e.Message);
                }
            }
            return false;
        }
    }
}
