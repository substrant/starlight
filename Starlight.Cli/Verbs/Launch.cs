using CommandLine;
using Starlight.Core;
using Starlight.Rbx;
using Starlight.RbxApp;
using System;
using Starlight.Rbx.JoinGame;

namespace Starlight.Cli.Verbs
{
    [Verb("launch", HelpText = "Launch Roblox from the command line.")]
    public class Launch : VerbBase, IStarlightLaunchParams
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

        [Option('t', "token", Required = true, HelpText = "Roblox authentication token. Use a token here, NOT a ticket.")]
        public string Token { get; set; }

        [Option('p', "placeid", Required = true, Default = null, HelpText = "The place ID to join.")]
        public long PlaceId { get; set; }

        [Option('j', "jobid", Required = false, Default = null, HelpText = "The server instance ID to join.")]
        public Guid? JobId { get; set; }

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
            var session = Session.Login(Token); // TODO: captcha and other bullcrap for user:pass maybe use chromium embedded framework and make user enter it
            if (session is null)
            {
                Console.WriteLine("Failed to log in: an invalid authentication token was specified.");
                return 1;
            }
            
            Console.WriteLine("Logged in as {0} ({1})", session.Username, session.UserId);
            var info = new LaunchParams { Ticket = session.GetTicket() };

            if (JobId.HasValue)
            {
                info.Request = new JoinRequest
                {
                    PlaceId = PlaceId,
                    JobId = JobId.Value,
                    ReqType = JoinType.Specific
                };
            }
            else
            {
                info.Request = new JoinRequest
                {
                    PlaceId = PlaceId,
                    ReqType = JoinType.Auto
                };
            }
            
            Console.WriteLine("Launching Roblox...");
            try
            {
                Launcher.Launch(info, this);
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
