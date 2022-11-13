using Microsoft.Win32;
using Starlight.Misc;
using Starlight.Misc.Extensions;
using Starlight.Misc.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using static Starlight.Misc.Shared;

namespace Starlight.Bootstrap;

/// <summary>
///     Contains methods for installing, and uninstalling Roblox, as well as auxliary functions for installation.
/// </summary>
public static class Bootstrapper
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

    /* Clients */

    static readonly RestClient RbxCdnClient = new("https://setup.rbxcdn.com");

    /* Data */

    static DateTime _lastVersionHashFetch = DateTime.MinValue;
    static string _latestVersionHash;

    /* Versions */

    /// <summary>
    ///    Get the directory of a client at the given scope.
    /// </summary>
    /// <param name="scope">The scope to use.</param>
    /// <returns>The directory that corresponds to the given scope.</returns>
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

    /// <summary>
    ///     Get the latest version hash of Roblox.
    /// </summary>
    /// <param name="bypassCache">As an optimization opportunity, Starlight will cache the version hash to avoid making unnecessary web requests. Set this to <c>true</c> to bypass caching the hash.</param>
    /// <param name="token">The cancellation token to use.</param>
    /// <returns>The latest version hash of Roblox.</returns>
    /// <exception cref="TaskCanceledException">Thrown if the task is cancelled.</exception>
    public static async Task<string> GetLatestVersionHashAsync(bool bypassCache = false, CancellationToken token = default)
    {
        if (!bypassCache && (_latestVersionHash is not null || DateTime.Now - _lastVersionHashFetch > TimeSpan.FromDays(1)))
            return await Task.FromResult(_latestVersionHash);
        
        var version = await RbxCdnClient.GetAsync(new RestRequest("/version.txt"), token);

        _lastVersionHashFetch = DateTime.Now;
        return _latestVersionHash = version.Content.Split("version-")[1];
    }

    /* Clients */

    /// <summary>
    ///    Get a list of installed clients.
    /// </summary>
    /// <param name="scope">The scope to search.</param>
    /// <returns></returns>
    public static IList<Client> GetClients(ClientScope scope = ClientScope.Global)
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

    /// <summary>
    ///     Get a client by its version hash.
    /// </summary>
    /// <param name="versionHash">The version hash to search for.</param>
    /// <param name="scope">The scope to search in.</param>
    /// <returns>The client that was found, or <see cref="null"/> if it doesn't exist.</returns>
    public static Client QueryClient(string versionHash, ClientScope scope = ClientScope.Global)
    {
        var client = GetClients(scope).FirstOrDefault(x => x.VersionHash == versionHash);
        return client;
    }

    /// <summary>
    ///     Get the first client that matches the given predicate in the given scope.
    /// </summary>
    /// <param name="scope">The scope to search in.</param>
    /// /// <param name="predicate">The scope to search in.</param>
    /// <returns>The first client that matches the given predicate,  or null if not found.</returns>
    public static Client GetFirstClient(ClientScope scope = ClientScope.Global, Func<Client, bool> predicate = null)
    {
        return GetClients(scope).FirstOrDefault(predicate ?? (_ => true));
    }

    /// <summary>
    ///    Get the latest client in the given scope.
    /// </summary>
    /// <param name="scope">The scope to set the client's location in.</param>
    /// <param name="token">The cancellation token to use.</param>
    /// <returns>The latest client. If the client doesn't exist, it will still return a client. Use <see cref="Client.Exists"/> to check if it exists.</returns>
    /// <exception cref="TaskCanceledException">Thrown if the task is cancelled.</exception>
    public static async Task<Client> GetLatestClientAsync(ClientScope scope = ClientScope.Global, CancellationToken token = default)
    {
        var versionHash = await GetLatestVersionHashAsync(false, token);
        return new Client(versionHash, scope);
    }

    /* Registry */

    /// <summary>
    ///     Checks if Roblox is registered in the registry.
    /// </summary>
    /// <returns>
    ///     A boolean variable that determines if Roblox has been registered in the past.<br/>
    ///     <strong>Note:</strong> This does not check if Roblox is currently registered. If Roblox has been installed prior to uninstallation, this method will return <c>true</c>.
    /// </returns>
    public static bool IsRobloxRegistered()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        using var softKey = hkcu.OpenSubKey(@"Software\ROBLOX Corporation");
        return softKey is null;
    }

    /// <summary>
    ///     Adds Roblox's <c>roblox-player</c> scheme class to the registry.
    /// </summary>
    /// <param name="client">The client to use for registration.</param>
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

    /// <summary>
    ///     Removes Roblox's scheme class from the registry.
    /// </summary>
    public static void UnregisterClass()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        hkcu.DeleteSubKeyTree(@"Software\Classes\roblox-player", false);
    }

    /// <summary>
    ///     Adds Roblox's environment to the registry.<br/>
    ///     Registry envirionment keys are required for Roblox's player to function properly.
    /// </summary>
    /// <param name="client"></param>
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

    /// <summary>
    ///    Removes Roblox's environment from the registry.
    /// </summary>
    public static void UnregisterClient()
    {
        using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        hkcu.DeleteSubKeyTree(@"Software\ROBLOX Corporation", false);
    }

    /* Installation */

    /// <summary>
    ///     Installs a <see cref="Client"/> to the computer.<br/>
    ///     This method will download the client, extract it, and register it.
    /// </summary>
    /// <param name="client">The client object to install.</param>
    /// <param name="tracker">The progress tracker to use when installing.</param>
    /// <param name="cfg">The installation configuration to use when installing.</param>
    /// <param name="token">The cancellation token to use.</param>
    /// <exception cref="TaskCanceledException">Thrown if the task is cancelled.</exception>
    public static async Task InstallAsync(Client client, ProgressTracker tracker = null, InstallConfig cfg = null, CancellationToken token = default)
    {
        cfg ??= InstallConfig.Default;

        // Create directory (do an uninstallation if needed)
        if (Directory.Exists(client.Location))
            Directory.Delete(client.Location, true);
        Directory.CreateDirectory(client.Location);

        // Get files
        tracker?.Start(2, "Getting files");
        var files = await client.GetFilesAsync();

        // Download files
        var downloadTracker = tracker?.SubStep(files.Count);

        void Download(Downloadable file)
        {
            downloadTracker?.Step($"Downloading {file.Name}");
            AsyncHelpers.RunSync(() => file.DownloadAsync(client.Location));
        }

        await Utility.DisperseActionsAsync(files, Download, cfg.DownloadConcurrency, token);

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

        await Utility.DisperseActionsAsync(files, Extract, cfg.UnzipConcurrency, token);

        File.WriteAllText(Path.Combine(client.Location, "AppSettings.xml"),
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<Settings>\r\n    <ContentFolder>content</ContentFolder>\r\n    <BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>");

        // Register class and client
        if (cfg.RegisterClass)
            RegisterClass(client);

        if (cfg.RegisterClient)
            RegisterClient(client);
    }

    /// <summary>
    ///     Uninstalls a <see cref="Client"/>.<br/>
    ///     This method will unregister the client and delete the client's directory.
    /// </summary>
    /// <param name="client">The client object to uninstall.</param>
    /// <param name="cfg">The installation configuration to use when uninstalling.</param>
    public static void Uninstall(Client client, InstallConfig cfg = null)
    {
        cfg ??= InstallConfig.Default;

        Directory.Delete(client.Location, true);

        client = GetFirstClient();
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

    /* Shortcuts */

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