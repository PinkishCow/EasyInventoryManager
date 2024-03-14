using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;

namespace EasyInventoryManager.Retainer
{
    internal unsafe class RetainerInventoryManager
    {

        private RetainerManager retainerManager;

        private byte retainerCount { get; set; }

        public RetainerInventoryManager()
        {
            retainerManager = new RetainerManager();
            retainerCount = retainerManager.GetRetainerCount();
        }

        public static bool IsRetainerInventoryOpen()
        {
            if (!Svc.Condition[ConditionFlag.OccupiedSummoningBell]) return false;
            if (!Svc.Targets.Target!.IsRetainerBell()) return false;
            if (!Svc.Objects.Any(x => x.ObjectKind == ObjectKind.Retainer)) return false;

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSellList", out var addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerGrid0", out addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerGrid1", out addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerGrid2", out addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerGrid3", out addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerGrid4", out addon) && GenericHelpers.IsAddonReady(addon)) return true;
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerCrystalGrid", out addon) && GenericHelpers.IsAddonReady(addon)) return true;

            return false;
        }
    }
}
