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

namespace Mini
{
    class Mini : IDalamudPlugin
    {
        internal DalamudPluginInterface pi;
        private const int SW_MINIMIZE = 6;

        public string Name => "Mini";

        public void Dispose()
        {
            pi.CommandManager.RemoveHandler("/mini");
            pi.UiBuilder.OnBuildUi -= Draw;
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            pi.CommandManager.AddHandler("/mini", new Dalamud.Game.Command.CommandInfo(delegate 
            {
                ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);
            }));
            pi.UiBuilder.OnBuildUi += Draw;
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
            if (ImGuiIconButton(FontAwesomeIcon.WindowMinimize))
            {
                ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);
            }
            WindowPos.X = ImGuiHelpers.MainViewport.Size.X - ImGui.GetColumnWidth();
            ImGui.End();
            ImGui.PopStyleVar(2);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool ImGuiIconButton(FontAwesomeIcon icon)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-MiniButton");
            ImGui.PopFont();
            return result;
        }
    }
}
