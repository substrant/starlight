using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using Starlight.Misc;
using Starlight.Misc.Extensions;
using Starlight.Misc.Profiling;
using static Starlight.Misc.Shared;

namespace Starlight.Bootstrap;

public class Bootstrapper
{
    static readonly IReadOnlyDictionary<string, string> ZipMap = new Dictionary<string, string>
    {
        { "content-avatar.zip", "content\\avatar" },
        { "content-configs.zip", "content\\configs" },
        { "content-fonts.zip", "content\\fonts" },
        { "content-models.zip", "content\\models" },
        { "content-platform-fonts.zip", "PlatformContent\\pc\\fonts" },
        { "content-sky.zip", "content\\sky" },
        { "content-sounds.zip", "content\\sounds" },
        { "content-terrain.zip", "PlatformContent\\pc\\terrain" },
        { "content-textures2.zip", "content\\textures" },
        { "content-textures3.zip", "PlatformContent\\pc\\textures" },
        { "extracontent-luapackages.zip", "ExtraContent\\LuaPackages" },
        { "extracontent-models.zip", "ExtraContent\\models" },
        { "extracontent-places.zip", "ExtraContent\\places" },
        { "extracontent-textures.zip", "ExtraContent\\textures" },
        { "extracontent-translations.zip", "ExtraContent\\translations" },
        { "RobloxApp.zip", "." },
        { "shaders.zip", "shaders" },
        { "ssl.zip", "ssl" }
    };

    static string _latestVersionHash; // Cache for a micro-optimization

