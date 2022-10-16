using Microsoft.VisualStudio.TestTools.UnitTesting;
using Starlight.Misc;
using System;
using System.IO;

namespace Starlight.Test
{
    public class Setup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var installPath = Path.Combine(localAppData, "Roblox");

            if (Directory.Exists(installPath))
                Directory.Delete(installPath);

            Logger.Init(true);
        }
    }
}
