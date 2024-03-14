using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Tasks
{
    internal unsafe static class SelectBellTask
    {
        internal static void Enqueue()
        {
            Instance.TaskManager.Enqueue(() =>
            {
                var bell = Helpers.GetClosestRetainerBell();
                if (!bell)
                {
                    DuoLog.Error("No retainer bell found");
                    Instance.TaskManager.Abort();
                }

                if (Vector3.Distance(bell.Position, Svc.ClientState.LocalPlayer.Position) > 4f)
                {
                    if (bell != null && Vector3.Distance(bell.Position, Svc.ClientState.LocalPlayer.Position) < 20f)
                    {
                        Instance.TaskManager.EnqueueImmediate(() => SetTarget(bell), "SetTarget");
                        Instance.TaskManager.EnqueueImmediate(() => Lockon(), "Lockon");
                        Instance.TaskManager.EnqueueImmediate(() => Approach(), "Approach");
                        Instance.TaskManager.EnqueueImmediate(() => AutorunOff(bell), "AutorunOff");
                    } else
                    {
                        DuoLog.Error("No retainer bell found close enough");
                        Instance.TaskManager.Abort();
                    }
                }
                Instance.TaskManager.EnqueueImmediate(() => Interact(bell), "Interact with bell");
            });
        }

        internal static bool SetTarget(GameObject bell)
        {
            if (EzThrottler.Throttle("SetTarget", 200))
            {
                Svc.Targets.Target = bell;
                return true;
            }
            return false;
        }

        internal static bool? Interact(GameObject bell)
        {
            if (EzThrottler.Throttle("Interact", 200))
            {
                TargetSystem.Instance()->InteractWithObject((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)bell.Address, false);
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
        internal static bool? AutorunOff(GameObject bell)
        {
            if (Vector3.Distance(bell.Position, Svc.ClientState.LocalPlayer.Position) < 4f && EzThrottler.Throttle("AutorunOff", 200))
            {
                Chat.Instance.SendMessage("/automove off");
                return true;
            }
            return false;
        }
    }
}
