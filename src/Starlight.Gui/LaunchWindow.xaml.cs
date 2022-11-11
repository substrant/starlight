using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.Misc.Profiling;

namespace Starlight.Gui
{
    public partial class LaunchWindow : Window
    {
        public readonly LaunchParams LaunchInfo;
        public readonly ProgressTracker Tracker = new();

        public LaunchWindow(LaunchParams launchInfo)
        {
            InitializeComponent();
            LaunchInfo = launchInfo;
            Tracker.ProgressUpdated += sender =>
                Dispatcher.Invoke(() => ProgressBar.Value = sender.PercentComplete);
        }

        async Task<Client> EnsureLatest()
        {
            Label.Text = "fetcshing bone";
            var client = await Bootstrapper.GetLatestClientAsync(ClientScope.Local);
            Label.Text = "ensuring";

            if (!client.Exists)
            {
                Label.Text = "updiatign";
                Tracker.Start(2, "Updating Roblox");
                await Bootstrapper.InstallAsync(client, Tracker.SubStep(), new InstallConfig
                {
                    CreateDesktopShortcut = false,
                    CreateStartMenuShortcut = false,
                    RegisterClass = false
                });
            }
            else
            {
                Tracker.Start(1, "Launching Roblox");
            }

            return client;
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label.Text = "init";
            try
            {
                var client = await EnsureLatest();
                Label.Text = "quack";
                Launcher.Launch(LaunchInfo, client);        
            }
            catch (BadIntegrityException)
            {
                MessageBox.Show("Installation failed: Corrupt download", "Starlight", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (PrematureCloseException)
            {
                MessageBox.Show("Roblox prematurely closed. Did you provide a valid authentication ticket?");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex}");
            }
            finally
            {
                Close();
            }
        }
    }
}
