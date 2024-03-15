using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Utility;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Events;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Helpers
{
    internal unsafe static class RetainerListHelpers
    {
        internal static bool? SelectRetainerBySortedIndex(int retainerIndex)
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && GenericHelpers.IsAddonReady(retainerList))
            {
                if (EzThrottler.Throttle("GenericThrottle", 500))
                {
                    DuoLog.Information($"Clicking retainer {retainerIndex}");
                    ClickRetainerList.Using((IntPtr)retainerList).Retainer(retainerIndex);
                    return true;
                }
            }
            return false;
        }

        internal static bool? CloseRetainerList()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && GenericHelpers.IsAddonReady(retainerList))
            {
                if (EzThrottler.Throttle("GenericThrottle", 500))
                {
                    var v = stackalloc AtkValue[1]
                    {
                        new()
                        {
                        Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                        Int = -1
                        }
                    };
                    retainerList->FireCallback(1, v);
                    DuoLog.Information($"Closing retainer window");
                    return true;
                }
            }
            return false;
        }

        internal static bool CheckRetainerAvailable(out string name)
        {
            if (Svc.Condition[ConditionFlag.OccupiedSummoningBell] && ProperOnLogin.PlayerPresent && Svc.Objects.Where(x => x.ObjectKind == ObjectKind.Retainer).OrderBy(x => Vector3.Distance(Svc.ClientState.LocalPlayer.Position, x.Position)).TryGetFirst(out var obj))
            {
                name = obj.Name.ToString();
                return true;
            }
            name = default;
            return false;
        }
    }
}
