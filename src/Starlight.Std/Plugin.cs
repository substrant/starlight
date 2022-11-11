using System;
using System.Security.Cryptography;
using System.Windows.Forms;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.Misc;
using Starlight.Plugins;
using Starlight.PostLaunch;

namespace Starlight.Std;

public class Plugin : PluginBase
{
    public override string Name => "Starlight.Std";

    public override string Author => "RealNickk";

    public override string Description => "Base plugin for Starlight's functionality";

    public override void Load()
    {
        /* Overloads */

        Sdk.SetDefaultValue<string>("versionHash", null);

        Sdk.SetDefaultValue<string>("clientScope", null);

        /* Spoofing */

        Sdk.SetDefaultValue("spoofTracker", true);
        Sdk.SetDefaultValue("resetTime", true);

        /* Display */

        Sdk.SetDefaultValue<int?>("maxFramerate", null);
        Sdk.SetDefaultValue<string>("resolution", null);
        Sdk.SetDefaultValue("headless", false);

        if (Sdk.UnsavedConfig)
            Sdk.SaveConfig();
    }

    public override void PreLaunch(LaunchParams info, ref Client client)
    {
        /* Overloads */

        if (Sdk.GetValue("versionHash", out string versionHash) && !string.IsNullOrWhiteSpace(versionHash))
        {
            client = new Client(versionHash, ClientScope.Local);
            if (!client.Exists)
                throw new ClientNotFoundException(client);
        }

        /* Spoofing */

        if (Sdk.GetValue("spoofTracker", out bool spoofTracker) && spoofTracker)
        {
            var seed = new byte[4];

            using var cryptoRng = new RNGCryptoServiceProvider();
            cryptoRng.GetBytes(seed);

            var rng = new Random(BitConverter.ToInt32(seed, 0));
            info.Request.BrowserTrackerId = rng.Next();
        }

        if (Sdk.GetValue("resetTime", out bool resetTime) && resetTime)
            info.LaunchTime = DateTime.Now;
    }

    public override void PostLaunch(ClientInstance inst)
    {
        /* Display */

        if (Sdk.GetValue("maxFramerate", out int? fpsCap) && fpsCap is not null)
            if (fpsCap == 0)
                inst.SetFrameDelay(1.0d / 1000); // lol
            else
                inst.SetFrameDelay(1.0d / fpsCap.Value);
    }

    public override void PostWindow(IntPtr hwnd)
    {
        /* Display */

        if (Sdk.GetValue("resolution", out string resValue) && !string.IsNullOrWhiteSpace(resValue))
        {
            var res = Utility.ParseResolution(resValue);
            if (res.HasValue)
            {
                // Get the bounds of the window and screen
                var bounds = Utility.GetWindowBounds(hwnd);
                var screenBounds = Screen.PrimaryScreen.WorkingArea with { X = 0, Y = 0 };

                // Center the window in the middle of the screen
                Native.SetWindowPos(
                    hwnd,
                    IntPtr.Zero,
                    screenBounds.Right / 2 - bounds.Width / 2, // Center X
                    screenBounds.Bottom / 2 - bounds.Height / 2, // Center Y
                    res.Value.X,
                    res.Value.Y,
                    Native.SwpNoOwnerZOrder | Native.SwpNoZOrder);

                // Remove title bar and other junk, then show it
                Native.SetWindowLong(hwnd, Native.GwlStyle, Native.WsPopupWindow);
                Native.ShowWindow(hwnd, Native.CmdShow.Show);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        if (Sdk.GetValue("headless", out bool isHeadless) && isHeadless)
        {
            Native.SendMessage(hwnd, Native.WmSysCommand, Native.ScMinimize,
                IntPtr.Zero); // Just learned that minimize = no render :thumbsup:
            Native.ShowWindow(hwnd, Native.CmdShow.Show);
        }
    }
}