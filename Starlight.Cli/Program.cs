using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CommandLine;
using Starlight.Cli.Commands;
using static Starlight.Cli.Native;

namespace Starlight.Cli
{
    internal class Program
    {
        static uint ConsoleWndHandle;
        
        static void HandleException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Exception thrown: {0}", ex.Message);
            Console.Write("Press enter to continue...");
            Console.ReadLine();
        }

        static int Hook(HookOptions hookOptions)
        {
            var launcherBin = Assembly.GetExecutingAssembly().Location;
            if (Scheme.Hook(launcherBin, "launch " + hookOptions.Serialize() + " -p ")) // --payload %1
                return 0;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Failed to hook scheme.");
            return 1;
        }

        static int Unhook()
        {
            if (Scheme.Unhook())
                return 0;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Failed to unhook scheme.");
            return 1;
        }
        
        static async Task<int> Launch(LaunchOptions launchOptions)
        {
            if (string.IsNullOrEmpty(launchOptions.Hash))
                launchOptions.Hash = await Bootstrapper.GetLatestHash();

            var clientQuery = Bootstrapper.GetClients().Where(x => x.Hash == launchOptions.Hash).ToArray();
            if (clientQuery.Length != 1)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Installing Roblox...");
                
                try
                {
                    await Bootstrapper.Install(launchOptions.Hash);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Fail.");
                    HandleException(ex);

                    return 1;
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Done.");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Launching...");
            
            // Interfaces are awesome
            if (!Scheme.Launch(launchOptions.Payload, launchOptions))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fail.");
                
                await Task.Delay(1000);
                return 1;
            }
            
            return 0;
        }

        static async Task<int> Install(InstallOptions installOptions)
        {
            if (string.IsNullOrEmpty(installOptions.Hash))
                installOptions.Hash = await Bootstrapper.GetLatestHash();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Installing Roblox...");
            
            try
            {
                await Bootstrapper.Install(installOptions.Hash);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fail.");
                HandleException(ex);

                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Done.");

            return 0;
        }

        static async Task<int> Uninstall(UninstallOptions options)
        {
            Bootstrapper.Uninstall(options.Hash ?? await Bootstrapper.GetLatestHash());
            return 1;
        }
        
        static void CommitThread(CancellationToken cancelToken) // 15hz check
        {
            while (!cancelToken.IsCancellationRequested)
            {
                // I use findwindow because scanning process list kills my cpu
                
                while (FindWindow(null, "Roblox") == 0) // If Roblox isn't open yet, wait for open.
                {
                    cancelToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1 / 15));
                    if (cancelToken.IsCancellationRequested)
                        break;
                }
                
                Launcher.CommitSingleton();

                while (FindWindow(null, "Roblox") != 0) // Wait for all instances to close.
                {
                    cancelToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1 / 15));
                    if (cancelToken.IsCancellationRequested)
                        goto ReleaseCommit;
                }
                
            ReleaseCommit:
                Launcher.ReleaseSingleton();
            }
        }

        static async Task<int> Unlock(UnlockOptions options)
        {
            // Wait for Roblox to close
            if (FindWindow(null, "Roblox") != 0)
            {
                Console.WriteLine("Waiting for all Roblox processes to close...");
                while (FindWindow(null, "Roblox") != 0)
                    await Task.Delay(TimeSpan.FromSeconds(1 / 15));
            }

            if (options.Relock)
            {
                Launcher.CommitSingleton();

                while (FindWindow(null, "Roblox") != 0)
                    await Task.Delay(TimeSpan.FromSeconds(1 / 15));

                Launcher.ReleaseSingleton();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Press enter to release singleton...");

                CancellationTokenSource commitTask = new();
                new Thread(new ThreadStart(() => CommitThread(commitTask.Token))).Start();
                
                Console.ReadLine();
                commitTask.Cancel(); // Stop the commit thread
            }
            
            return 0;
        }

        static int Main(string[] args)
        {
            Console.Title = "Starlight CLI";
            ConsoleWndHandle = GetConsoleWindow();

#if DEBUG
            Console.Write("Waiting for debugger..."); // Debug->Attach to process..."Shift + Alt + P" to reattach
            while (!IsDebuggerPresent())
                Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Attached!");
            Console.ForegroundColor = ConsoleColor.Gray;
#endif

            // a bit messy
            var code = Parser.Default.ParseArguments<HookOptions, UnhookOptions, LaunchOptions, InstallOptions, UnlockOptions>(args)
                .MapResult(
                    (HookOptions x) => Hook(x),
                    (UnhookOptions x) => Unhook(),
                    (LaunchOptions x) => Launch(x).Result,
                    (InstallOptions x) => Install(x).Result,
                    (UninstallOptions x) => Uninstall(x).Result,
                    (UnlockOptions x) => Unlock(x).Result,
                    x => 1);
            
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n------ EXIT ------\nExit code: 0x{0:X}", code);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadLine();
#endif

            return code;
        }
    }
}
