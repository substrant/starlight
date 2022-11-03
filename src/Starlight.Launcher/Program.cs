using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using Newtonsoft.Json;
using Starlight.Apis;
using Starlight.Bootstrap;
using Starlight.Except;
using Starlight.Misc;
using Starlight.Plugins;
using Starlight.SchemeLaunch;

namespace Starlight.Launcher;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            MessageBox.Show(
                "You're not supposed to run this directly! Do \"Starlight.Cli.exe hook\" in the command prompt to hook Roblox's scheme.",
                "Starlight", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }

        // Init
        PluginArbiter.LoadPlugins();

#if DEBUG
        var session = Session.Login(Environment.GetEnvironmentVariable("RBX_AUTH", EnvironmentVariableTarget.User), AuthType.Token);
        var authTicket = session.GetTicket();
#endif

        var client = Bootstrapper.GetLatestClient(ClientScope.Local);

        if (!client.Exists)
        {
            try
            {
                Bootstrapper.Install(client, new InstallConfig
                {
                    RegisterClass = false,
                    RegisterClient = true,
                    CreateDesktopShortcut = false,
                    CreateStartMenuShortcut = false
                });
            }
            catch (BadIntegrityException)
            {
                MessageBox.Show("fail bad integrity :(");
                return 1;
            }
        }
        
        try
        {
            var info = Scheme.Parse(args[0]);
#if DEBUG
            info.Ticket = authTicket;
#endif
            var inst = Launch.Launcher.Launch(info, client);
            return inst is null ? 1 : 0;
        }
        catch (SchemeParseException)
        {
        }
        catch (ClientNotFoundException)
        {
        }
        catch (PostLaunchException)
        {
            // I wouldn't say fatal here because Roblox did open.
        }

        MessageBox.Show("fail postlaunch :(");

        return 1;
    }
}