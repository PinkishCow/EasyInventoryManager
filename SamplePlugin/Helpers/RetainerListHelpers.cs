using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Utility;
using EasyInventoryManager.Retainer;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Events;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
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
                if (EzThrottler.Throttle("SelectRetainerBySortedIndex", 1000))
                {
                    DuoLog.Information($"Clicking retainer {retainerIndex}");
                    ClickRetainerList.Using((IntPtr)retainerList).Retainer(retainerIndex);
                    return true;

                }
            }
            return false;
        }

        internal static bool? ClickTalkBox()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Talk", out var talk) && GenericHelpers.IsAddonReady(talk))
            {
                if (EzThrottler.Throttle("ClickTalkBox", 1000))
                {
                    ClickTalk.Using((IntPtr)talk).Click();
                    return true;

                }
            }
            return false;
        }

        // Close the retainer window
        internal static bool? CloseRetainer()
        {

            if (GenericHelpers.TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && GenericHelpers.IsAddonReady(&addon->AtkUnitBase))
            {
                if (EzThrottler.Throttle("CloseRetainer", 1000))
                {
                    DuoLog.Information($"Closing retainer window");
                    return Helpers.TrySelectSpecificEntry([Helpers.quitRetainerString]);
                }
            }
            return false;
        }

        internal static bool? CloseRetainerList()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && GenericHelpers.IsAddonReady(retainerList))
            {
                if (EzThrottler.Throttle("CloseRetainerList", 1000))
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
                    DuoLog.Information($"Closing retainer list window");
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
                DuoLog.Information($"Retainer {name} is available");
                return true;
            }
            name = default;
            return false;
        }

        public unsafe static List<Item> GetAllItemsInInv()
        {
            var items = new List<Item>();
            var inventoryManager = InventoryManager.Instance();

            var inventoryContainer = inventoryManager->GetInventoryContainer(InventoryType.Inventory1);

            if (inventoryContainer != null && inventoryContainer->Loaded != 0)
            {
                for (var i = 0; i < inventoryContainer->Size; i++)
                {
                    var slot = inventoryContainer->GetInventorySlot(i);
                    if (slot != null && slot->ItemID != 0)
                    {
                        var item = new Item
                        {
                            itemId = slot->ItemID,
                            quantity = slot->Quantity
                        };
                        items.Add(item);
                    }
                }
            }
            return items;
        }
    }
}
