using System;
using CommandLine;
using Starlight.Bootstrap;
using Starlight.Except;

namespace Starlight.Cli.Verbs;

[Verb("install", HelpText = "Install a Roblox client.")]
public class Install : VerbBase
{
    [Option('h', "hash", Required = false, Default = null, HelpText = "The hash of the client to install.")]
    public string Hash { get; set; }

    protected override int Init()
    {
        if (string.IsNullOrEmpty(Hash))
            Hash = Bootstrapper.GetLatestHash();

        try
        {
            Bootstrapper.QueryClient(Hash);
            Console.WriteLine($"Roblox version-{Hash} is already installed.");
            return 1;
        }
        catch (ClientNotFoundException)
        {
            return 0;
        }
    }

    protected override int InternalInvoke()
    {
        Console.WriteLine($"Installing Roblox version-{Hash}...");
        try
        {
            Bootstrapper.Install(Hash);
            Console.WriteLine("Roblox has been installed successfully.");
            return 0;
        }
        catch (BadIntegrityException ex)
        {
            Console.WriteLine($"Failed to install Roblox: {ex.Message}");
            return 1;
        }
    }
}