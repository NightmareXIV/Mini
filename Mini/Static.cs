using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mini
{
    class Static
    {
        public const int SW_MINIMIZE = 6;
        public const int SW_FORCEMINIMIZE = 11;
        public static bool ImGuiIconButton(FontAwesomeIcon icon)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-MiniButton");
            ImGui.PopFont();
            return result;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        /*[DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);*/

        public static bool TryFindGameWindow(out IntPtr hwnd)
        {
            hwnd = IntPtr.Zero;
            while (true)
            {
                hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
                if (hwnd == IntPtr.Zero) break;
                GetWindowThreadProcessId(hwnd, out var pid);
                if (pid == Process.GetCurrentProcess().Id && IsWindowVisible(hwnd)) break;
            }
            return hwnd != IntPtr.Zero;
        }
    }
}
