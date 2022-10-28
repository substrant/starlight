using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Starlight.Bootstrap;
using Starlight.Except;
using Starlight.Misc;
using Starlight.PostLaunch;
using static Starlight.Misc.Native;

namespace Starlight.Launch;

public class Launcher
{
    // ReSharper disable once PossibleNullReferenceException
    internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    /* Singleton */

    static Mutex _rbxSingleton;

    /// <summary>
    ///     Creates a new systemwide mutex that will allow multiple instances of Roblox from running by making the singleton
    ///     instance thread yield forever.
    /// </summary>
    public static void CommitSingleton()
    {
        _rbxSingleton = new Mutex(true, "ROBLOX_singletonMutex");
    }

    /// <summary>
    ///     Closes the systemwide mutex that was created by <see cref="CommitSingleton" />.
    /// </summary>
    public static void ReleaseSingleton()
    {
        // Releases on thread exit as well.
        _rbxSingleton.Dispose();
    }

    /* Launcher */

    /// <summary>
    ///     Launches Roblox with the specified arguments.
    /// </summary>
    /// <param name="info">The basic information for launching Roblox.</param>
    /// <param name="extras">The custom Starlight parameters for launching Roblox.</param>
    /// <returns>A <see cref="ClientInstance" /> class that governs the new instance of Roblox.</returns>
    /// <exception cref="System.Web.HttpException">Thrown when anything related to Roblox web services fails.</exception>
    /// <exception cref="ClientNotFoundException">The specfied client doesn't exist.</exception>
    /// <exception cref="PrematureCloseException">Roblox closed before Starlight could do anything with it.</exception>
    /// <exception cref="PostLaunchException">A post-launch task failed.</exception>
    public static ClientInstance Launch(LaunchParams info, IStarlightLaunchParams extras = null)
    {
        if (extras != null)
            info.Merge(extras);

        if (string.IsNullOrWhiteSpace(info.Hash))
            info.Hash = Bootstrapper.GetLatestHash();

        Log.Info($"Launch: Preparing to launch Roblox version-{info.Hash}...");

        // Spoof the trackers if spoofing is enabled
        if (info.Spoof)
        {
            info.TrackerId = Utility.SecureRandomInteger();
            info.LaunchTime = DateTime.Now;
            Log.Debug($"Launch: Spoofed BrowserTrackerId to {info.TrackerId} and LaunchTime to {info.LaunchTime}");
        }

        // Get client
        Log.Debug($"Launch: Querying {info.Hash}...");
        var client = Bootstrapper.QueryClient(info.Hash);

        // Open Roblox client
        Log.Debug("Launch: Native open...");
        if (!OpenRoblox(client.Player, info, out var procInfo))
        {
            var ex = new PrematureCloseException();
            Log.Fatal("Launch: Failed to open Roblox.", ex);
            throw ex;
        }

        ResumeThread(procInfo.hThread);

        // Create an instance
        ClientInstance inst;
        try
        {
            inst = new ClientInstance(procInfo.dwProcessId);
        }
        catch
        {
            var ex = new PrematureCloseException();
            Log.Fatal("Launch: Failed to initialize Roblox instance.", ex);
            throw ex;
        }

        // Wait for Roblox's window to open
        Log.Debug("Launch: Waiting for Roblox window...");

        IntPtr hWnd;
        var waitStart = DateTime.Now;
        while ((hWnd = inst.Proc.MainWindowHandle) == IntPtr.Zero)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
            if (DateTime.Now - waitStart <= TimeSpan.FromSeconds(10))
                continue;

            var ex = new PrematureCloseException();
            Log.Fatal("Launch: Roblox unexpectedly closed.", ex);
            throw ex;
        }

        Log.Debug("Roblox launched!");

        /* Runtime */

        // Set FPS cap
        if (info.FpsCap != 0)
        {
            inst.SetFrameDelay(1.0d / info.FpsCap);
            Log.Debug($"Launch: Set FPS cap to {info.FpsCap}.");
        }

        // Set resolution of Roblox
        if (!string.IsNullOrWhiteSpace(info.Resolution))
        {
            var res = Utility.ParseResolution(info.Resolution);
            if (res.HasValue)
            {
                var bounds = Utility.GetWindowBounds(hWnd);
                var screenBounds = Screen.PrimaryScreen.WorkingArea with { X = 0, Y = 0 };

                SetWindowPos(
                    hWnd,
                    IntPtr.Zero,
                    screenBounds.Right / 2 - bounds.Width / 2, // Center X
                    screenBounds.Bottom / 2 - bounds.Height / 2, // Center Y
                    res.Value.Item1,
                    res.Value.Item2,
                    SWP_NOOWNERZORDER | SWP_NOZORDER);
                SetWindowLong(hWnd, GWL_STYLE, WS_POPUPWINDOW); // Remove window styles (title bar, etc.)
                ShowWindow(hWnd, SW_SHOW);

                Log.Debug($"Launch: Set resolution to {info.Resolution}.");
            }
            else
            {
                Log.Error("Launch: Skipped setting resolution because parse failed.");
            }
        }

        // Enter headless mode
        if (info.Headless)
        {
            SendMessage(hWnd, WM_SYSCOMMAND, SC_MINIMIZE,
                IntPtr.Zero); // Just learned that minimize = no render :thumbsup:
            ShowWindow(hWnd, SW_HIDE);
            Log.Debug("Launch: Roblox window hidden. Client is now headless.");
        }

        // Attach
        if (info.AttachMethod != AttachMethod.None)
            inst.Attach(info.AttachMethod);

        Log.Debug("Launch: Finished post-launch.");
        Log.Info("Launch: Roblox launched.");

        return inst;
    }

    public static async Task<ClientInstance> LaunchAsync(LaunchParams info, IStarlightLaunchParams extras = null)
    {
        return await Task.Run(() => Launch(info, extras));
    }

    internal static bool OpenRoblox(string robloxPath, LaunchParams info, out PROCESS_INFORMATION procInfo)
    {
        STARTUPINFO startInfo = new();
        return CreateProcess(
            Path.GetFullPath(robloxPath),
            $"--play -a https://auth.roblox.com/v1/authentication-ticket/redeem -t {info.Ticket} -j {info.Request.Serialize()} -b {info.TrackerId} " + // Updated to another endpoint.
            $"--launchtime={info.LaunchTime.ToUnixTimeMilliseconds()} --rloc {info.RobloxLocale.Name} --gloc {info.GameLocale.Name}",
            0,
            0,
            false,
            ProcessCreationFlags.CREATE_SUSPENDED,
            0,
            null,
            ref startInfo,
            out procInfo);
    }
}