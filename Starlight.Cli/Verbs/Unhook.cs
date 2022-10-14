using CommandLine;
using Starlight.Core;
using System;

namespace Starlight.Cli.Verbs
{
    [Verb("unhook", HelpText = "Unhook from Roblox's scheme.")]
    public class Unhook : VerbBase
    {
        protected override int Init()
        {
            if (Bootstrapper.GetClients().Count >= 1)
                return 0;
            
            Console.WriteLine("No Roblox clients are installed.");
            return 1;

        }

        protected override int InternalInvoke()
        {
            if (Scheme.Unhook())
            {
                Console.WriteLine("Unhooked scheme. Launching Roblox from the browser should no longer open Starlight.");
                return 0;
            }

            Console.WriteLine("Failed to unhook scheme.");
            return 1;
        }
    }
}
