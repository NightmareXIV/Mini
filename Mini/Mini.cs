global using Dalamud.Interface.Utility;
global using ECommons.DalamudServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.Funding;
using ECommons.ImGuiMethods;
using ECommons.Interop;
using ECommons.Schedulers;
using ImGuiNET;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ECommons.Interop.WindowFunctions;
using static PInvoke.User32;

namespace Mini;

public class Mini : IDalamudPlugin
{
    public string Name => "Mini";

    private bool isHovered = false;
    private bool open = false;
    private Config config;
    private Vector2 WindowPos = Vector2.Zero;
    private NotifyIcon trayIcon = null;
    private bool FpsLimiterActive = false;
    private bool[] muted;

    public void Dispose()
    {
        Svc.Commands.RemoveHandler("/mini");
        Svc.PluginInterface.UiBuilder.Draw -= Draw;
        Audio.Unmute();
        TryDisposeTrayIcon();
        ECommonsMain.Dispose();
        //miniThread.Dispose();
    }

    public Mini(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);
        EzConfig.PluginConfigDirectoryOverride = "Mini";
        PatreonBanner.IsOfficialPlugin = () => true;
        new TickScheduler(delegate
        {
            PatreonBanner.IsOfficialPlugin = () => true;
            Svc.Commands.AddHandler("/mini", new CommandInfo(delegate (string command, string arguments)
            {
                if (arguments == "tray" || arguments == "t")
                {
                    TryMinimizeToTray();
                }
                else if (arguments == "config" || arguments == "settings" || arguments == "c" || arguments == "s")
                {
                    open = true;
                }
                else
                {
                    TryMinimize();
                }
            })
            {
                HelpMessage = "Minimize the game\n/mini <t|tray> → Minimize to tray\n/mini <c|config|s|settings> → Open settings"
            });
            EzConfig.Migrate<Config>();
            config = EzConfig.Init<Config>();
            WindowPos.Y = config.OffestY;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { open = true; };
            SetAlwaysVisible(config.AlwaysVisible);
            if (config.PermaTrayIcon)
            {
                new TickScheduler(delegate { CreateTrayIcon(false); });
            }
            // Init audio settings
            if (config.MuteChannels == null)
            {
                config.MuteChannels = new() { (int)Audio.Channel.Master };
            }
            Audio.config = config;
            muted = (bool[])Enum.GetValues(typeof(Audio.Channel))
                .Cast<int>()
                .Select(ch => config.MuteChannels.Contains(ch)).ToArray();
        });
        //miniThread = new MiniThread(this);
    }

    private void StartFpsLimiter()
    {
        if (!FpsLimiterActive
            && !Svc.Condition[ConditionFlag.BoundByDuty56]
            && !Svc.Condition[ConditionFlag.Crafting])
        {
            FpsLimiterActive = true;
            Svc.Framework.Update += LimitFps;
        }
    }

    private void LimitFps(object _)
    {
        Thread.Sleep(1000);
        if (ApplicationIsActivated())
        {
            Svc.Framework.Update -= LimitFps;
            FpsLimiterActive = false;
        }
    }

    private void TryMinimize()
    {
        if (WindowFunctions.TryFindGameWindow(out var hwnd))
        {
            User32.ShowWindow(hwnd, WindowShowStyle.SW_MINIMIZE);
            if (config.LimitFpsWhenMini) StartFpsLimiter();
            if (config.MuteWhenMinimized && !config.MuteWhenInTrayOnly) Audio.Mute();
        }
        else
        {
            Notify.Error("Failed to minimize game");
        }
    }

    private void TryMinimizeToTray()
    {
        if (TryFindGameWindow(out var hwnd))
        {
            if (!config.PermaTrayIcon)
            {
                CreateTrayIcon();
            }
            ShowWindow(hwnd, SW_HIDE);
            if (config.LimitFpsWhenMiniTray) StartFpsLimiter();
            if (config.MuteWhenMinimized) Audio.Mute();
        }
        else
        {
            Notify.Error("Failed to minimize game");
        }
    }

    private void CreateTrayIcon(bool ephemeral = true)
    {
        Icon icon;
        try
        {
            icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (Exception)
        {
            icon = SystemIcons.Application;
        }
        TryDisposeTrayIcon();
        trayIcon = new NotifyIcon()
        {
            Icon = icon,
            Text = $"[Mini] Final Fantasy XIV #{Process.GetCurrentProcess().Id}",
            Visible = true,
        };
        trayIcon.Click += delegate
        {
            if (TryFindGameWindow(out var tHwnd))
            {
                ShowWindow(tHwnd, config.TrayNoActivate ? WindowShowStyle.SW_SHOWNA : WindowShowStyle.SW_SHOW);
                if (ephemeral)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null;
                }
            }
        };
    }

    private bool TryDisposeTrayIcon()
    {
        if (trayIcon != null)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayIcon = null;
            return true;
        }
        return false;
    }

    private void SetAlwaysVisible(bool value)
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

            if (config.AlwaysOnTop)
            {
                Native.igBringWindowToDisplayFront(Native.igGetCurrentWindow());
            }
            ImGui.SetWindowFontScale(config.Scale);
            if (config.TransparentButton && !isHovered)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0);
                ImGui.PushStyleColor(ImGuiCol.Text, 0);
            }
            if (ImGuiEx.IconButton(FontAwesomeIcon.WindowMinimize))
            {
                if (config.LeftClickBehavior == ClickBehavior.Minimize)
                {
                    TryMinimize();
                }
                else if (config.LeftClickBehavior == ClickBehavior.Minimize_to_tray)
                {
                    TryMinimizeToTray();
                }
            }
            if (config.TransparentButton && !isHovered) ImGui.PopStyleColor(2);
            isHovered = ImGui.IsItemHovered();
            if (isHovered && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                //right clicked
                if (config.RightClickBehavior == ClickBehavior.Minimize)
                {
                    TryMinimize();
                }
                else if (config.RightClickBehavior == ClickBehavior.Minimize_to_tray)
                {
                    TryMinimizeToTray();
                }
            }
            if (config.Position == 0) // right
            {
                WindowPos.X = ImGuiHelpers.MainViewport.Size.X - ImGui.GetWindowSize().X + config.OffestX;
            }
            else if (config.Position == 1) // left
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
            if (ImGui.Begin("Mini configuration", ref open, ImGuiWindowFlags.AlwaysAutoResize))
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
                    ImGui.SetNextItemWidth(100f);
                    ImGuiEx.EnumCombo("Left click behavior", ref config.LeftClickBehavior);
                    ImGui.SetNextItemWidth(100f);
                    ImGuiEx.EnumCombo("Right click behavior", ref config.RightClickBehavior);
                    ImGui.Checkbox("Don't activate while restoring from tray", ref config.TrayNoActivate);
                    if (ImGui.Checkbox("Create permanent tray icon", ref config.PermaTrayIcon))
                    {
                        if (config.PermaTrayIcon)
                        {
                            CreateTrayIcon(false);
                        }
                        else
                        {
                            TryDisposeTrayIcon();
                        }
                    }
                    ImGui.Checkbox("Limit FPS to 1 while minimized to taskbar", ref config.LimitFpsWhenMini);
                    ImGui.Checkbox("Limit FPS to 1 while minimized to tray", ref config.LimitFpsWhenMiniTray);
                    ImGui.Text("   - FPS will not be limited while in duty or crafting");
                    ImGui.Text("   - Unminimizing may take a second with this option");
                    ImGui.Checkbox("Minimize button always on top", ref config.AlwaysOnTop);
                }

                if (Audio.Debugging && ImGui.Button("Test minimization muting")) Audio.Mute(true);
                ImGui.Checkbox("Mute audio when minimized", ref config.MuteWhenMinimized);
                if (config.MuteWhenMinimized)
                {
                    ImGui.Indent();
                    ImGui.Checkbox("Mute only when in tray", ref config.MuteWhenInTrayOnly);
                    ImGui.Checkbox("Mute all channels", ref muted[(int)Audio.Channel.Master]);
                    if (!muted[(int)Audio.Channel.Master])
                    {
                        config.MuteChannels.Remove((int)Audio.Channel.Master);
                        // Individual channels selector
                        ImGui.Indent();
                        foreach ((var name, var index) in Audio.Channels)
                        {
                            if (index == (int)Audio.Channel.Master) continue;
                            ImGui.Checkbox(name, ref muted[index]);
                            if (!muted[index])
                            {
                                config.MuteChannels.Remove(index);
                            }
                            else if (!config.MuteChannels.Contains(index))
                            {
                                config.MuteChannels.Add(index);
                            }
                        }
                        ImGui.Unindent();
                    }
                    else if (!config.MuteChannels.Contains((int)Audio.Channel.Master))
                    {
                        config.MuteChannels.Add((int)Audio.Channel.Master);
                    }
                    ImGui.Unindent();
                }
            }
            ImGuiEx.LineCentered("donate", PatreonBanner.DrawRaw);
            ImGui.End();
            if (!open)
            {
                EzConfig.Save();
                Notify.Success("Configuration saved");
                SetAlwaysVisible(config.AlwaysVisible);
            }
        }
    }
}
