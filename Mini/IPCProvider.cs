using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini;
public class IPCProvider
{
    public Dictionary<string, long> UnlockIPCRequests = [];
    private IPCProvider()
    {
        EzIPC.Init(this);
    }

    void RequestUnlockFPS(string name, int timeLimitMS)
    {
        UnlockIPCRequests[name] = Environment.TickCount64 + timeLimitMS;
    }

    void RemoveUnlockFPSRequest(string name)
    {
        UnlockIPCRequests.Remove(name);
    }
}
