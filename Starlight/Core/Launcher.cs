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

            Log.Info($"Preparing to launch Roblox version-{info.Hash}...");

            // Spoof the trackers if spoofing is enabled
            if (info.Spoof)
            {
                info.TrackerId = Utility.SecureRandomInteger();
                info.LaunchTime = DateTime.Now;
                Log.Debug($"Spoofed BrowserTrackerId to {info.TrackerId} and LaunchTime to {info.LaunchTime}");
            }

            // Get client
            Log.Debug($"Querying {info.Hash}...");
            var client = Bootstrapper.QueryClient(info.Hash);
            if (client is null)
            {
                var ex = new LaunchException("No valid client corresponds the given hash.");
                Log.Fatal("Client does not exist.", ex);
                throw ex;
            }

            // Open Roblox client
            Log.Debug("Native open...");
            if (!OpenRoblox(client.Path, info, out var procInfo))
            {
                var ex = new LaunchException("Failed to open Roblox application.");
                Log.Fatal("Failed to open Roblox.", ex);
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
                Log.Fatal("Failed to initialize Roblox instance. Roblox may have prematurely exited.", ex);
                throw ex;
            }

            /* Runtime */

            // Set FPS cap
            if (info.FpsCap != 0)
            {
                inst.SetFrameDelay(1.0d / info.FpsCap);
                Log.Debug($"Set FPS cap to {info.FpsCap}.");
            }

            // Wait for Roblox's window to open
            // TODO: Disable rendering by hooking render job instead of limiting fps, will be more efficient.
            Log.Debug("Waiting for Roblox window...");
            while (inst.Proc.MainWindowHandle == IntPtr.Zero)
                Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
            var hWnd = inst.Proc.MainWindowHandle;

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
                    
                    Log.Debug($"Set resolution to {info.Resolution}.");
                }
                else
                    Log.Error("Skipped setting resolution because parse failed.");
            }

            // Enter headless mode
            if (info.Headless)
            {
                ShowWindow(hWnd, SW_HIDE);
                Log.Debug("Roblox window hidden. Client is now headless.");
            }

            Log.Info("Roblox has launched.");

            return inst;
        }

        public static async Task<RbxInstance> LaunchAsync(LaunchParams info, IStarlightLaunchParams extras = null) =>
            await Task.Run(() => Launch(info, extras));

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
}
