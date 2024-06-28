using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ECommons.Interop.WindowFunctions;

namespace Mini;

internal static class Audio
{
    private class ChannelWrapper
    {
        private readonly string snMuted;  // Setting name: is channel muted
        private bool wasMuted;  // Channel was already muted when we minimized

        public ChannelWrapper(string name) => snMuted = name;

        public void Mute()
        {
            wasMuted = Svc.GameConfig.System.GetBool(snMuted);
            if (!wasMuted) Svc.GameConfig.System.Set(snMuted, true);
        }

        public void Restore()
        {
            if (!wasMuted) Svc.GameConfig.System.Set(snMuted, false);
        }
    }

    private static bool muted = false;
    public static readonly bool Debugging = false;
    public static Config config;  // Initialized in Mini constructor
    public enum Channel : int
    {
        Master,
        BGM,
        Sound_Effects,
        Voice,
        System_Sounds,
        Ambient_Sounds,
        Performance,
    }
    public static readonly IEnumerable<(string name, int index)> Channels = Enum.GetNames(typeof(Audio.Channel))
        .Select((name, index) => (name: name.Replace("_", " "), index));
    private static readonly ChannelWrapper[] channels =
    {
        new("IsSndMaster"),
        new("IsSndBgm"),
        new("IsSndSe"),
        new("IsSndVoice"),
        new("IsSndSystem"),
        new("IsSndEnv"),
        new("IsSndPerform"),
    };

    public static void Mute(bool testing = false)
    {
        if (muted) return;
        muted = true;
        for (var index = 0; index < channels.Length; index++)
        {
            if (config.MuteChannels.Contains(index))
            {
                channels[index].Mute();
                if (index == (int)Channel.Master) break;
            }
        }
        if (testing)
        {
            Task.Delay(3000).ContinueWith(_ => Unmute());
        }
        else
        {
            Svc.Framework.Update += WatchForActivation;
        }
    }

    public static void Unmute()
    {
        if (!muted) return;
        for (var index = 0; index < channels.Length; index++)
        {
            if (config.MuteChannels.Contains(index))
            {
                channels[index].Restore();
                if (index == (int)Channel.Master) break;
            }
        }
        muted = false;
    }

    private static void WatchForActivation(object _)
    {
        if (ApplicationIsActivated())
        {
            Svc.Framework.Update -= WatchForActivation;
            Unmute();
        }
    }
}
