using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Starlight.Except;
using Starlight.Misc;
using static Starlight.Misc.Shared;
using static Starlight.Misc.Native;

namespace Starlight.Bootstrap;

public class Bootstrapper
{
    // ReSharper disable once PossibleNullReferenceException
    internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

    static string _latestHash; // Cache for a micro-optimization

    static readonly string RobloxPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");

    static readonly string InstallationPath = Path.Combine(RobloxPath, "Versions");

    static readonly List<string> SkippedClients = new();

    /// <summary>
    ///     Fetch an installed Roblox client with its hash.
    /// </summary>
    /// <param name="hash">The hash of the Roblox client.</param>
    /// <returns>The corresponding client to the given hash.</returns>
    /// <exception cref="ClientNotFoundException">Thrown when there is no client with the specified hash.</exception>
    public static Client QueryClient(string hash)
    {
        var client = GetClients().FirstOrDefault(x => x.Hash == hash);
        if (client is null)
            throw new ClientNotFoundException(hash);
        return client;
    }

    /// <summary>
    ///     Fetch the latest hash from Roblox's CDN.
    /// </summary>
    /// <param name="bypassCache">Bypass the cache for long-running tasks.</param>
    /// <returns>The latest Roblox hash.</returns>
    /// <exception cref="HttpException">Thrown when the hash couldn't be fetched due to an external problem.</exception>
    public static string GetLatestHash(bool bypassCache = false)
    {
        if (_latestHash is not null && !bypassCache)
            return _latestHash;

        string version;
        lock (Web)
        {
            version = Web.DownloadString("http://setup.rbxcdn.com/version.txt");
        }

        if (_latestHash is not null)
            Log.Info($"GetLatestHash: Latest Roblox version: {version}");

        return _latestHash = version.Split("version-")[1];
    }

    /// <summary>
    ///     Fetch the latest hash from Roblox's CDN.
    /// </summary>
    /// <param name="bypassCache">Bypass the cache for long-running tasks.</param>
    /// <returns>The latest Roblox hash.</returns>
    /// <exception cref="HttpException">Thrown when the hash couldn't be fetched due to an external problem.</exception>
    public static async Task<string> GetLatestHashAsync(bool bypassCache = false)
    {
        return await Task.Run(() => GetLatestHash(bypassCache));
    }

    /// <summary>
    ///     Install Roblox using the native installer.
    /// </summary>
    /// <returns>The installed Roblox client.</returns>
    /// <exception cref="HttpException">Thrown either when fetching the latest hash fails or the installer download fails.</exception>
    /// <exception cref="IOException">Thrown when there was a problem accessing the filesystem.</exception>
    /// <exception cref="PrematureCloseException">The installer closed too early or the installation timed out.</exception>
    public static Client NativeInstall()
    {
        // TODO: use a less crappy method for checking installer exit
        var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Roblox Player.lnk");
        if (File.Exists(desktopShorctut))
            File.Delete(desktopShorctut);

        var latestHash = GetLatestHash();
        var installPath = Path.Combine(InstallationPath, $"version-{latestHash}");
        var tempPath = Utility.GetTempDir();

        var installerBin = Path.Combine(tempPath, "RobloxPlayerLauncher.exe");
        Log.Debug($"NativeInstall: Downloading installer to {installerBin}...");

        lock (Web)
        {
            Web.DownloadFile($"https://setup.rbxcdn.com/version-{latestHash}-Roblox.exe", installerBin);
        }

        Process setup;
        try
        {
            setup = Process.Start(new ProcessStartInfo(installerBin) { WorkingDirectory = tempPath });
        }
        catch (Win32Exception)
        {
            var ex = new PrematureCloseException();
            Log.Fatal("NativeInstall: Failed to start installer", ex);
            throw ex;
        }

        if (setup is null)
        {
            var ex = new PrematureCloseException();
            Log.Fatal("NativeInstall: Failed to start installer: Process is null", ex);
            throw ex;
        }

        IntPtr hWnd;
        var waitStart = DateTime.Now;
        while ((hWnd = setup.MainWindowHandle) == IntPtr.Zero)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
            if (DateTime.Now - waitStart <= TimeSpan.FromSeconds(10))
                continue;

            var ex = new PrematureCloseException();
            Log.Fatal("NativeInstall: Installer unexpectedly closed", ex);
            throw ex;
        }

