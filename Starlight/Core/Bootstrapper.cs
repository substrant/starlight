using log4net;
using Starlight.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Starlight.Misc.Shared;

namespace Starlight.Core
{
    public class Bootstrapper
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // TODO: Figure out how to do this without a map like the official launcher.
        static readonly IReadOnlyDictionary<string, string> ZipMap = new Dictionary<string, string>()
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
        
        public static Client QueryClient(string hash)
        {
            var client = GetClients().FirstOrDefault(x => x.Hash == hash);
            Log.Debug($"Client {hash} query " + (client is null ? "succeeded." : "failed."));
            return client;
        }

        public static string GetLatestHash()
        {
            var version = Web.DownloadString("http://setup.rbxcdn.com/version.txt");
            return version.Split("version-")[1];
        }

        public static async Task<string> GetLatestHashAsync() =>
            await Task.Run(GetLatestHash);

        internal static string GetInstallationPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var installPath = Path.Combine(localAppData, "Roblox\\Versions");
            if (Directory.Exists(installPath))
                return installPath;
            
            Log.Error("Installation directory does not exist.");
            return null;

        }

        public static IReadOnlyList<Client> GetClients()
        {
            List<Client> clients = new();

            var path = GetInstallationPath();
            if (path == null) // No valid installation of Roblox exists.
                return clients;
            
            foreach (var item in Directory.EnumerateDirectories(path))
            {
                // I don't want this to error because someone was an idiot and put a random folder in the directory.
                if (!File.Exists(Path.Combine(item, "RobloxPlayerBeta.exe")) || !Path.GetFileName(item).StartsWith("version-"))
                {
                    Log.Warn($"Skipping {Path.GetFileName(item)} because it doesn't look like a Roblox installation.");
                    continue;
                }
                
                var fileHash = Path.GetFileName(item).Split("version-")[1];
                clients.Add(new Client(item, fileHash));

                Log.Debug($"Found {fileHash}.");
            }

            return clients;
        }
        
        public static Manifest GetManifest(string hash)
        {
            try
            {
                var raw = Web.DownloadString($"http://setup.rbxcdn.com/version-{hash}-rbxPkgManifest.txt");
                return new Manifest(hash, raw);
            }
            catch (HttpRequestException)
            {
                Log.Error($"Failed to get manifest for version-{hash}. This is probably because the version doesn't exist.");
                return null;
            }
        }

        public static async Task<Manifest> GetManifestAsync(string hash) =>
            await Task.Run(() => GetManifest(hash));
        
        public static Client Install(Manifest manifest)
        {
            Log.Info($"Preparing to install Roblox version-{manifest.Hash}...");
            var tempPath = Utility.GetTempDir();
            var path = Path.Combine(GetInstallationPath(), $"version-{manifest.Hash}");
            Directory.CreateDirectory(path);

            foreach (var file in manifest.Files)
            {
                var filePath = Path.Combine(tempPath, file.Name);
                Log.Info($"Downloading {file.Name}...");
                file.Download(tempPath);

                if (Path.GetExtension(filePath) == ".zip")
                {
                    using (var archive = ZipFile.OpenRead(filePath))
                    {
                        if (!ZipMap.TryGetValue(file.Name, out var extractPath))
                        {
                            Log.Warn($"No extraction path found for \"{file.Name}\".");
                            goto ExtractFin;
                        }

                        var extractTo = Path.Combine(path, extractPath);
                        if (!Directory.Exists(extractTo))
                            Directory.CreateDirectory(extractTo);

                        archive.ExtractToDirectory(extractTo, true);
                    }
                    
                    ExtractFin:
                    File.Delete(filePath);
                }
                else
                    Log.Warn($"Skipped unknown file \"{file.Name}\".");
            }

            // No clue where the client got this. I just copied it and called it a day
            File.WriteAllText(Path.Combine(path, "AppSettings.xml"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Settings>
    <ContentFolder>content</ContentFolder>
    <BaseUrl>http://www.roblox.com</BaseUrl>
</Settings>
");

            // Clean up the temp directory
            Directory.Delete(tempPath, true);
            Log.Info("Installation completed.");

            return new Client(path, manifest.Hash);
        }

        public static async Task<Client> InstallAsync(Manifest manifest) =>
            await Task.Run(() => Install(manifest));

        public static Client Install(string hash)
        {
            var manifest = GetManifest(hash);
            if (manifest is not null)
                return Install(manifest);

            var ex = new BootstrapException("Failed to get manifest.");
            Log.Fatal("Failed to get manifest. Is an invalid hash provided?", ex);
            throw ex;
        }

        public static async Task<Client> InstallAsync(string hash) =>
            await Task.Run(() => Install(hash));

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

            Log.Info($"Uninstalled Roblox version-{hash}.");
        }

        public static void Uninstall(Client client) =>
            Uninstall(client.Hash);

        internal static void AddShortcuts(string hash, string launcher = null)
        {
            RemoveShortcuts();

            var roblox = Path.Combine(GetInstallationPath(), $"version-{hash}");
            launcher ??= Path.Combine(roblox, "RobloxPlayerLauncher.exe");

            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Roblox Player.lnk");
            
            Utility.CreateShortcut(menuShortcut, launcher, roblox);
            Utility.CreateShortcut(desktopShorctut, launcher, roblox);

            Log.Info("Created shortcuts.");
        }

        internal static void RemoveShortcuts()
        {
            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Roblox Player.lnk");

            if (File.Exists(menuShortcut))
                File.Delete(menuShortcut);

            if (File.Exists(desktopShorctut))
                File.Delete(desktopShorctut);

            Log.Info("Removed shortcuts.");
        }
    }
}
