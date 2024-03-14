using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
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
        var depositAll = this.configuration.DepositAll;
        var useSaddlebag = this.configuration.UseSaddlebag;
        var depositCrystals = this.configuration.DepositCrystals;
        var usePersonalHouse = this.configuration.UsePersonalHouse;
        var useFCHouse = this.configuration.UseFCHouse;
        var retardTest = this.configuration.retardTest; 

        if (ImGui.Checkbox("Deposit all items", ref depositAll))
        {
            config.DepositAll = depositAll;
        }
        else if (ImGui.Checkbox("retardTtest", ref retardTest))
        {
            this.configuration.retardTest = retardTest;
        }
        else if (ImGui.Checkbox("Use saddlebag", ref useSaddlebag))
        {
            config.UseSaddlebag = useSaddlebag;
        }
        else if (ImGui.Checkbox("Deposit crystals", ref depositCrystals))
        {
            config.DepositCrystals = depositCrystals;
        }
        else if (ImGui.Checkbox("Use personal house", ref usePersonalHouse))
        {
            config.UsePersonalHouse = usePersonalHouse;
        }
        else if (ImGui.Checkbox("Use FC house", ref useFCHouse))
        {
            config.UseFCHouse = useFCHouse;
        }
        config.Save();

    }
}
