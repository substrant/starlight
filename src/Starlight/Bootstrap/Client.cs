using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Starlight.Misc.Extensions;

namespace Starlight.Bootstrap;

/// <summary>
///     Represents a Roblox client installation.
/// </summary>
public partial class Client
{
    /// <summary>
    ///     Determines of the installation is common, or stored in Roblox's installation directory.
    /// </summary>
    internal bool IsCommon;

    /// <summary>
    ///     The full path to the launcher executable (RobloxPlayerLauncher.exe).
    /// </summary>
    public string Launcher;

    /// <summary>
    ///     The full path to the installation directory.
    /// </summary>
    public string Location;

    /// <summary>
    ///     The full path to the player executable (RobloxPlayerBeta.exe).
    /// </summary>
    public string Player;

    /// <summary>
    ///     The version hash of the installation.
    /// </summary>
    public string VersionHash;

    internal Client()
    {
    }

    /// <summary>
    ///     A boolean value indicating if the client is installed.
    /// </summary>
    public bool Exists => Directory.Exists(Location);

    /// <summary>
    ///     Instantiates a new <see cref="Client" /> from Roblox's global installation directory.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    public static Client FromCommon(string versionHash)
    {
        if (versionHash == null)
            throw new ArgumentNullException(nameof(versionHash));

        var installPath = Path.Combine(Bootstrapper.GlobalInstallPath, "version-" + versionHash);
        return new Client
        {
            IsCommon = true,
            VersionHash = versionHash,
            Location = installPath,
            Player = Path.Combine(installPath, "RobloxPlayerBeta.exe"),
            Launcher = Path.Combine(installPath, "RobloxPlayerLauncher.exe")
        };
    }

    /// <summary>
    ///     Instantiates a new <see cref="Client" /> from a custom (hinted) Roblox installation directory.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentException" />
    public static Client FromLocal(string installPath, string versionHash = null)
    {
        if (installPath == null)
            throw new ArgumentNullException(nameof(installPath));

        if (versionHash == null)
        {
            var lockFile = Path.Combine(installPath, "Starlight.lock");
            if (!File.Exists(lockFile))
                throw new ArgumentException(
                    "Could not find the version hint file to compensate for the undefined hash.", nameof(installPath));

            versionHash = File.ReadAllText(lockFile);
        }

        return new Client
        {
            VersionHash = versionHash,
            Location = installPath,
            Player = Path.Combine(installPath, "RobloxPlayerBeta.exe"),
            Launcher = Path.Combine(installPath, "RobloxPlayerLauncher.exe")
        };
    }

    /// <summary>
    ///     Gets a list of the <see cref="Client" />'s <see cref="Downloadable" /> instances.
    /// </summary>
    /// <exception cref="TaskCanceledException" />
    public async Task<IList<Downloadable>> GetFilesAsync(CancellationToken token = default)
    {
        var files = new List<Downloadable>();

        var raw = await Bootstrapper.RbxCdnClient.GetAsync(
            new RestRequest($"/version-{VersionHash}-rbxPkgManifest.txt"), token);
        var split = raw.Content.Split("\r\n", "\n").Where(x => x != string.Empty).ToArray();
        for (var i = 1; i < split.Length;)
            files.Add(new Downloadable(VersionHash, split[i++], split[i++], long.Parse(split[i++]),
                long.Parse(split[i++])));

        return files;
    }
}