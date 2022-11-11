using log4net;
using Starlight.Misc;
using Starlight.RbxApp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Starlight.Misc.Native;

namespace Starlight.Core
{
    public class Launcher
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /* Singleton */

        static Mutex _rbxSingleton;

        public static void CommitSingleton() =>
            _rbxSingleton = new Mutex(true, "ROBLOX_singletonMutex");

        public static void ReleaseSingleton() => // Releases on thread exit as well.
            _rbxSingleton.Dispose();

        /* Launcher */
        
        public static RbxInstance Launch(LaunchParams info, IStarlightLaunchParams extras = null)
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
            if (client is null)
            {
                var ex = new LaunchException("No valid client corresponds the given hash.");
                Log.Fatal("Launch: Client does not exist.", ex);
                throw ex;
            }

            // Open Roblox client
            Log.Debug("Launch: Native open...");
            if (!OpenRoblox(client.Path, info, out var procInfo))
            {
                var ex = new LaunchException("Failed to open Roblox application.");
                Log.Fatal("Launch: Failed to open Roblox.", ex);
                throw ex;
            }
            ResumeThread(procInfo.hThread);

            RbxInstance inst;
            try
            {
                inst = new RbxInstance(procInfo.dwProcessId);
            }
            catch
            {
                var ex = new LaunchException("Failed to initialize Roblox instance.");
                Log.Fatal("Launch: Failed to initialize Roblox instance. Roblox may have prematurely exited.", ex);
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

                var ex = new LaunchException("Roblox unexpectedly closed.");
                Log.Fatal("Launch: Roblox unexpectedly closed.", ex);
                throw ex;
            }

            Log.Debug("Roblox launched!");

            /* Runtime */

            // Set FPS cap
            // TODO: Disable rendering by hooking render job instead of limiting fps, will be more efficient.
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
                    Log.Error("Launch: Skipped setting resolution because parse failed.");
            }

            // Enter headless mode
            if (info.Headless)
            {
                ShowWindow(hWnd, SW_HIDE);
                Log.Debug("Launch: Roblox window hidden. Client is now headless.");
            }
            
            Log.Debug("Launch: Finished post-launch.");
            Log.Info("Launch: Roblox launched.");

            return inst;
        }

        public static async Task<RbxInstance> LaunchAsync(LaunchParams info, IStarlightLaunchParams extras = null) =>
            await Task.Run(() => Launch(info, extras));

        internal static bool OpenRoblox(string robloxPath, LaunchParams info, out PROCESS_INFORMATION procInfo)
        {
            STARTUPINFO startInfo = new();
            return CreateProcess(
                Path.GetFullPath(robloxPath),
                $"\"{Path.GetFullPath(robloxPath)}\" --app " +
                $"-t {info.Ticket} -j {info.Request.Serialize()} -b {info.TrackerId} " +
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
}
