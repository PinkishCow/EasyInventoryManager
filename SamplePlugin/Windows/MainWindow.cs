using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EasyInventoryManager.Windows;

public class MainWindow : Window, IDisposable
{
    private EasyInventoryManager plugin;

    public MainWindow(EasyInventoryManager plugin) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            this.plugin.DrawConfigUI();
        }
        else if (ImGui.Button("Start"))
        {
            Instance.StartMainLoop();
        }
        else if (ImGui.Button("Stop"))
        {
            Instance.StopAll();
        }
        else if (ImGui.Button("InvTest"))
        {
            Instance.InvTest();
        }
    }
}
