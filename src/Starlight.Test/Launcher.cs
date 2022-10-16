using Microsoft.VisualStudio.TestTools.UnitTesting;
using Starlight.Core;
using Starlight.Rbx;
using Starlight.Rbx.JoinGame;
using System;

namespace Starlight.Test
{
    [TestClass]
    public class Launcher
    {
        [TestMethod]
        public void TestLauncher()
        {
            // Skip if it's CI
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
            {
                // Invalid ticket = no launch; Github Actions can't do it since Roblox invalidates the token every time a new IP address uses it
                Assert.Inconclusive("Skipping test because CI doesn't support launching. ENSURE THAT LAUNCHING WORKS BEFORE MERGING A PR!");
            }
            
            Setup.Init(); // Setup a clean environment

            // Install Roblox if not installed
            if (Core.Bootstrapper.GetClients().Count < 1)
                Core.Bootstrapper.Install();

            // Get ticket
            var token = Environment.GetEnvironmentVariable("AUTH_TOKEN");
            Assert.IsFalse(token is null, "No Roblox token was provided. Please set the AUTH_TOKEN environment variable.");

            var session = Session.Login(token);
            var ticket = session.GetTicket();

            var info = new LaunchParams
            {
                // Roblox join stuff.
                Ticket = ticket,
                Request = new JoinRequest
                {
                    PlaceId = 4483381587,
                    ReqType = JoinType.Auto
                },

                // Should make everything run
                FpsCap = 160,
                Headless = true,
                Spoof = true,
                Resolution = "800x600"
            };

            // Launch Roblox
            var inst = Core.Launcher.Launch(info);
            if (inst is null)
                Assert.Fail("Launch failed");

            inst.Proc.Kill();
        }
    }
}
