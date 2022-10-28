using System;
using System.IO;
using System.Reflection;
using CommandLine;
using Starlight.SchemeLaunch;

namespace Starlight.Cli.Verbs;

[Verb("hook", HelpText = "Hook Roblox's scheme.")]
public class Hook : VerbBase
{
    protected override int Init()
    {
        return 0;
    }

    protected override int InternalInvoke()
    {
        var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (binDir is null)
        {
            Console.WriteLine("Failed to get current assembly directory.");
            return 1;
        }

        var launcherBin = Path.Combine(binDir, "Starlight.Launcher.exe");
        if (Scheme.Hook(launcherBin))
        {
            Console.WriteLine("Hooked scheme. You can now launch with Starlight from the browser.");
            return 0;
        }

        Console.WriteLine("Failed to hook scheme.");
        return 1;
    }
}