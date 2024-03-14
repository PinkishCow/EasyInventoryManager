using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EasyInventoryManager.Windows;

public class MainWindow : Window, IDisposable
{
    private IDalamudTextureWrap goatImage;
    private EasyInventoryManager plugin;

    public MainWindow(EasyInventoryManager plugin, IDalamudTextureWrap goatImage) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.goatImage = goatImage;
        this.plugin = plugin;
    }

    public void Dispose()
    {
        this.goatImage.Dispose();
    }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            this.plugin.DrawConfigUI();
        }

        ImGui.Spacing();

        ImGui.Text("Have a goat:");
        ImGui.Indent(55);
        ImGui.Image(this.goatImage.ImGuiHandle, new Vector2(this.goatImage.Width, this.goatImage.Height));
        ImGui.Unindent(55);
    }
}
