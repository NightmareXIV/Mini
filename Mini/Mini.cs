using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Mini.Static;

namespace Mini
{
    class Mini : IDalamudPlugin
    {
        public string Name => "Mini";
        bool isHovered = false;

        public void Dispose()
        {
            Svc.Commands.RemoveHandler("/mini");
            Svc.PluginInterface.UiBuilder.Draw -= Draw;
            //miniThread.Dispose();
        }

        public Mini(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            Svc.Commands.AddHandler("/mini", new CommandInfo(delegate
            {
                ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);
            })
            {
                HelpMessage = "Minimize the game"
            });
            Svc.PluginInterface.UiBuilder.Draw += Draw;
            //miniThread = new MiniThread(this);
        }

        Vector2 WindowPos = Vector2.Zero;
        private void Draw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(WindowPos);
            ImGui.Begin("MinimizeButton",
                ImGuiWindowFlags.AlwaysAutoResize
                | ImGuiWindowFlags.NoNavFocus
                | ImGuiWindowFlags.NoFocusOnAppearing
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.AlwaysAutoResize
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.AlwaysUseWindowPadding);
            if (!isHovered)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0);
                ImGui.PushStyleColor(ImGuiCol.Text, 0);
            }
            if (ImGuiIconButton(FontAwesomeIcon.WindowMinimize))
            {
                ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);
            }
            if (!isHovered) ImGui.PopStyleColor(2);
            isHovered = ImGui.IsItemHovered();
            WindowPos.X = ImGuiHelpers.MainViewport.Size.X - ImGui.GetColumnWidth();
            ImGui.End();
            ImGui.PopStyleVar(2);
        }
    }
}
