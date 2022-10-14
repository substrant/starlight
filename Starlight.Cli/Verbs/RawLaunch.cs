using CommandLine;
using Starlight.Core;
using Starlight.RbxApp;
using System;

namespace Starlight.Cli.Verbs
{
    [Verb("rawlaunch", HelpText = "Launch Roblox using a roblox-player scheme payload.")]
    public class RawLaunch : VerbBase, IStarlightLaunchParams
    {
        [Option('s', "spoof", Required = false, Default = false, HelpText = "Spoof Roblox's tracking.")]
        public bool Spoof { get; set; }

        [Option('h', "hash", Required = false, Default = null, HelpText = "Launch a specific hash of Roblox.")]
        public string Hash { get; set; }

        [Option("headless", Required = false, Default = false, HelpText = "Launch Roblox in the background.")]
        public bool Headless { get; set; }

        [Option('r', "res", Required = false, Default = null, HelpText = "Set the intiial resolution of Roblox. Example: \"-r 1920x1080\"")]
        public string Resolution { get; set; }

        [Option("fps-cap", Required = false, Default = 0, HelpText = "Limits the FPS of Roblox.")]
        public int FpsCap { get; set; }

        [Option('p', "payload", Required = true, HelpText = "The roblox-player scheme payload.")]
        public string Payload { get; set; }

        protected override int Init()
        {
            Hash = Bootstrapper.GetLatestHash();

            var client = Bootstrapper.QueryClient(Hash);
            if (client is not null)
                return 0;
            
            Console.WriteLine($"Installing Roblox version-{Hash}...");
            try
            {
                Bootstrapper.Install(Hash);
            }
            catch (BootstrapException ex)
            {
                Console.WriteLine($"Failed to install Roblox: {ex.Message}");
                return 1;
            }

            return 0;
        }

        protected override int InternalInvoke()
        {
            Console.WriteLine("Launching Roblox...");
            try
            {
                Scheme.Launch(Payload, this);
            }
            catch (LaunchException ex)
            {
                Console.WriteLine($"Failed to launch Roblox: {ex.Message}");
                return 1;
            }
            catch (AppModException ex)
            {
                Console.WriteLine($"Launch succeeded, but failed to post-launch: {ex.Message}");
                return 1;
            }
            
            return 0;
        }
    }
}
