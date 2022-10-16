using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Starlight.Test
{
    [TestClass]
    public class Bootstrapper
    {
        string _latestHash;

        [TestMethod]
        public void RunTest()
        {
            LatestHash();
            GetManifest();
            NativeInstall();
            GetClientAndUninstall();
            Install();
            Query();
        }
        
        void LatestHash()
        {
            _latestHash = Core.Bootstrapper.GetLatestHash();
        }

        void GetManifest()
        {
            var manifest = Core.Bootstrapper.GetManifest(_latestHash);
            if (manifest is null)
                Assert.Fail("Manifest fetch failed");
        }

        void NativeInstall()
        {
            var client = Core.Bootstrapper.NativeInstall();
            if (client is null)
                Assert.Fail("Native install failed");
        }
        
        void GetClientAndUninstall()
        {
            Core.Bootstrapper.Uninstall(Core.Bootstrapper.GetClients()[0]);
        }
        
        void Install()
        {
            var client = Core.Bootstrapper.Install(_latestHash);
            if (client is null)
                Assert.Fail("Install failed");
        }
        
        void Query()
        {
            var client = Core.Bootstrapper.QueryClient(_latestHash);
            if (client is null)
                Assert.Fail("Client query failed");
        }
    }
}
