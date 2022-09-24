using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using static Starlight.Native;
using static Starlight.Shared;

namespace Starlight
{
    public interface IRobloxLaunchParams
    {
        public string Token { get; set; }

        public DateTimeOffset LaunchTime { get; set; }

        public string LaunchUrl { get; set; }
        
        public long TrackerId { get; set; }

        public CultureInfo RobloxLocale { get; set; }

        public CultureInfo GameLocale { get; set; }
    }

    public interface IStarlightLaunchParams
    {
        // NOTE TO SELF IF YOU ADD ANY CRAP HERE MAKE SURE TO ADD IT TO THE DERIVE FUNCTION
        
        public int FpsCap { get; set; }

        public bool Headless { get; set; }

        public bool Spoof { get; set; }

        public string Hash { get; set; }
    }

    public class LaunchParams : IRobloxLaunchParams, IStarlightLaunchParams
    {
        public int FpsCap { get; set; } = 0;
        
        public bool Headless { get; set; } = false;

        public bool Spoof { get; set; } = false;

        public string Hash { get; set; } = null;

        public string Token { get; set; } = string.Empty;
        
        public DateTimeOffset LaunchTime { get; set; }

        public string LaunchUrl { get; set; } = string.Empty;

        public long TrackerId { get; set; } = 0;

        public CultureInfo RobloxLocale { get; set; }

        public CultureInfo GameLocale { get; set; }

        public void Derive(IStarlightLaunchParams args)
        {
            FpsCap = args.FpsCap;
            Headless = args.Headless;
            Spoof = args.Spoof;
            Hash = args.Hash;
        }
    }
    
    public class Launcher
    {
        static Mutex RbxSingleton;

        public static void CommitSingleton() =>
            RbxSingleton = new(true, "ROBLOX_singletonMutex");

        public static void ReleaseSingleton() => // releases when program leaves too
            RbxSingleton.Dispose();

        public static bool Launch(LaunchParams info)
        {
            info.Hash ??= Bootstrapper.GetLatestHash().Result;
            
            if (info.Spoof)
            {
                info.TrackerId = Utility.SecureRandomInteger();
                info.LaunchUrl = Regex.Replace(info.LaunchUrl, @"browserTrackerId=\d+", $"browserTrackerId={info.TrackerId}");
                info.LaunchTime = (DateTimeOffset)DateTime.Now;
            }

            // Get client
            var clientQuery = Bootstrapper.GetClients().Where(x => x.Hash == info.Hash).ToArray();
            if (clientQuery.Length != 1) return false;
            var client = clientQuery[0];
            
            if (!OpenRoblox(client.Path, info, out PROCESS_INFORMATION procInfo))
                return false;
            ResumeThread(procInfo.hThread);
            
            RbxProc = Process.GetProcessById(procInfo.dwProcessId);
            Rbx = new(RbxProc);

            /* Runtime */

            if (info.FpsCap != 0) // todo: for headless disable rendering by hooking render job instead of limiting fps, will be more efficient
            {
                var sched = RBX.TaskScheduler.Singleton();
                sched.FramesPerSecond = info.FpsCap;
            }
            
            if (info.Headless)
            {
                // wait for window and hide it
                while (RbxProc.MainWindowHandle == IntPtr.Zero) Thread.Sleep(10);
                ShowWindow(RbxProc.MainWindowHandle, SW_HIDE);
            }

            return true;
        }
        
        internal static bool OpenRoblox(string robloxPath, LaunchParams info, out PROCESS_INFORMATION procInfo)
        {
            STARTUPINFO startInfo = new();
            return CreateProcess(
                Path.GetFullPath(robloxPath),
                $"--play -a https://www.roblox.com/Login/Negotiate.ashx -t {info.Token} -j {info.LaunchUrl} -b {info.TrackerId} " + // Roblox uses ASP.NET, interesting?
                $"--launchtime={info.LaunchTime.ToUnixTimeMilliseconds()} --rloc {info.RobloxLocale.Name} --gloc {info.GameLocale.Name}",
                0,
                0,
                false,
                ProcessCreationFlags.CREATE_SUSPENDED,
                0,
                null,
                ref startInfo,
                out procInfo
            );
        }
    }
}
