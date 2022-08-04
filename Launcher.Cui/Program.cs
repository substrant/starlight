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
using System.Text;
using System.Threading.Tasks;
using CommandLine;

using static Starlight.Cui.CommandLineInterface;

namespace Starlight.Cui
{
    public class CommandLineInterface
    {
        [Verb("launch", HelpText = "Launch using a Roblox launcher URL.")]
        public class LaunchOptions
        {
            [Option("payload", Required = false, HelpText = "The Roblox launch payload.")]
            public string Payload { get; set; }

            [Option("headless", Required = false, Default = false, HelpText = "Launch without opening the window.")]
            public bool Headless { get; set; }

            [Option("no-spoof", Required = false, Default = false, HelpText = " Roblox's launch schema to launch this program.")]
            public bool NoSpoof { get; set; }

            [Option('h', "git-hash", Required = false, Default = "", HelpText = "Force launcher to run a specific verison of Roblox instead of the latest version.")]
            public string GitHash { get; set; }
            public bool ForceVersion { get => !GitHash.IsEmpty(); }

            [Option('s', "strict", Required = false, Default = false, HelpText = "If you launch with strict mode, the launcher will not update Roblox if the binary is obsolete.")]
            public bool Strict { get; set; }
        }

        [Verb("hook", HelpText = "Hook Roblox's schema to launch using this program.")]
        public class HookOptions : LaunchOptions // Mirror the options
        {
            [Obsolete("This property is a placeholder and cannot be used", true)]
            [Option("payload", Required = false, Hidden = true)]
            public new string Payload { get; }
        }

        [Verb("unhook", HelpText = "Remove the hook on Roblox's schema.")]
        public class UnhookOptions
        {

        }

        [Verb("install", HelpText = "Install Roblox.")]
        public class InstallOptions
        {
            [Option('h', "git-hash", Required = false, HelpText = "Install a specific version of Roblox.")]
            public string GitHash { get; set; }
        }
    }

    static class Program
    {
        // was unaware there was an api for this
        static string SerializeOption(OptionAttribute opt)
        {
            if (opt.ShortName != string.Empty)
                return $"-{opt.ShortName}";
            else return $"--{opt.LongName}";
        }

        static string SerializeOptions(object options) // Reflection is hot
        {
            StringBuilder szOptions = new();
            foreach (PropertyInfo prop in typeof(HookOptions).GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    foreach (OptionAttribute attr in prop.GetCustomAttributes(true).Where(x => x as OptionAttribute != null))
                    {
                        var value = prop.GetValue(options);
                        if (value != null)
                        {
                            //Console.WriteLine("{0} -> {1} ({2}", prop.Name, value, prop.PropertyType.Name);
                            if (prop.PropertyType == typeof(string))
                            {
                                if (!((string)value).IsEmpty())
                                    szOptions.Append(SerializeOption(attr) + ' ' + value.ToString() + ' ');
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                if ((bool)value)
                                    szOptions.Append(SerializeOption(attr) + ' ');
                            }
                            else throw new Exception("Invalid command value type.");
                        }
                    }
                }
            }
            return szOptions.ToString();
        }

        static int Hook(HookOptions hookOptions)
        {
            string szAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string szOptions = SerializeOptions(hookOptions); // smh their code sucks and errors so i had to make my own serializer

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
                Console.WriteLine("No suitable Roblox installation was found.");
                return 1;
            }
            string szBinary = Path.Combine(installations.First().Value, "RobloxPlayerLauncher.exe");

            using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\roblox-player\\shell\\open\\command");
            registryKey.SetValue(string.Empty, $"\"{szBinary}\" %1", RegistryValueKind.String);

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
        
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            
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