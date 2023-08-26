using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Mini
{
    [Serializable]
    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; } =  1;
        public int Position = 0;
        public int OffestX = 0;
        public int OffestY = 0;
        public bool DisplayButton = true;
        public bool TransparentButton = false;
        public float Scale = 1f;
        public bool AlwaysVisible = true;
        public ClickBehavior LeftClickBehavior = ClickBehavior.Minimize;
        public ClickBehavior RightClickBehavior = ClickBehavior.None;
        public bool TrayNoActivate = false;
        public bool PermaTrayIcon = false;
        public bool LimitFpsWhenMini = false;
        public bool LimitFpsWhenMiniTray = false;
        public bool AlwaysOnTop = false;
        public bool MuteWhenMinimized = false;
        public bool MuteWhenInTrayOnly = false;
    }
}
