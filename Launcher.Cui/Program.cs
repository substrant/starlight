/* 
 *  Starlight - A custom Roblox launcher implementation.
 *  Author: RealNickk
*/

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;

using static Starlight.CLI;
using System.Collections.Generic;

namespace Starlight.Cui
{
    static class Program
    {
        static int Hook(HookOptions hookOptions)
        {
            string szAssemblyPath = hookOptions.Gui ?
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Starlight.Gui.exe") :
                Assembly.GetExecutingAssembly().Location;
            string szOptions = SerializeOptions(hookOptions, new List<string> { "Gui" }); // smh their code sucks and errors so i had to make my own serializer

            using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
            registryKey.SetValue(string.Empty, $"\"{szAssemblyPath}\" launch {szOptions}--payload %1", RegistryValueKind.String);

            Console.WriteLine("Starlight has hooked Roblox's launch scheme.");
            return 0;
        }
        
        static int Unhook()
        {
            var installations = Roblox.GetInstallations();
            if (installations.Count < 1)
            {
                Registry.CurrentUser.DeleteSubKey("Software\\Classes\\roblox-player");
                goto Unhooked;
            }
            string szBinary = Path.Combine(installations.First().Value, "RobloxPlayerLauncher.exe");

            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command"))
                registryKey.SetValue(string.Empty, $"\"{szBinary}\" %1", RegistryValueKind.String);

        Unhooked:
            Console.WriteLine("Starlight has unhooked its launcher from Roblox.");
            return 0;
        }
        
        static async Task<int> Launch(LaunchOptions launchOptions)
        {
            LaunchParams launchParams = Launcher.ParsePayload(launchOptions.Payload);

            // Check for Roblox
            Console.WriteLine("Checking Roblox...");
            string szGitHash = launchOptions.ForceVersion ? launchOptions.GitHash : await Roblox.GetLatestHash();
            var installations = Roblox.GetInstallations();
            if (!installations.TryGetValue(szGitHash, out string szInstallationPath))
            {
                if (launchOptions.Strict) goto RobloxNotInstalled;
                if (launchOptions.ForceVersion)
                {
                    if (!ConsoleExtensions.ShowDialog($"Roblox version-{szGitHash} does not exist. Do you want to install it?"))
                        goto RobloxNotInstalled;
                }
                goto UpdateRoblox;

            RobloxNotInstalled:
                Console.WriteLine("Roblox is not installed.");
                return 1;

            UpdateRoblox:
                Manifest manifest = await Bootstrapper.GetManifest(szGitHash);
                try
                {
                    Console.WriteLine("Updating Roblox...");
                    szInstallationPath = await Bootstrapper.InstallRoblox(manifest);
                }
                catch (BootstrapError ex)
                {
                    Console.WriteLine($"Update failed: \"{ex.Message}\"");
                    return 1;
                }
            }

            // Evade tracking if not explicitly told to do otherwise
            if (!launchOptions.NoSpoof)
                Launcher.EvadeTracking(launchParams);

            // Launch Roblox
            Console.WriteLine("Opening Roblox...");
            string szBinPath = Path.Combine(szInstallationPath, "RobloxPlayerBeta.exe");
            if (!Launcher.OpenRoblox(szBinPath, launchParams, out Native.PROCESS_INFORMATION robloxInfo))
            {
                Console.WriteLine("Failed to open Roblox.");
                return 1;
            }

            // Switch to use of the .NET process class
            Process roblox = Process.GetProcessById(robloxInfo.dwProcessId);

            // middleware

            // Resume Roblox's thread and wait for window
            Native.ResumeThread(robloxInfo.hThread);
            while (roblox.MainWindowHandle == IntPtr.Zero)
                await Task.Delay(1500);

            if (roblox.HasExited)
            {
                Console.WriteLine("Roblox opened but exited while joining.");
                return 1;
            }

            if (launchOptions.Headless) // I can't put a hidden window flag in the start info
                Native.ShowWindow(roblox.MainWindowHandle, Native.SW_HIDE);

            return 0;
        }

        static async Task<int> Install(InstallOptions installOptions)
        {
            string szGitHash = installOptions.GitHash ?? await Roblox.GetLatestHash();
            var installations = Roblox.GetInstallations();
            if (installations.ContainsKey(szGitHash))
            {
                Console.WriteLine($"Roblox version-{szGitHash} is already installed.");
                return 0;
            }

            Manifest manifest = await Bootstrapper.GetManifest(szGitHash);
            try
            {
                Console.WriteLine("Installing Roblox...");
                await Bootstrapper.InstallRoblox(manifest);
                Console.WriteLine("Installation finished.");
                return 0;
            }
            catch (BootstrapError ex)
            {
                Console.WriteLine($"Installation failed: \"{ex.Message}\"");
                return 1;
            }
        }

#if DEBUG
        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            if (ex as AggregateException != null)
            {
                AggregateException aex = (AggregateException)args.ExceptionObject;
                foreach (Exception inner in aex.InnerExceptions)
                    Console.WriteLine("An unhandled exception was caught: {0}", inner.ToString());
            }
            else Console.WriteLine("An unhandled exception was caught: {0}", ex.Message);
            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();
        }
#endif

        static int Main(string[] args)
        {
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
#endif
            int result = Parser.Default.ParseArguments<HookOptions, UnhookOptions, LaunchOptions, InstallOptions>(args)
                .MapResult(
                    (HookOptions x) => Hook(x),
                    (UnhookOptions x) => Unhook(),
                    (LaunchOptions x) => Launch(x).Result,
                    (InstallOptions x) => Install(x).Result,
                    x => 1);
#if DEBUG
            Console.ReadLine();
#endif
            return result;
        }
    }
}