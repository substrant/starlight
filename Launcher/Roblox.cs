/* 
 *  Roblox.cs
 *  Author: RealNickk
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using static Starlight.Shared;

namespace Starlight
{
    public class Roblox
    {
		public static string GetInstallationPath()
        {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox\\Versions");
        }

        public static async Task<string> GetLatestHash()
        {
            string szVersion = await Web.DownloadStringTaskAsync($"http://{Endpoints.Setup}/version.txt");
            return szVersion.Split("version-")[1];
        }

        public static Dictionary<string, string> GetInstallations() // { GitHash, InstallationPath }
        {
			string szPath = GetInstallationPath();
			if (!Directory.Exists(szPath))
				throw new Exception("Roblox is not installed.");

			Dictionary<string, string> insts = new();
			foreach (string item in Directory.EnumerateDirectories(szPath))
			{
				if (!File.Exists(Path.Combine(item, "RobloxPlayerBeta.exe"))) continue;
				string szFileName = Path.GetFileName(item);
				insts.Add(szFileName.Split("version-")[1], item);
			}

			return insts;
		}
    }
}
