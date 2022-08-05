/* 
 *  Starlight GUI
 *  Author: pepsi, RealNickk
 *  Note: doing code review on pepsi's code is hell - nick
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

using static Starlight.CLI;

namespace Starlight.Gui
{
    public partial class MainWindow : Window
    {
        public static void DoFade(UIElement element, bool bInOut, double dSet, TimeSpan duration) // casting DependencyObject to UIElement bruh what LOL
        {
            DoubleAnimation fadeAnimation = bInOut ? 
                new(0, dSet, duration) { BeginTime = TimeSpan.Zero } :
                new(element.Opacity, 0, duration) { BeginTime = TimeSpan.Zero };
            element.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        async void WriteOut(string szStr, params string[] fmt)
        {
            await Dispatcher.BeginInvoke(() => Status.Text = string.Format(szStr, fmt));
        }

        public int ExitCode;
        static async Task<int> Launch(LaunchOptions launchOptions)
        {
            LaunchParams launchParams = Launcher.ParsePayload(launchOptions.Payload);

            // Check for Roblox
            Output.WriteLineOut("Checking Roblox...");
            string szGitHash = launchOptions.ForceVersion ? launchOptions.GitHash : await Roblox.GetLatestHash();
            var installations = Roblox.GetInstallations();
            if (!installations.TryGetValue(szGitHash, out string szInstallationPath))
            {
                if (launchOptions.Strict) goto RobloxNotInstalled;
                goto UpdateRoblox;

            RobloxNotInstalled:
                Output.WriteLineOut("Roblox is not installed.");
                await Task.Delay(2500);
                return 1;

            UpdateRoblox:
                Manifest manifest = await Bootstrapper.GetManifest(szGitHash);
                try
                {
                    Output.WriteLineOut("Updating Roblox...");
                    szInstallationPath = await Bootstrapper.InstallRoblox(manifest);
                }
                catch (BootstrapError ex)
                {
                    Output.WriteLineOut($"Update failed: \"{ex.Message}\"");
                    await Task.Delay(2500);
                    return 1;
                }
            }

            // Evade tracking if not explicitly told to do otherwise
            if (!launchOptions.NoSpoof)
                Launcher.EvadeTracking(launchParams);

            // Launch Roblox
            Output.WriteLineOut("Opening Roblox...");
            string szBinPath = Path.Combine(szInstallationPath, "RobloxPlayerBeta.exe");
            if (!Launcher.OpenRoblox(szBinPath, launchParams, out Native.PROCESS_INFORMATION robloxInfo))
            {
                Output.WriteLineOut("Failed to open Roblox.");
                await Task.Delay(2500);
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
                Output.WriteLineOut("Roblox opened but exited while joining.");
                await Task.Delay(2500);
                return 1;
            }

            if (launchOptions.Headless) // I can't put a hidden window flag in the start info
                Native.ShowWindow(roblox.MainWindowHandle, Native.SW_HIDE);

            Output.WriteLineOut("Done!");
            await Task.Delay(500);
            return 0;
        }

        LaunchOptions options;
        public MainWindow(LaunchOptions launchOptions)
        {
            options = launchOptions;
            Output.SetOutput(WriteOut);
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoFade(Background, true, 1, TimeSpan.FromMilliseconds(200));
            await Task.Delay(300);
            DoFade(bar, true, 1, TimeSpan.FromMilliseconds(200));
            await Task.Delay(300);
            DoFade(Status, true, 1, TimeSpan.FromMilliseconds(200));
            await Task.Delay(100);
            
            int result = await Launch(options);
            ExitCode = result;
            Close();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DoFade(Background, false, 0, TimeSpan.FromMilliseconds(200));
            await Task.Delay(300);
            DoFade(bar, false, 0, TimeSpan.FromMilliseconds(200));
            await Task.Delay(300);
            DoFade(Status, false, 0, TimeSpan.FromMilliseconds(200));
        }
    }
}
