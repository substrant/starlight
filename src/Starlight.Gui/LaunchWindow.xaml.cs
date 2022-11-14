using System;
using System.Collections.Generic;
using System.IO;
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
                Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrWhiteSpace(sender.Annotation))
                        Label.Text = sender.Annotation;
                    ProgressBar.Value = sender.PercentComplete;
                });
        }

        async Task<Client> EnsureLatest()
        {
            var client = await Bootstrapper.GetLatestClientAsync(ClientScope.Local);

            if (!client.Exists)
            {
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
            try
            {
                var client = await EnsureLatest();
                await Launcher.LaunchAsync(client, LaunchInfo);        
            }
            catch (IOException)
            {
                MessageBox.Show("Installation failed due to a filesystem error.", "Starlight", MessageBoxButton.OK, MessageBoxImage.Error);
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