        ShowWindow(hWnd, SW_HIDE); // hide window to prevent user from seeing it

        // ReSharper enable PossibleNullReferenceException

        Log.Debug("NativeInstall: Waiting for installer to exit...");

        waitStart = DateTime.Now;
        while (!File.Exists(desktopShorctut))
        {
            Thread.Sleep(TimeSpan.FromSeconds(1.0));

            if (DateTime.Now - waitStart <=
                TimeSpan.FromSeconds(120)) // Two minutes should be plenty. We also don't want CI to hang forever.
                continue;

            var ex = new PrematureCloseException();
            Log.Fatal("NativeInstall: Installer timed out", ex);
            throw ex;
        }

        setup.Kill();
        Utility.WaitShare(Path.Combine(installPath, "RobloxPlayerBeta.exe"), FileShare.Read);

        if (Directory.Exists(installPath))
        {
            Utility.WaitShare(installerBin, FileShare.Delete);
            Thread.Sleep(100);
            Directory.Delete(tempPath, true);

            Log.Debug("NativeInstall: Cleaned up.");
            return new Client(installPath, latestHash);
        }

        // if !installSuccess
        {
            var ex = new BadIntegrityException("Installer failed to execute.");
            Log.Fatal("NativeInstall: Installer failed to execute", ex);
            throw ex;
        }
    }

    /// <summary>
    ///     Install Roblox using the native installer.
    /// </summary>
    /// <returns>The installed Roblox client.</returns>
    /// <exception cref="HttpException">Thrown either when fetching the latest hash fails or the installer download fails.</exception>
    /// <exception cref="IOException">Thrown when there was a problem accessing the filesystem.</exception>
    /// <exception cref="PrematureCloseException">The installer closed too early or the installation timed out.</exception>
    public static async Task<Client> NativeInstallAsync()
    {
        return await Task.Run(NativeInstall);
    }

    /// <summary>
    ///     Get a list of installed Roblox clients.
    /// </summary>
    /// <returns>A read-only list of Roblox clients.</returns>
    /// <exception cref="IOException">Thrown when there was a problem accessing the filesystem.</exception>
    public static IReadOnlyList<Client> GetClients()
    {
        List<Client> clients = new();

        if (!Directory.Exists(InstallationPath)) // No valid installation of Roblox exists.
            return clients;

        foreach (var item in Directory.EnumerateDirectories(InstallationPath))
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

    /// <summary>
    ///     Get the installation manifest for a particular Roblox hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to use.</param>
    /// <returns>The parsed manifest.</returns>
    /// <exception cref="HttpException">Thrown when fetching the manifest fails.</exception>
    public static Manifest GetManifest(string hash)
    {
        try
        {
            string raw;
            lock (Web)
            {
                raw = Web.DownloadString($"http://setup.rbxcdn.com/version-{hash}-rbxPkgManifest.txt");
            }

            return new Manifest(hash, raw);
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"GetManifest: Failed to get manifest for version-{hash}. Is an invalid hash provided?", ex);
            return null;
        }
    }

    /// <summary>
    ///     Get the installation manifest for a particular Roblox hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to use.</param>
    /// <returns>The parsed manifest.</returns>
    /// <exception cref="HttpException">Thrown when fetching the manifest fails.</exception>
    public static async Task<Manifest> GetManifestAsync(string hash)
    {
        return await Task.Run(() => GetManifest(hash));
    }

    /// <summary>
    ///     Install Roblox with an installation manifest.
    /// </summary>
    /// <param name="manifest">The installation manifest to refer to.</param>
    /// <returns>The installed <see cref="Client" /> class.</returns>
    /// <exception cref="BadIntegrityException">The hash of a downloaded file did not match.</exception>
    /// <exception cref="HttpException">Thrown when downloading a file fails.</exception>
    /// <exception cref="IOException">Thrown when there was a problem accessing the filesystem.</exception>
    public static Client Install(Manifest manifest)
    {
        if (!Directory.Exists(InstallationPath))
        {
            Log.Info("Install: Running native installer for initial setup...");
            NativeInstall();

            try
            {
                // Installer may have already installed the client.
                return QueryClient(manifest.Hash);
            }
            catch (ClientNotFoundException)
            {
            }
        }

        Log.Info($"Install: Preparing to install Roblox version-{manifest.Hash}...");
        var tempPath = Utility.GetTempDir();
        var path = Path.Combine(InstallationPath, $"version-{manifest.Hash}");
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
            {
                Log.Warn($"Install: Skipped unknown file \"{file.Name}\".");
            }
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

    /// <summary>
    ///     Install Roblox with an installation manifest.
    /// </summary>
    /// <param name="manifest">The installation manifest to refer to.</param>
    /// <returns>The installed <see cref="Client" /> class.</returns>
    public static async Task<Client> InstallAsync(Manifest manifest)
    {
        return await Task.Run(() => Install(manifest));
    }

    /// <summary>
    ///     Install Roblox with a hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to install.</param>
    /// <returns>The installed <see cref="Client" /> class.</returns>
    public static Client Install(string hash = null)
    {
        if ((hash ??= GetLatestHash()) is null)
            return null;

        var manifest = GetManifest(hash);
        return manifest is not null ? Install(manifest) : null;
    }

    /// <summary>
    ///     Install Roblox with a hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to install.</param>
    /// <returns>The installed <see cref="Client" /> class.</returns>
    public static async Task<Client> InstallAsync(string hash = null)
    {
        return await Task.Run(() => Install(hash));
    }

    /// <summary>
    ///     Uninstall Roblox with a <see cref="Client" /> class.
    /// </summary>
    /// <param name="client">The <see cref="Client" /> class to uninstall.</param>
    public static void Uninstall(Client client)
    {
        Directory.Delete(client.Location, true);

        var clients = GetClients();
        if (clients.Count < 1)
            RemoveShortcuts();
        else
            AddShortcuts(clients[0].Hash);

        Log.Debug($"Uninstall: Uninstalled Roblox version-{client.Hash}.");
    }

    /// <summary>
    ///     Uninstall Roblox with a hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to uninstall.</param>
    /// <exception cref="ClientNotFoundException">Thrown when the specified client doesn't exist.</exception>
    public static void Uninstall(string hash)
    {
        Uninstall(QueryClient(hash ?? GetLatestHash()));
    }

    /// <summary>
    ///     Uninstall Roblox with a hash.
    /// </summary>
    /// <param name="hash">The hash of Roblox to uninstall.</param>
    /// <exception cref="ClientNotFoundException">Thrown when the specified client doesn't exist.</exception>
    public static async Task UninstallAsync(string hash)
    {
        await Task.Run(() => Uninstall(hash));
    }

    internal static void AddShortcuts(string hash, string launcherBin = null)
    {
        RemoveShortcuts();

        launcherBin ??= Path.Combine(InstallationPath, $"version-{hash}", "RobloxPlayerLauncher.exe");
        var workingDir = Path.GetDirectoryName(launcherBin);

        var menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft\\Windows\\Start Menu\\Programs\\Roblox", "Roblox Player.lnk");
        var desktopShorctut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Roblox Player.lnk");

        Utility.CreateShortcut(menuShortcut, launcherBin, workingDir);
        Utility.CreateShortcut(desktopShorctut, launcherBin, workingDir);

        Log.Debug("AddShortcuts: Created shortcuts.");
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

        Log.Debug("RemoveShortcuts: Removed shortcuts.");
    }
}