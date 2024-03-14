using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EasyInventoryManager.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;

    public ConfigWindow(EasyInventoryManager plugin) : base(
        "A Wonderful Configuration Window",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse
        )
    {
        this.Size = new Vector2(232, 75);
        this.SizeCondition = ImGuiCond.Always;

        this.configuration = plugin.Configuration;
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

        if (ImGui.Checkbox("Deposit all items", ref depositAll))
        {
            this.configuration.DepositAll = depositAll;
        }
        else if (ImGui.Checkbox("Use saddlebag", ref useSaddlebag))
        {
            this.configuration.UseSaddlebag = useSaddlebag;
        }
        else if (ImGui.Checkbox("Deposit crystals", ref depositCrystals))
        {
            this.configuration.DepositCrystals = depositCrystals;
        }
        else if (ImGui.Checkbox("Use personal house", ref usePersonalHouse))
        {
            this.configuration.UsePersonalHouse = usePersonalHouse;
        }
        else if (ImGui.Checkbox("Use FC house", ref useFCHouse))
        {
            this.configuration.UseFCHouse = useFCHouse;
        }
        this.configuration.Save();

    }
}
