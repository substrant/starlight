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
        public void RunTest()
        {
            // Install Roblox if not installed
            if (Core.Bootstrapper.GetClients().Count < 1)
                Core.Bootstrapper.Install();
            
            // Get ticket
            string token, ticket;
            if ((token = Environment.GetEnvironmentVariable("STARLIGHT_ROBLOSECURITY", EnvironmentVariableTarget.Machine)) is null) // idk if theres a better way to do this
                if ((token = Environment.GetEnvironmentVariable("STARLIGHT_ROBLOSECURITY", EnvironmentVariableTarget.User)) is null)
                    token = Environment.GetEnvironmentVariable("STARLIGHT_ROBLOSECURITY", EnvironmentVariableTarget.Process);

            if (token is not null)
            {
                var session = Session.Login(token);
                ticket = session.GetTicket();
            }
            else
                ticket = "notprovided";

            var info = new LaunchParams
            {
                // Roblox join stuff
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
