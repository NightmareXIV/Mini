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
using ECommons.GameFunctions;

namespace Mini;

public unsafe class RenderSuppressionManager : IDisposable
{
    //Client::System::Framework::Framework_TaskRenderGraphicsRender()
    public uint* FrameCounter = &Framework.Instance()->FrameCounter;
    public bool Test = false;

    public void Tick(object _)
    {
        if(P.config.EnableDisableIconicRender)
        {
            if((Test || TerraFX.Interop.Windows.Windows.IsIconic((TerraFX.Interop.Windows.HWND)(*ECommonsMain.MainWindowHandle))) && (P.config.RenderEvery < 2 || * FrameCounter % P.config.RenderEvery != 0))
            {
                RenderDisableManager.PlaceRequest();
            }
            else
            {
                RenderDisableManager.RemoveRequest();
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
        RenderDisableManager.RemoveRequest();
    }
}
