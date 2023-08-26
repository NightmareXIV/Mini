using System.Collections.Generic;
using System.Threading.Tasks;
using static ECommons.Interop.WindowFunctions;

namespace Mini
{
    static internal class Audio
    {
        public static readonly bool Debugging = false;
        static bool muted = false;

        class ChannelWrapper
        {
            readonly string snMuted;  // Setting name: is channel muted
            bool wasMuted;  // Channel was already muted when we minimized

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

        static readonly List<ChannelWrapper> Channels = new()
        {
            new ChannelWrapper("IsSndMaster"),
        };

        public static void Mute()
        {
            if (muted) return;
            muted = true;
            Channels.ForEach(ch => ch.Mute());
            Svc.Framework.Update += WatchForActivation;
        }

        public static void Unmute()
        {
            if (!muted) return;
            Channels.ForEach(ch => ch.Restore());
            muted = false;
        }

        static void WatchForActivation(object _)
        {
            if (ApplicationIsActivated())
            {
                Svc.Framework.Update -= WatchForActivation;
                Task.Delay(Debugging ? 3000 : 0).ContinueWith(_ => Unmute());
            }
        }
    }
}
