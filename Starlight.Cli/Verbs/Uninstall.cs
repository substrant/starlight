using CommandLine;
using System;

using Starlight.Core;

namespace Starlight.Cli.Verbs
{
    [Verb("uninstall", HelpText = "Uninstall a Roblox client.")]
    public class Uninstall : VerbBase
    {
        [Option('h', "hash", Required = true, Default = null, HelpText = "The hash of the client to uninstall.")]
        public string Hash { get; set; }

        protected override int Init()
        {
            if (string.IsNullOrEmpty(Hash))
                Hash = Bootstrapper.GetLatestHash();

            var client = Bootstrapper.QueryClient(Hash);
            if (client is not null)
                return 0;
            
            Console.WriteLine($"Roblox version-{Hash} doesn't exist.");
            return 1;

        }

        protected override int InternalInvoke()
        {
            Bootstrapper.Uninstall(Hash);
            Console.WriteLine($"Roblox version-{Hash} has been uninstalled successfully.");
            return 0;
        }
    }
}
