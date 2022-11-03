using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Starlight.Bootstrap;
using Starlight.Except;
using Starlight.Misc;

namespace Starlight.Test;

[TestFixture]
public class BootstrapperTests
{
    [OneTimeSetUp]
    public void InitLogger()
    {
        Logger.Init(true);
    }

    [Test]
    public void GetLatestHash()
    {
        try
        {
            Bootstrapper.GetLatestVersionHash();
        }
        catch (BadIntegrityException)
        {
            Assert.Inconclusive("Couldn't fetch latest hash.");
        }
    }

    [Test]
    public void FetchManifest()
    {
        var manifest = Bootstrapper.GetManifest(Bootstrapper.GetLatestVersionHash());
        if (manifest is null)
            Assert.Inconclusive("Couldn't fetch manifest.");
    }

    static void PreNativeInstall()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var installPath = Path.Combine(localAppData, "Roblox");

        // Uninstall Roblox if it's installed.
        if (Directory.Exists(installPath))
            Directory.Delete(installPath, true);
    }

    [Test]
    public void NativeInstall()
    {
        Env.AssertCi();

        PreNativeInstall();
        try
        {
            var client = Bootstrapper.NativeInstall();
            Assert.IsFalse(client is null, "Client installation failed.");
        }
        catch (BadIntegrityException ex)
        {
            Assert.Fail("Client installation failed due to integrity check fail.", ex);
        }

        Thread.Sleep(1000); // idk bruh it works lol
    }

    [Test]
    public void Query()
    {
        Env.AssertCi();

        if (Bootstrapper.GetClients().Count < 1)
            Bootstrapper.Install();

        var clients = Bootstrapper.GetClients();
        if (clients.Count < 1)
            Assert.Fail("Failed to find any clients.");

        Bootstrapper.QueryClient(clients[0].Hash);
    }

    [Test]
    public void Uninstall()
    {
        Env.AssertCi();

        if (Bootstrapper.GetClients().Count < 1)
            Bootstrapper.Install();

        var clients = Bootstrapper.GetClients();
        Bootstrapper.Uninstall(clients[0]);
    }

    [Test]
    public void Install()
    {
        Env.AssertCi();

        var client = Bootstrapper.Install();
        if (client is null)
            Assert.Fail("Client installation failed.");
    }
}