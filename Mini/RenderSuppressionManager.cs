using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ECommons;
using ECommons.EzHookManager;
using ECommons.EzIpcManager;
using ECommons.Interop;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using static Mini.Mini;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace Mini;

public unsafe class RenderSuppressionManager : IDisposable
{
    //Client::System::Framework::Framework_TaskRenderGraphicsRender()
    public byte* RenderDisabled => (byte*)(((nint)Manager.Instance()) + 230232);
    public uint* FrameCounter = &Framework.Instance()->FrameCounter;
    public nint* Handle = (nint*)Svc.SigScanner.GetStaticAddressFromSig("48 89 1D ?? ?? ?? ?? 48 8B CB FF 15");
    public bool Test = false;

    public void Tick(object _)
    {
        if(P.config.EnableDisableIconicRender)
        {
            if((Test || TerraFX.Interop.Windows.Windows.IsIconic((TerraFX.Interop.Windows.HWND)(*Handle))) && (P.config.RenderEvery < 2 || * FrameCounter % P.config.RenderEvery != 0))
            {
                *RenderDisabled = 1;
            }
            else
            {
                *RenderDisabled = 0;
            }
        }
    }

    private RenderSuppressionManager()
    {
        Svc.Framework.Update += Tick;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= Tick;
        *RenderDisabled = 0;
    }
}
