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

        public async Task<string> Download(string dir)
        {
            var outPath = Path.Combine(dir, Name);
            await Web.DownloadFileTaskAsync($"http://setup.rbxcdn.com/version-{Parent.Hash}-{Name}", outPath);

            if (!Check(dir))
                throw new Exception("Download failed: Hash/size mismatch.");

            return outPath;
        }

        public bool Check(string dir)
        {
            var outPath = Path.Combine(dir, Name);
            if (!File.Exists(outPath)) return false;

            if (new FileInfo(outPath).Length != Size)
                return false;
            
            using FileStream stm = File.OpenRead(outPath);
            using MD5 hasher = MD5.Create();
            var hash = BitConverter.ToString(hasher.ComputeHash(stm)).Replace("-", "").ToLower();
            if (hash != Checksum)
                return false;

            return true;
        }
    }

    public class Manifest
    {
        public readonly string Hash;
        
        readonly string RawData;
        
        internal List<DownloadableFile> Files;

        public Manifest(string hash, string raw)
        {
            Hash = hash;
            RawData = raw;
            Files = new();

            // Parse raw manifest
            var split = RawData.Split("\r\n").Where(x => x != string.Empty).ToArray();
            for (int i = 1; i < split.Count();)
                Files.Add(new(this, split[i++], split[i++], long.Parse(split[i++]), long.Parse(split[i++])));
        }
    }
    
    public class Client
    {
        public readonly string Directory;

        public readonly string Path;
        
        public readonly string Hash;

        public Client(string path, string hash)
        {
            Directory = path;
            Path = System.IO.Path.Combine(path, "RobloxPlayerBeta.exe");
            Hash = hash;
        }
    }

    public class InstallSettings
    {
        public bool CreateShortcuts = false;
    }
    
    public class Bootstrapper
    {
        public static async Task<string> GetLatestHash()
        {
            var version = await Web.DownloadStringTaskAsync($"http://setup.rbxcdn.com/version.txt");
            return version.Split("version-")[1];
        }

        internal static string GetInstallationPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "Roblox\\Versions");
        }

        public static IReadOnlyList<Client> GetClients()
        {
            List<Client> clients = new();

            var path = GetInstallationPath();
            if (path == null)
                return clients;
            
            foreach (var item in Directory.EnumerateDirectories(path))
            {
                if (!File.Exists(Path.Combine(item, "RobloxPlayerBeta.exe")))
                    continue;
                
                var fileHash = Path.GetFileName(item).Split("version-")[1];
                clients.Add(new Client(item, fileHash));
            }

            return clients;
        }

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

        public static async Task<Manifest> GetManifest(string hash)
        {
            string raw = await Web.DownloadStringTaskAsync($"http://setup.rbxcdn.com/version-{hash}-rbxPkgManifest.txt");
            return new(hash, raw);
        }

        public static async Task<string> Install(Manifest manifest, InstallSettings settings = null)
        {
            settings ??= new();
            
            var path = Path.Combine(GetInstallationPath(), $"version-{manifest.Hash}");
            Directory.CreateDirectory(path);

            foreach (var file in manifest.Files)
            {
                var filePath = Path.Combine(path, file.Name);
                if (!file.Check(path))
                    await file.Download(path);

                if (Path.GetExtension(filePath) == ".zip")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(filePath))
                    {
                        var extractTo = Path.Combine(path, ZipMap[file.Name]);
                        if (!Directory.Exists(extractTo))
                            Directory.CreateDirectory(extractTo);
                        archive.ExtractToDirectory(extractTo, true);
                    }
                    
                    File.Delete(filePath);
                }
            }

            // smh idk where the client got this so i just copied it and called it a day
            File.WriteAllText(Path.Combine(path, "AppSettings.xml"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Settings>
    <ContentFolder>content</ContentFolder>
    <BaseUrl>http://www.roblox.com</BaseUrl>
</Settings>
");
            
            if (settings.CreateShortcuts)
                AddShortcuts(path);

            return path;
        }
        
        public static async Task<string> Install(string hash) =>
            await Install(await GetManifest(hash));
        
        public static void Uninstall(string hash)
        {
            var path = Path.Combine(GetInstallationPath(), $"version-{hash}");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            var clients = GetClients();
            if (clients.Count < 1)
                RemoveShortcuts();
            else
                AddShortcuts(clients[0].Hash);
        }

        internal static void AddShortcuts(string hash, string launcher = null)
        {
            RemoveShortcuts();

            var roblox = Path.Combine(GetInstallationPath(), $"version-{hash}");
            launcher ??= Path.Combine(roblox, "RobloxPlayerLauncher.exe");

            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Roblox Player.lnk");
            
            Utility.CreateShortcut(menuShortcut, launcher, roblox);
            Utility.CreateShortcut(desktopShorctut, launcher, roblox);
        }

        internal static void RemoveShortcuts()
        {
            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Roblox Player.lnk");

            if (File.Exists(menuShortcut))
                File.Delete(menuShortcut);

            if (File.Exists(desktopShorctut))
                File.Delete(desktopShorctut);
        }
    }
}
