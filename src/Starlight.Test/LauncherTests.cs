using NUnit.Framework;
using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Launch;

namespace Starlight.Test;

[TestFixture]
[Order(2)]
public class LauncherTests
{
    [Test]
    public void Launch()
    {
        Env.AssertCi();

        if (Bootstrapper.GetClients().Count < 1)
            Bootstrapper.Install();

        if (Env.AuthToken is null)
            Assert.Inconclusive("No Roblox token was provided. Please set the AUTH_TOKEN environment variable.");

        var session = Session.Login(Env.AuthToken);
        var ticket = session.GetTicket();

        var info = new LaunchParams
        {
            // Roblox join stuff.
            Ticket = ticket,
            Request = new JoinRequest
            {
                PlaceId = 4483381587, // https://www.roblox.com/games/4483381587/a-literal-baseplate
                ReqType = JoinType.Auto
            },

            // Should make everything run
            FpsCap = 160,
            Headless = true,
            Spoof = true,
            Resolution = "800x600"
        };

        // Launch Roblox
        var inst = Launcher.Launch(info);
        if (inst is null)
            Assert.Fail("Failed to launch Roblox.");

        inst.Proc.Kill();
    }
}