using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Mini.Static;

namespace Mini
{
    class Mini : IDalamudPlugin
    {
        public string Name => "Mini";
        bool isHovered = false;
        bool open = false;
        Config config;
        Vector2 WindowPos = Vector2.Zero;

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
                TryMinimize();
            })
            {
                HelpMessage = "Minimize the game"
            });
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new Config();
            WindowPos.Y = config.OffestY;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { open = true; };
            SetAlwaysVisible(config.AlwaysVisible);
            //miniThread = new MiniThread(this);
        }

        void TryMinimize()
        {
            if (TryFindGameWindow(out var hwnd))
            {
                ShowWindow(hwnd, SW_MINIMIZE);
            }
            else
            {
                Svc.PluginInterface.UiBuilder.AddNotification("Failed to minimize game", "Mini", NotificationType.Error);
            }
        }

        void SetAlwaysVisible(bool value)
        {
            Svc.PluginInterface.UiBuilder.DisableAutomaticUiHide = value;
            Svc.PluginInterface.UiBuilder.DisableCutsceneUiHide = value;
            Svc.PluginInterface.UiBuilder.DisableGposeUiHide = value;
            Svc.PluginInterface.UiBuilder.DisableUserUiHide = value;
        }

        private void Draw()
        {
            if (config.DisplayButton)
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
                ImGui.SetWindowFontScale(config.Scale);
                if (config.TransparentButton && !isHovered)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, 0);
                    ImGui.PushStyleColor(ImGuiCol.Text, 0);
                }
                if (ImGuiIconButton(FontAwesomeIcon.WindowMinimize))
                {
                    TryMinimize();
                }
                if (config.TransparentButton && !isHovered) ImGui.PopStyleColor(2);
                isHovered = ImGui.IsItemHovered();
                if (config.Position == 0) // right
                {
                    WindowPos.X = ImGuiHelpers.MainViewport.Size.X - ImGui.GetWindowSize().X + config.OffestX;
                }
                else if(config.Position == 1) // left
                {
                    WindowPos.X = config.OffestX;
                }
                else // center
                {
                    WindowPos.X = ImGuiHelpers.MainViewport.Size.X / 2f - ImGui.GetWindowSize().X / 2f + config.OffestX;
                }
                ImGui.End();
                ImGui.PopStyleVar(2);
            }
            if (open)
            {
                if(ImGui.Begin("Mini configuration", ref open, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Checkbox("Enable minimize button", ref config.DisplayButton);
                    if (config.DisplayButton)
                    {
                        ImGui.SetNextItemWidth(100f);
                        ImGui.Combo("Position", ref config.Position, new string[] { "Right", "Left", "Center" }, 3);
                        ImGui.Checkbox("Button transparent unless hovered", ref config.TransparentButton);
                        ImGui.Checkbox("Button always visible", ref config.AlwaysVisible);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Enable this if you want button to show in cutscenes/gpose\neven if you have selected Dalamud to hide interface during it");
                        }
                        ImGui.SetNextItemWidth(100f);
                        ImGui.DragInt("X offset", ref config.OffestX, 1f);
                        ImGui.SetNextItemWidth(100f);
                        ImGui.DragInt("Y offset", ref config.OffestY, 1f);
                        WindowPos.Y = config.OffestY;
                        ImGui.SetNextItemWidth(100f);
                        ImGui.DragFloat("Scale", ref config.Scale, 0.02f, 0.1f, 50f);
                        if (config.Scale < 0.1f) config.Scale = 0.1f;
                        if (config.Scale > 50f) config.Scale = 50f;
                    }
                }
                ImGui.End();
                if (!open)
                {
                    Svc.PluginInterface.SavePluginConfig(config);
                    Svc.PluginInterface.UiBuilder.AddNotification("Configuration saved", "Mini", NotificationType.Success);
                    SetAlwaysVisible(config.AlwaysVisible);
                }
            }
        }
    }
}
