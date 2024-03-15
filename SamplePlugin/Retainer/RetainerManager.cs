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
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.Interop.Attributes;
using ImGuiNET;
using System.Reflection.Metadata.Ecma335;
using EasyInventoryManager.Helpers;

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

        public static void PrintGarbageInDebug()
        {
            List<Item> allItems = RetainerInventory.GetAllItemsInInv();
            foreach (var item in allItems)
            {
                DuoLog.Debug($"Item ID: {item.ItemID}, Quantity: {item.Quantity}");
            }
        }

        public static bool IsRetainerInventoryOpen()
        {
            if (!Svc.Condition[ConditionFlag.OccupiedSummoningBell]) return false;
            if (!Svc.Targets.Target!.IsRetainerBell()) return false;
            if (!Svc.Objects.Any(x => x.ObjectKind == ObjectKind.Retainer)) return false;

            var addonsToCheck = new[] { "RetainerGrid0", "RetainerGrid1", "RetainerGrid2", "RetainerGrid3", "RetainerGrid4", "RetainerCrystalGrid" };
            foreach (var addonName in addonsToCheck)

                if (GenericHelpers.TryGetAddonByName<AtkUnitBase>(addonName, out var addon) && GenericHelpers.IsAddonReady(addon))

                {
                    // This is wrong, we have to figure out how to do this correctly
                    DuoLog.Debug("Checked 'addonName and' it worky");
                    return true;

                }

            return false;
        }

        public static int GetInventoryRemainingSpace()
        {

            var empty = 0;
            foreach (var i in new[] { InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4 })
            {
                var c = InventoryManager.Instance()->GetInventoryContainer(i);
                if (c == null) continue;
                if (c->Loaded == 0) continue;
                for (var s = 0; s < c->Size; s++)
                {
                    var slot = c->GetInventorySlot(s);
                    if (slot->ItemID == 0) empty++;
                }
            }
            DuoLog.Debug("There are " + empty + " slots in your inventory");
            return empty;
        }

        // Define a method to get the remaining space in the retainer's inventory
        public static int GetRetainerRemainingSpace()
        {
            var empty = 0;
            // For some reason, there are 7 pages in the game, each with 25 slots, instead of the displayed 5 pages of 35 slots each
            foreach (var retainerType in new[] {
                InventoryType.RetainerPage1,
                InventoryType.RetainerPage2,
                InventoryType.RetainerPage3,
                InventoryType.RetainerPage4,
                InventoryType.RetainerPage5,
                InventoryType.RetainerPage6,
                InventoryType.RetainerPage7
              })
            {
                var c = InventoryManager.Instance()->GetInventoryContainer(retainerType);
                if (c == null)
                {
                    // Handle error or log the issue and continue to the next iteration
                    continue;
                }

                if (c->Loaded == 0) continue;

                // Iterate over the slots in the container
                for (var s = 0; s < c->Size; s++)
                {
                    var slot = c->GetInventorySlot(s);
                    // Check if the slot is empty (ItemID == 0)
                    if (slot->ItemID == 0) empty++;
                }

            }

            // Log the number of empty slots in the retainer bags
            DuoLog.Debug($"Open spaces in your retainer {empty} KILL YOURSELF");
            // Return the total count of empty slots
            return empty;

        }
        public static int GetAvailableRetainerCount()
        {
            var retainerCount = FFXIVClientStructs.FFXIV.Client.Game.RetainerManager.Instance()->GetRetainerCount();
            DuoLog.Debug($"PENIS {retainerCount} KILL YOURSELF");
            return retainerCount;
        }

    }

    public static class RetainerInventory
    {

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
                            ItemID = slot->ItemID,
                            Quantity = slot->Quantity
                        };
                        items.Add(item);
                    }
                }
            }
            return items;
        }

    }
    public struct Item
    {
        public uint ItemID { get; set; }
        public uint Quantity { get; set; }

    }
}
