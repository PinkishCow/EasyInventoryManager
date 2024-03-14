using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using EasyInventoryManager.Retainer;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace EasyInventoryManager.Windows;

public class ConfigWindow : Window, IDisposable
{

    public ConfigWindow(EasyInventoryManager plugin) : base(
        "A Wonderful Configuration Window"
        )
    {
        this.SizeConstraints = new()
        {
            MinimumSize = new(250, 500),
            MaximumSize = new(9999, 9999)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var depositAll = config.DepositAll;
        var useSaddlebag = config.UseSaddlebag;
        var depositCrystals = config.DepositCrystals;
        var usePersonalHouse = config.UsePersonalHouse;
        var useFCHouse = config.UseFCHouse;
        var retardTest = config.retardTest;
        var iSlots = config.iSlots;
        var retSlots = config.retSlots;
        var retainerCount = config.retainerCount;
        var getInvItems = config.getInvItems;


        if (ImGui.Checkbox("Deposit all items", ref depositAll))
        {
            config.DepositAll = depositAll;
        }
        else if (ImGui.Checkbox("retardTest", ref retardTest))
        {
            config.retardTest = retardTest;
            RetainerInventoryManager.IsRetainerInventoryOpen();
        }
        else if (ImGui.Checkbox("Use saddlebag", ref useSaddlebag))
        {
            config.UseSaddlebag = useSaddlebag;
        }
        else if (ImGui.Checkbox("Get iSlots", ref iSlots))
        {
            config.iSlots = iSlots;
            RetainerInventoryManager.GetInventoryRemainingSpace();
        }
        else if (ImGui.Checkbox("Get retSlots", ref retSlots))
        {
            config.retSlots = retSlots;
            RetainerInventoryManager.GetRetainerRemainingSpace();
        }
        else if (ImGui.Checkbox("Deposit crystals", ref depositCrystals))
        {
            config.DepositCrystals = depositCrystals;
        }
        else if (ImGui.Checkbox("Test Inventory Array", ref depositCrystals))
        {
            config.DepositCrystals = depositCrystals;
        }
        else if (ImGui.Checkbox("retainerCount", ref retainerCount))
        {
            config.retainerCount = retainerCount;
            RetainerInventoryManager.GetAvailableRetainerCount();
        }
        else if (ImGui.Checkbox("Use personal house", ref usePersonalHouse))
        {
            config.UsePersonalHouse = usePersonalHouse;
        }
        else if (ImGui.Checkbox("Use FC house", ref useFCHouse))
        {
            config.UseFCHouse = useFCHouse;
        }
        else if (ImGui.Checkbox("getInvItems", ref getInvItems))
        {
            config.getInvItems = getInvItems;
            RetainerInventoryManager.PrintGarbageInDebug();
        }
        config.Save();

    }
}
