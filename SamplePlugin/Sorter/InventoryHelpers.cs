using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using ECommons;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Sorter
{
    internal unsafe class InventoryHelpers
    {
        public struct EimInventory
        {
            public CharacterType characterType;
            public ulong characterId;
            public string characterName;
            public int inventoryFreeSlots;
            public int inventoryUsedSlots;
            public int inventoryMaxSlots;
        }

        public struct EimItem
        {
            public EimInventory* inventory;
            public InventoryItem item;
        }


        public static List<EimItem> GetAllInventoryData()
        {
            var itemList = new List<EimItem>();

            GetPlayerInventoryData(ref itemList);
            GetRetainerInventoryData(ref itemList);

            return itemList;
        }

        private static void GetPlayerInventoryData(ref List<EimItem> itemList)
        {
            InventoryType[] validBags = [InventoryType.Bag0, InventoryType.Bag1, InventoryType.Bag2, InventoryType.Bag3]; // Handle crystals seperately

            var inventory = inventoryMonitor.Inventories.Where(x => characterMonitor.Characters[x.Value.CharacterId].CharacterType.Equals(CharacterType.Character) && x.Value.CharacterId == characterMonitor.ActiveCharacterId).First().Value;
            //First is fine cause there should never be more than one player inventory

            processInventory(ref itemList, validBags, inventory);
        }

        private static void GetRetainerInventoryData(ref List<EimItem> itemList)
        {
            InventoryType[] validBags = [InventoryType.RetainerBag0, InventoryType.RetainerBag1, InventoryType.RetainerBag2, InventoryType.RetainerBag3, InventoryType.RetainerBag4]; // Handle crystals seperately    

            var inventories = inventoryMonitor.Inventories.Where(x => characterMonitor.Characters[x.Value.CharacterId].CharacterType.Equals(CharacterType.Retainer) && characterMonitor.Characters[x.Value.CharacterId].OwnerId == characterMonitor.ActiveCharacterId);

            foreach (var inventory in inventories)
            {
                processInventory(ref itemList, validBags, inventory.Value);
            }
        }

        private static void processInventory(ref List<EimItem> itemList, InventoryType[] validBags, Inventory inventory)
        {
            var eimInventory = new EimInventory
            {
                characterType = CharacterType.Character,
                characterId = inventory.CharacterId,
                characterName = characterMonitor.Characters[inventory.CharacterId].Name,
                inventoryFreeSlots = 0,
                inventoryUsedSlots = 0,
                inventoryMaxSlots = 0
            };

            List<InventoryItem> items = new List<InventoryItem>();

            foreach (var bag in validBags)
            {
                InventoryItem[] bagItems = inventory.GetInventoryByType(bag);

                if (bagItems == null)
                {
                    DuoLog.Error($"Bag {bag} is null");
                    continue;
                }

                eimInventory.inventoryFreeSlots += bagItems.Where(x => x == null).Count();
                eimInventory.inventoryUsedSlots += bagItems.Where(x => x != null).Count();
                eimInventory.inventoryMaxSlots += bagItems.Length;
                items.InsertRange(items.Count, bagItems.Where(x => x != null));
            }

            if (eimInventory.inventoryMaxSlots != eimInventory.inventoryFreeSlots + eimInventory.inventoryUsedSlots)
            {
                DuoLog.Error("Inventory slot count mismatch");
            }

            foreach (var item in items)
            {
                if (item != null)
                {
                    itemList.Add(new EimItem { inventory = &eimInventory, item = item });
                }
            }
        }

        
    }
}
