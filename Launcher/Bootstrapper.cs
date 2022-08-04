/* 
 *  Boostrapper.cs
 *  Author: RealNickk
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using static Starlight.Shared;

namespace Starlight
{
    internal static class Endpoints
    {
        internal const string Setup = "setup.rbxcdn.com";
    }

    internal class DownloadableFile
    {
        readonly Manifest Parent;
        public readonly string Name;
        public readonly string Checksum;
        public readonly long Size;
        public readonly long TrueSize;

        public DownloadableFile(Manifest parent, string name, string checksum, long size, long trueSize)
        {
            Parent = parent;
            Name = name;
            Checksum = checksum;
            TrueSize = trueSize;
            Size = size;
        }

        public async Task<string> Download(string szDirectory)
        {
            string szOutPath = Path.Combine(szDirectory, Name);
            await Web.DownloadFileTaskAsync($"http://{Endpoints.Setup}/version-{Parent.GitHash}-{Name}", szOutPath);

            if (!Check(szDirectory))
                throw new BootstrapError("Download failed: Hash/size mismatch.");

            return szOutPath;
        }

        public bool Check(string szDirectory)
        {
            string szOutPath = Path.Combine(szDirectory, Name);
            if (!File.Exists(szOutPath)) return false;

            if (new FileInfo(szOutPath).Length != Size)
                return false;

            using FileStream stm = File.OpenRead(szOutPath);
            using MD5 hasher = MD5.Create();
            string szFileHash = BitConverter.ToString(hasher.ComputeHash(stm)).Replace("-", "").ToLower();
            if (szFileHash != Checksum)
                return false;

            return true;
        }
    }

    public class Manifest
    {
        public readonly string GitHash;
        readonly string RawData;
        internal List<DownloadableFile> Files;

        public Manifest(string szGitHash, string szRawData)
        {
            GitHash = szGitHash;
            RawData = szRawData;
            Files = new();

            // Parse raw manifest
            string[] split = RawData.Split("\r\n").Where(x => x != string.Empty).ToArray();
            for (int i = 1; i < split.Count();)
                Files.Add(new(this, split[i++], split[i++], long.Parse(split[i++]), long.Parse(split[i++])));
        }
    }

    public class Bootstrapper
    {
        static readonly IReadOnlyDictionary<string, string> ZipMap = new Dictionary<string, string>() // todo: figure out how the hell roblox unpacks this without a map
        {
            { "content-avatar.zip",            "content\\avatar" },
            { "content-configs.zip",           "content\\configs" },
            { "content-fonts.zip",             "content\\fonts" },
            { "content-models.zip",            "content\\models" },
            { "content-platform-fonts.zip",    "PlatformContent\\pc\\fonts" },
            { "content-sky.zip",               "content\\sky" },
            { "content-sounds.zip",            "content\\sounds" },
            { "content-terrain.zip",           "PlatformContent\\pc\\terrain" },
            { "content-textures2.zip",         "content\\textures" },
            { "content-textures3.zip",         "PlatformContent\\pc\\textures" },
            { "extracontent-luapackages.zip",  "ExtraContent\\LuaPackages" },
            { "extracontent-models.zip",       "ExtraContent\\models" },
            { "extracontent-places.zip",       "ExtraContent\\places" },
            { "extracontent-textures.zip",     "ExtraContent\\textures" },
            { "extracontent-translations.zip", "ExtraContent\\translations" },
            { "RobloxApp.zip",                 "." },
            { "shaders.zip",                   "shaders" },
            { "ssl.zip",                       "ssl" },
        };

        public static async Task<Manifest> GetManifest(string szGitHash)
        {
            string szRawData = await Web.DownloadStringTaskAsync($"http://{Endpoints.Setup}/version-{szGitHash}-rbxPkgManifest.txt");
            return new(szGitHash, szRawData);
        }

        public static async Task<string> InstallRoblox(Manifest manifest)
        {
            string szInstallationPath = Path.Combine(Roblox.GetInstallationPath(), $"version-{manifest.GitHash}");
            Directory.CreateDirectory(szInstallationPath);

            foreach (var file in manifest.Files)
            {
                string szFilePath = Path.Combine(szInstallationPath, file.Name);
                if (!file.Check(szInstallationPath)) // Don't download it twice!
                    await file.Download(szInstallationPath);

                if (Path.GetExtension(szFilePath) == ".zip")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(szFilePath))
                    {
                        string szExtractTo = Path.Combine(szInstallationPath, ZipMap[file.Name]);
                        if (!Directory.Exists(szExtractTo))
                            Directory.CreateDirectory(szExtractTo);
                        archive.ExtractToDirectory(szExtractTo, true);
                    }
                    File.Delete(szFilePath);
                }
            }

            File.WriteAllText(Path.Combine(szInstallationPath, "AppSettings.xml"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Settings>
    <ContentFolder>content</ContentFolder>
    <BaseUrl>http://www.roblox.com</BaseUrl>
</Settings>
");

            return szInstallationPath;
        }
    }
}
