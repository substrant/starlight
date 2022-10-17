using NUnit.Framework;
using Starlight.Core;
using Starlight.Misc;
using System;
using System.IO;
using System.Threading;

namespace Starlight.Test
{
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
                Bootstrapper.GetLatestHash();
            }
            catch (BootstrapException)
            {
                Assert.Inconclusive("Couldn't fetch latest hash.");
            }
        }

        [Test]
        public void FetchManifest()
        {
            var manifest = Bootstrapper.GetManifest(Bootstrapper.GetLatestHash());
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
            catch (BootstrapException ex)
            {
                Assert.Fail("Client installation failed: " + ex.Message, ex);
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
            
            var client = Bootstrapper.QueryClient(clients[0].Hash);
            Assert.IsFalse(client is null, "Client query failed.");
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
}
