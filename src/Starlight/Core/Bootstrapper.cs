using log4net;
using Starlight.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static Starlight.Misc.Shared;

namespace Starlight.Core
{
    public class Bootstrapper
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static readonly IReadOnlyDictionary<string, string> ZipMap = new Dictionary<string, string>
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
            if (client is null)
                Log.Warn($"QueryClient {hash}: Failed to find client");
            else
                Log.Debug($"QueryClient {hash}: Success");
            return client;
        }

        static string _latestHash; // Cache for a micro-optimization
        public static string GetLatestHash()
        {
            if (_latestHash is not null)
                return _latestHash;
            
            try
            {
                string version;
                lock (Web)
                    version = Web.DownloadString("http://setup.rbxcdn.com/version.txt");
                Log.Info($"GetLatestHash: Latest Roblox version: {version}");
                return _latestHash = version.Split("version-")[1];
            }
            catch (HttpException innerEx)
            {
                var ex = new BootstrapException("Failed to get latest hash.", innerEx);
                Log.Fatal("GetLatestHash: Failed to get latest hash", ex);
                throw ex;
            }
        }

        public static async Task<string> GetLatestHashAsync() =>
            await Task.Run(GetLatestHash);

        public static Client NativeInstall()
        {
            var latestHash = GetLatestHash();
            var installPath = Path.Combine(GetInstallationPath(), $"version-{latestHash}");
            var tempPath = Utility.GetTempDir();
            
            var installerBin = Path.Combine(tempPath, "RobloxPlayerLauncher.exe");
            Log.Debug($"NativeInstall: Downloading installer to {installerBin}...");
            try
            {
                lock (Web)
                    Web.DownloadFile($"https://setup.rbxcdn.com/version-{latestHash}-Roblox.exe", installerBin);
            }
            catch (HttpException innerEx)
            {
                var ex = new BootstrapException("Failed to download installer.", innerEx);
                Log.Fatal("NativeInstall: Failed to download installer", ex);
                throw ex;
            }

            Process setup;
            try
            {
                setup = Process.Start(new ProcessStartInfo(installerBin) { WorkingDirectory = tempPath });
            }
            catch (Win32Exception innerEx)
            {
                var ex = new BootstrapException("Failed to start installer.", innerEx);
                Log.Fatal("NativeInstall: Failed to start installer", ex);
                throw ex;
            }

            if (setup is null)
            {
                var ex = new BootstrapException("Failed to start installer: Process is null.");
                Log.Fatal("NativeInstall: Failed to start installer: Process is null", ex);
                throw ex;
            }

            Log.Debug("NativeInstall: Waiting for installer to exit...");
            setup.WaitForExit();

            if (Directory.Exists(installPath))
            {
                Thread.Sleep(1000); // Idk it just fixes access denied exception :shrug:
                Directory.Delete(tempPath, true);
                Log.Debug("NativeInstall: Cleaned up.");
                return new Client(installPath, latestHash);
            }

            // if !installSuccess
            {
                var ex = new BootstrapException("Installer failed to execute.");
                Log.Fatal("NativeInstall: Installer failed to execute", ex);
                throw ex;
            }
        }

        public static async Task<Client> NativeInstallAsync() =>
            await Task.Run(NativeInstall);

        static volatile bool _loggedInstallPathDoesntExist;
        internal static string GetInstallationPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var installPath = Path.Combine(localAppData, "Roblox\\Versions");

            if (Directory.Exists(installPath))
                return installPath;

            if (_loggedInstallPathDoesntExist)
                return null;
            _loggedInstallPathDoesntExist = true;

            Log.Warn("GetInstallationPath: Installation path doesn't exist");
            return null;
        }

        static readonly List<string> SkippedClients = new();
        public static IReadOnlyList<Client> GetClients()
        {
            List<Client> clients = new();

            var path = GetInstallationPath();
            if (path == null) // No valid installation of Roblox exists.
                return clients;
            
            foreach (var item in Directory.EnumerateDirectories(path))
            {               
                var dirName = Path.GetFileName(item);

                // Multiple threads could access this
                lock (SkippedClients)
                {
                    // I don't want this to error because someone was an idiot and put a random folder in the directory.
                    if (!dirName.StartsWith("version-"))
                    {
                        if (!SkippedClients.Contains(dirName))
                        {
                            SkippedClients.Add(dirName);
                            Log.Warn($"GetClients: Skipping {dirName}: invalid directory name.");
                        }
                        continue;
                    }

                    if (!File.Exists(Path.Combine(item, "RobloxPlayerBeta.exe")))
                    {
                        if (!SkippedClients.Contains(dirName))
                        {
                            if (File.Exists(Path.Combine(item, "RobloxStudioBeta.exe")))
                                Log.Warn($"GetClients: Skipping {Path.GetFileName(item)}: invalid installation.");
                            else
                                Log.Warn($"GetClients: Skipping {Path.GetFileName(item)}: invalid installation.");
                        }
                        continue;
                    }
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
                string raw;
                lock (Web) 
                    raw = Web.DownloadString($"http://setup.rbxcdn.com/version-{hash}-rbxPkgManifest.txt");
                return new Manifest(hash, raw);
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"GetManifest: Failed to get manifest for version-{hash}. Is an invalid hash provided?", ex);
                return null;
            }
        }

        public static async Task<Manifest> GetManifestAsync(string hash) =>
            await Task.Run(() => GetManifest(hash));

        public static Client Install(Manifest manifest)
        {
            var installationPath = GetInstallationPath();
            if (installationPath is null)
            {
                Log.Info("Install: Running native installer for initial setup...");
                NativeInstall();
                var client = QueryClient(manifest.Hash);
                if (client is not null) // Installer may have already installed the client.
                    return client;
                installationPath = GetInstallationPath();
            }

            Log.Info($"Install: Preparing to install Roblox version-{manifest.Hash}...");
            var tempPath = Utility.GetTempDir();
            var path = Path.Combine(installationPath, $"version-{manifest.Hash}");
            Directory.CreateDirectory(path);

            // Download all files in parallel
            Utility.DisperseActions(manifest.Files, file =>
            {
                Log.Info($"Install: Downloading {file.Name}...");
                file.Download(tempPath);
            }, 5);

            // Unzip all files
            foreach (var file in manifest.Files)
            {
                var filePath = Path.Combine(tempPath, file.Name);
                if (Path.GetExtension(filePath) == ".zip")
                {
                    Log.Debug($"Install: Unzipping {file.Name}...");
                    using (var archive = ZipFile.OpenRead(filePath))
                    {
                        if (!ZipMap.TryGetValue(file.Name, out var extractPath))
                        {
                            Log.Warn($"Install: No extraction path found for \"{file.Name}\".");
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
                    Log.Warn($"Install: Skipped unknown file \"{file.Name}\".");
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
            Log.Info("Install: Installation completed.");

            return new Client(path, manifest.Hash);
        }

        public static async Task<Client> InstallAsync(Manifest manifest) =>
            await Task.Run(() => Install(manifest));

        public static Client Install(string hash = null)
        {
            hash ??= GetLatestHash();

            var manifest = GetManifest(hash);
            if (manifest is not null)
                return Install(manifest);

            var ex = new BootstrapException("Failed to get manifest.");
            Log.Fatal($"Install: Failed to get manifest for version-{hash}", ex);
            throw ex;
        }

        public static async Task<Client> InstallAsync(string hash = null) =>
            await Task.Run(() => Install(hash));

        public static void Uninstall(string hash = null)
        {
            hash ??= GetLatestHash();

            var path = Path.Combine(GetInstallationPath(), $"version-{hash}");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            var clients = GetClients();
            if (clients.Count < 1)
                RemoveShortcuts();
            else
                AddShortcuts(clients[0].Hash);

            Log.Debug($"Uninstall: Uninstalled Roblox version-{hash}.");
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

            Log.Debug("AddShortcuts: Created shortcuts.");
        }

        internal static void RemoveShortcuts()
        {
            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Roblox Player.lnk");

            if (File.Exists(menuShortcut))
                File.Delete(menuShortcut);

            if (File.Exists(desktopShorctut))
                File.Delete(desktopShorctut);

            Log.Debug("RemoveShortcuts: Removed shortcuts.");
        }
    }
}
