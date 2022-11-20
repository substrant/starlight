using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.App;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.Misc.Profiling;

namespace Starlight.Gui;

public partial class LaunchWindow : Window
{
    public readonly LaunchParams LaunchInfo;
    public readonly ProgressTracker Tracker = new();

    public LaunchWindow(LaunchParams launchInfo)
    {
        InitializeComponent();
        InitializeUi();

        LaunchInfo = launchInfo;
    }

    public LaunchWindow(Session session, JoinRequest req)
    {
        InitializeComponent();
        InitializeUi();

        LaunchInfo = new LaunchParams
        {
            AuthStr = session.GetTicket(),
            AuthType = AuthType.Ticket,
            Request = req,
            RobloxLocale = CultureInfo.CurrentUICulture,
            GameLocale = CultureInfo.CurrentCulture
        };
    }

    private void InitializeUi()
    {
        Tracker.ProgressUpdated += sender =>
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(sender.Annotation))
                    Label.Text = sender.Annotation;
                ProgressBar.Value = sender.PercentComplete;
            });
    }

    // Ensure it's the latest version and installs new version if not.
    private async Task<Client> GetClient()
    {
        var client = await AppShared.GetSharedClientAsync();
        if (!client.Exists)
        {
            Tracker.Start(2, "Updating Roblox");
            await Bootstrapper.InstallAsync(client, Tracker.SubStep(), new InstallConfig
            {
                RegisterClass = false
            });
        }
        else
        {
            Tracker.Start(1, "Launching Roblox");
        }

        return client;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var client = await GetClient();
            await Launcher.LaunchAsync(client, LaunchInfo);
        }
        catch (IOException)
        {
            MessageBox.Show("Installation failed due to a filesystem error.", "Starlight", MessageBoxButton.OK,
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