    public static string GetScopeDirectory(ClientScope scope)
    {
        return scope switch
        {
            ClientScope.Global => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "Versions"),
            ClientScope.Local => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roblox"),
            _ => null
        };
    }

    public static async Task<string> GetLatestVersionHashAsync(bool bypassCache = false)
    {
        if (!bypassCache && _latestVersionHash is not null)
            return await Task.FromResult(_latestVersionHash);

        var version = await Web.DownloadStringAsync("http://setup.rbxcdn.com/version.txt");
        return _latestVersionHash = version.Split("version-")[1];
    }

    public static string GetLatestVersionHash(bool bypassCache = false)
    {
        if (!bypassCache && _latestVersionHash is not null)
            return _latestVersionHash;

        var version = Web.DownloadString("http://setup.rbxcdn.com/version.txt");
        return _latestVersionHash = version.Split("version-")[1];
    }

    public static IReadOnlyList<Client> GetClients(ClientScope scope = ClientScope.Global)
    {
        List<Client> clients = new();
        var installPath = GetScopeDirectory(scope);

        if (!Directory.Exists(installPath)) // No valid installation of Roblox exists.
            return clients;

        foreach (var dir in Directory.EnumerateDirectories(installPath))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName.StartsWith("version-") && File.Exists(Path.Combine(dir, "RobloxPlayerBeta.exe")))
                clients.Add(new Client(dirName.Split("version-")[1], scope));
        }

        return clients;
    }

    public static Client QueryClient(string versionHash, ClientScope scope = ClientScope.Global)
    {
        var client = GetClients(scope).FirstOrDefault(x => x.VersionHash == versionHash);
        return client;
    }

    public static Client QueryClientDesperate(ClientScope scope = ClientScope.Global)
    {
        return GetClients(scope).FirstOrDefault();
    }

    public static Client GetLatestClient(ClientScope scope = ClientScope.Global)
    {
        var versionHash = GetLatestVersionHash();
        return new Client(versionHash, scope);
    }

    public static async Task<Client> GetLatestClientAsync(ClientScope scope = ClientScope.Global)
    {
        var versionHash = await GetLatestVersionHashAsync();
        return new Client(versionHash, scope);
    }

    public static bool IsRobloxRegistered()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        using var softKey = hkcu.OpenSubKey(@"Software\ROBLOX Corporation");
        return softKey is null;
    }

    public static void RegisterClass(Client client)
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);

        using var classKey = hkcu.CreateSubKey(@"Software\Classes\roblox-player", true);
        classKey?.SetValue(null, "URL: Roblox Protocol", RegistryValueKind.String);
        classKey?.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);

        using var defaultIconKey = classKey?.CreateSubKey("DefaultIcon", true);
        defaultIconKey?.SetValue(null, client.Launcher, RegistryValueKind.String);

        using var schemeKey = classKey?.CreateSubKey(@"shell\open\command", true);
        schemeKey?.SetValue(null, '"' + client.Launcher + "\" %1", RegistryValueKind.String);
    }

    public static void UnregisterClass()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        hkcu.DeleteSubKeyTree(@"Software\Classes\roblox-player", false);
    }

    public static void RegisterClient(Client client)
    {
        // Roblox's player takes care of this for us, but we might as well add it.
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);

        using var softKey = hkcu.CreateSubKey(@"Software\ROBLOX Corporation", true);

        using var baseEnvKey = softKey?.CreateSubKey("Environments", true);
        baseEnvKey?.SetValue("roblox-player", "roblox-player", RegistryValueKind.String);

        using var envKey = baseEnvKey?.CreateSubKey("roblox-player", true);
        envKey?.SetValue(null, client.Launcher, RegistryValueKind.String);
        envKey?.SetValue("curPlayerUrl", "www.roblox.com", RegistryValueKind.String);
        envKey?.SetValue("curPlayerVer", "version-" + client.VersionHash);
        envKey?.SetValue("version", "version-" + client.VersionHash);

        using var capabilitiesKey = envKey?.OpenSubKey("Capabiltiies", true);
        capabilitiesKey?.SetValue("ApplicationDescription", "Play Roblox!", RegistryValueKind.String);
        capabilitiesKey?.SetValue("ApplicationIcon", '"' + client.Launcher + "\",0", RegistryValueKind.ExpandString);
        capabilitiesKey?.SetValue("ApplicationName", "Roblox Player", RegistryValueKind.String);

        using var urlAssocKey = capabilitiesKey?.CreateSubKey("UrlAssociations", true);
        urlAssocKey?.SetValue("roblox-player", "roblox-player");
    }

    public static void UnregisterClient()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        hkcu.DeleteSubKeyTree(@"Software\ROBLOX Corporation", false);
    }

    public static async Task InstallAsync(Client client, ProgressTracker tracker = null, InstallConfig cfg = null)
    {
        cfg ??= InstallConfig.Default;

        // Create directory (do an uninstallation if needed)
        if (Directory.Exists(client.Location))
            Directory.Delete(client.Location, true);
        Directory.CreateDirectory(client.Location);

        // Get files
        tracker?.Start(2, "Getting files");
        var files = await client.GetFiles();

        // Download files
        var downloadTracker = tracker?.SubStep(files.Count);

        void Download(Downloadable file)
        {
            downloadTracker?.Step($"Downloading {file.Name}");
            file.Download(client.Location);
        }

        await Utility.DisperseActionsAsync(files, Download, cfg.DownloadConcurrency);

        // Post-download (extract and delete)
        var postDownloadTracker = tracker?.SubStep(files.Count);

        void Extract(Downloadable file)
        {
            var filePath = Path.Combine(client.Location, file.Name);
            var fileExt = Path.GetExtension(file.Name);

            if (fileExt != ".zip" || !ZipMap.TryGetValue(file.Name, out var extractPath))
            {
                if (fileExt == ".exe")
                {
                    postDownloadTracker?.Step($"Skipping {file.Name}");
                    return;
                }

                postDownloadTracker?.Step($"Deleting {file.Name}");
                goto ExtractFin;
            }

            var extractTo = Path.Combine(client.Location, extractPath);
            if (!Directory.Exists(extractTo)) Directory.CreateDirectory(extractTo);

            postDownloadTracker?.Step($"Extracting {file.Name}");
            using (var archive = ZipFile.OpenRead(filePath))
            {
                archive.ExtractToDirectory(extractTo, true);
            }

            ExtractFin:
            File.Delete(filePath);
        }

        await Utility.DisperseActionsAsync(files, Extract, cfg.UnzipConcurrency);

        File.WriteAllText(Path.Combine(client.Location, "AppSettings.xml"),
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<Settings>\r\n    <ContentFolder>content</ContentFolder>\r\n    <BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>");

        // Register class and client
        if (cfg.RegisterClass)
            RegisterClass(client);

        if (cfg.RegisterClient)
            RegisterClient(client);
    }

    public static void Install(Client client, InstallConfig cfg = null)
    {
        AsyncHelpers.RunSync(() => InstallAsync(client, null, cfg));
    }

    public static void Uninstall(Client client, InstallConfig cfg = null)
    {
        if (client is null)
            return;

        cfg ??= InstallConfig.Default;

        Directory.Delete(client.Location, true);

        client = QueryClientDesperate();
        if (client is not null)
        {
            if (cfg.RegisterClass)
                RegisterClass(client);

            if (cfg.RegisterClient)
                RegisterClient(client);

            AddShortcuts(client, cfg);
        }
        else
        {
            UnregisterClass();
            UnregisterClient();
            RemoveShortcuts();
        }
    }

    internal static void AddShortcuts(Client client, InstallConfig cfg)
    {
        RemoveShortcuts();

        if (cfg.CreateStartMenuShortcut)
        {
            var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
            Utility.CreateShortcut(menuShortcut, client.Launcher, client.Location);
        }

        // ReSharper disable once InvertIf
        if (cfg.CreateDesktopShortcut)
        {
            var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Roblox Player.lnk");
            Utility.CreateShortcut(desktopShorctut, client.Launcher, client.Location);
        }
    }

    internal static void RemoveShortcuts()
    {
        var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
        var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Roblox Player.lnk");

        if (File.Exists(menuShortcut))
            File.Delete(menuShortcut);

        if (File.Exists(desktopShorctut))
            File.Delete(desktopShorctut);
    }
}