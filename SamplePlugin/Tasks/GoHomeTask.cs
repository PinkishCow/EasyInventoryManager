using ClickLib.Clicks;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Linq;
using System.Numerics;

namespace EasyInventoryManager.Tasks
{
    internal unsafe class GoHomeTask
    {
        // A list of FCAetherytes and PrivateAetherytes
        static readonly uint[] FCAetherytes = [56, 57, 58, 96, 164];
        static readonly uint[] PrivateAetherytes = [59, 60, 61, 97, 165];

        internal static void Enqueue()
        {
            // Enqueue the teleportation to personal house or FC house based on the configuration
            Instance.TaskManager.Enqueue(() =>
            {
                if (config.UsePersonalHouse)
                {
                    Instance.TaskManager.EnqueueImmediate(() => TryTeleportToMultiple(PrivateAetherytes), $"Teleporting to personal house");
                }
                else if (config.UseFCHouse)
                {
                    Instance.TaskManager.EnqueueImmediate(() => TryTeleportToMultiple(FCAetherytes), $"Teleporting to FC house");
                }
                else
                {
                    DuoLog.Error("No house type selected");
                    Instance.TaskManager.Abort();
                }
            });

            // Set a timestamp to wait for a specific time
            var time = DateTimeOffset.Now.AddSeconds(6);
            Instance.TaskManager.Enqueue(() => Helpers.Helpers.waitUntilTimestamp(time), 1000 * 60, "WaitForTime");

            // Wait until the player is interactable and in one of the residential areas
            Instance.TaskManager.Enqueue(() => Player.Interactable && Svc.ClientState.TerritoryType.EqualsAny(ResidentalAreas.List), 1000 * 60, "WaitUntilArrival");

            // Log that the player has arrived at the house
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Arrived at house"));

            // Check if the reachable retainer bell is null
            Instance.TaskManager.Enqueue(() =>
            {
                if (Helpers.Helpers.GetReachableRetainerBell() == null)
                {
                    // Get the closest entrance and set it as the target
                    var entrance = Helpers.Helpers.GetClosestEntrance();
                    if (!entrance)
                    {
                        DuoLog.Error("Could not find entrance");
                        Instance.TaskManager.Abort();
                    }
                    Instance.TaskManager.EnqueueImmediate(() => SetTarget(entrance), "Set target");
                    Instance.TaskManager.EnqueueImmediate(Lockon, "Lockon");
                    Instance.TaskManager.EnqueueImmediate(Approach, "Approach");
                    Instance.TaskManager.EnqueueImmediate(() => AutorunOff(entrance), "AutorunOff");
                    Instance.TaskManager.EnqueueImmediate(() => { Chat.Instance.SendMessage("/automove off"); }, "Chat autorun off");

                    Instance.TaskManager.EnqueueImmediate(() => IsCloseEnough(entrance), "IsCloseEnough");
                    Instance.TaskManager.EnqueueImmediate(() => Interact(entrance), "Interact");
                    Instance.TaskManager.EnqueueImmediate(() => SelectYesno(), "YesNo");
                    Instance.TaskManager.EnqueueImmediate(() => WaitUntilLeavingZone(), "WaitTillLeavingZone");
                    Instance.TaskManager.DelayNextImmediate(60, true);
                }
                return true;
            });

            // Log that the player is inside the house
            Instance.TaskManager.Enqueue(() => DuoLog.Information("Inside house :)"));
        }

        // Set the target to the given entrance
        internal static bool SetTarget(GameObject entrance)
        {
            if (EzThrottler.Throttle("SetTarget", 200))
            {
                Svc.Targets.Target = entrance;
                return true;
            }
            return false;
        }

        // Check if the player is close enough to the entrance
        internal static bool? IsCloseEnough(GameObject entrance)
        {
            return Vector3.Distance(entrance.Position, Svc.ClientState.LocalPlayer.Position) < 4f;
        }

        // Interact with the entrance
        internal static bool? Interact(GameObject entrance)
        {
            if (EzThrottler.Throttle("Interact", 200))
            {
                TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entrance.Address, false);
                return true;
            }
            return false;
        }

        // Lock on to the target
        internal static bool? Lockon()
        {
            if (!EzThrottler.Throttle("Lockon", 200))
            {
                Chat.Instance.SendMessage("/lockon");
                return true;
            }
            return false;
        }

        // Enable autorun
        internal static bool? Approach()
        {
            Chat.Instance.SendMessage("/automove on");
            return true;
        }

        // Disable autorun when close to the entrance
        internal static bool? AutorunOff(GameObject entrance)
        {
            if (Vector3.Distance(entrance.Position, Svc.ClientState.LocalPlayer.Position) < 3f && EzThrottler.Throttle("AutorunOff", 200))
            {
                Chat.Instance.SendMessage("/automove off");
                return true;
            }
            return false;
        }

        // Wait until leaving the residential area
        internal static bool? WaitUntilLeavingZone()
        {
            return !ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType);
        }

        // Select "Yes" or "No" when prompted
        internal static bool? SelectYesno()
        {
            if (!ResidentalAreas.List.Contains(Svc.ClientState.TerritoryType))
            {
                return null;
            }
            var addon = Helpers.GetSpecificYesno(Helpers.ConfirmHouseEntrance);
            if (addon != null)
            {
                if (Helpers.IsAddonReady(addon) && EzThrottler.Throttle("SelectYesno"))
                {
                    DuoLog.Information("Select yes");
                    ClickSelectYesNo.Using((nint)addon).Yes();
                    return true;
                }
            }
            else
            {
                if (Helpers.TrySelectSpecificEntry(Helpers.GoToYourApartment, () => EzThrottler.Throttle("SelectYesno")))
                {
                    DuoLog.Information("Confirmed going to apartment");
                    return true;
                }
            }
            return false;
        }

        // Try to teleport to multiple aetherytes
        internal static bool? TryTeleportToMultiple(uint[] Aetherytes)
        {
            if (!Player.Available) return null;
            if (AgentMap.Instance()->IsPlayerMoving == 0 && !GenericHelpers.IsOccupied() && !Player.Object.IsCasting && EzThrottler.Throttle("GoHomeTP", 5000))
            {
                try
                {
                    foreach (var aetheryte in Aetherytes)
                    {
                        if (Svc.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport").InvokeFunc(aetheryte, 0))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    e.Log();
                    DuoLog.Error("Failed to teleport: " + e.Message);
                }
            }
            return false;
        }
    }
}
