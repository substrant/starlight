using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.Misc;
using Starlight.PostLaunch;

namespace Starlight.Plugins;

public class Standard : PluginBase
{
    const int SwShow = 5;
    
    const int SwHide = 0;
    
    const int WmSysCommand = 0x112;
    
    const int ScMinimize = 0xF020;
    
    const uint SwpNoZOrder = 0x0004;

    const uint SwpNoOwnerZOrder = 0x0200;

    const int GwlStyle = -16;

    const uint WsPopupWindow = 0x80880000;

    public override string Name => "Standard";

    public override string Author => "RealNickk";

    public override string Description => "Base plugin for Starlight's functionality";

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    public override void Load()
    {
        /* Overloads */

        Api.SetDefaultValue<string>("versionHash", null);

        /* Spoofing */

        Api.SetDefaultValue("spoofTracker", true);
        Api.SetDefaultValue("resetTime", true);

        /* Display */

        Api.SetDefaultValue<long>("maxFramerate", 0);
        Api.SetDefaultValue<string>("resolution", null);
        Api.SetDefaultValue("headless", false);

        Api.SaveConfig();
    }

    public override void PreLaunch(LaunchParams info, ref Client client)
    {
        /* Overloads */

        if (Api.GetValue("versionHash", out string versionHash) && !string.IsNullOrWhiteSpace(versionHash))
            if ((client = Bootstrapper.QueryClient(versionHash, ClientScope.Local)) is null)
                throw new NotImplementedException();

        /* Spoofing */

        if (Api.GetValue("spoofTracker", out bool spoofTracker) && spoofTracker)
            info.TrackerId = Utility.SecureRandomInteger();

        if (Api.GetValue("resetTime", out bool resetTime) && resetTime)
            info.LaunchTime = DateTime.Now;
    }

    public override void PostLaunch(ClientInstance inst)
    {
        /* Display */

        if (Api.GetValue("maxFramerate", out long fpsCap) && fpsCap is not 0)
            inst.SetFrameDelay(1.0d / fpsCap);
    }

    public override void PostWindow(IntPtr hwnd)
    {
        /* Display */

        if (Api.GetValue("resolution", out string resValue) && !string.IsNullOrWhiteSpace(resValue))
        {
            var res = Utility.ParseResolution(resValue);
            if (res.HasValue)
            {
                var bounds = Utility.GetWindowBounds(hwnd);
                var screenBounds = Screen.PrimaryScreen.WorkingArea with { X = 0, Y = 0 };

                SetWindowPos(
                    hwnd,
                    IntPtr.Zero,
                    screenBounds.Right / 2 - bounds.Width / 2, // Center X
                    screenBounds.Bottom / 2 - bounds.Height / 2, // Center Y
                    res.Value.X,
                    res.Value.Y,
                    SwpNoOwnerZOrder | SwpNoZOrder);
                SetWindowLong(hwnd, GwlStyle, WsPopupWindow); // Remove window styles (title bar, etc.)
                ShowWindow(hwnd, SwShow);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        if (Api.GetValue("headless", out bool isHeadless) && isHeadless)
        {
            SendMessage(hwnd, WmSysCommand, ScMinimize,
                IntPtr.Zero); // Just learned that minimize = no render :thumbsup:
            ShowWindow(hwnd, SwHide);
        }
    }
}