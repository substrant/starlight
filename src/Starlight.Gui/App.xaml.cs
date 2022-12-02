using System;
using System.Linq;
using System.Windows;

using Starlight.Apis;
using Starlight.App;
using Starlight.Launch;
using Starlight.Plugins;

namespace Starlight.Gui;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    void Application_Startup(object sender, StartupEventArgs e) {
        if (AutoUpdate.IsUpdateAvailable())
            AutoUpdate.UpdateOnExit();

        PluginArbiter.LoadPlugins();

        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

        if (args.Length > 0) {
            var info = Scheme.Parse(args[0]);

            if (info is null) {
                MessageBox.Show("An invalid payload was specified");
                Shutdown(1);
                return;
            }

# if DEBUG
            var session = Session.Login(Environment.GetEnvironmentVariable("RBX_AUTH"));
            info.AuthStr = session.GetTicket();
# endif

            var launchWnd = new LaunchWindow(info);
            launchWnd.Show();
        }
        else {
            var mainWnd = new MainWindow();
            mainWnd.Show();
        }
    }
}