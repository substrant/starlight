using Starlight.Misc;
using System;
using System.IO;

namespace Starlight.Test
{
    public class Setup
    {
        public static void Init()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var installPath = Path.Combine(localAppData, "Roblox");

            if (Directory.Exists(installPath))
                Directory.Delete(installPath, true);

            Logger.Init(true);
        }
    }
}