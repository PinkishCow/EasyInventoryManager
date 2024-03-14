using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game;
using ECommons.Logging; 
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

            var addonsToCheck = new[] { "RetainerSellList", "RetainerGrid0", "RetainerGrid1", "RetainerGrid2", "RetainerGrid3", "RetainerGrid4", "RetainerCrystalGrid" };
            foreach (var addonName in addonsToCheck)
            
                if (GenericHelpers.TryGetAddonByName<AtkUnitBase>(addonName, out var addon) && GenericHelpers.IsAddonReady(addon))

            {
                    DuoLog.Debug($"Checked 'addonName' and it worky");
                    return true; 
            }



            return false;
        }
    }
}
