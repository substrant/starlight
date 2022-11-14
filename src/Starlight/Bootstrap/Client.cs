using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Starlight.Misc.Extensions;
using static Starlight.Misc.Shared;

namespace Starlight.Bootstrap;

/// <summary>
///     Represents a Roblox client installation.
/// </summary>
public partial class Client
{
    /// <summary>
    ///     The full path to the installation directory.
    /// </summary>
    public readonly string Location;

    /// <summary>
    ///     A boolean value indicating if the client is installed.
    /// </summary>
    public bool Exists => Directory.Exists(Location);

    /// <summary>
    ///     The full path to the launcher executable (RobloxPlayerLauncher.exe).
    /// </summary>
    public readonly string Launcher;

    /// <summary>
    ///     The full path to the player executable (RobloxPlayerBeta.exe).
    /// </summary>
    public readonly string Player;

    /// <summary>
    ///     The scope of the installation.
    /// </summary>
    public readonly ClientScope Scope;

    /// <summary>
    ///     The version hash of the installation.
    /// </summary>
    public readonly string VersionHash;

    /// <summary>
    ///     Instantiates a new <see cref="Client"/>.
    /// </summary>
    public Client(string versionHash, ClientScope scope = ClientScope.Global, string launcherBin = null)
    {
        VersionHash = versionHash;
        Scope = scope;
        Location = Path.Combine(Bootstrapper.GetScopeDirectory(scope), "version-" + versionHash);
        Launcher = launcherBin ?? Path.Combine(Location, "RobloxPlayerLauncher.exe");
        Player = Path.Combine(Location, "RobloxPlayerBeta.exe");
    }

    /// <summary>
    ///     Gets a list of the <see cref="Client"/>'s <see cref="Downloadable"/> instances.
    /// </summary>
    /// <exception cref="TaskCanceledException"/>
    public async Task<IList<Downloadable>> GetFilesAsync(CancellationToken token = default)
    {
        var files = new List<Downloadable>();

        var raw = (await Bootstrapper.RbxCdnClient.GetAsync(new RestRequest($"/version-{VersionHash}-rbxPkgManifest.txt"), token));
        var split = raw.Content.Split("\r\n", "\n").Where(x => x != string.Empty).ToArray();
        for (var i = 1; i < split.Length;)
            files.Add(new Downloadable(VersionHash, split[i++], split[i++], long.Parse(split[i++]),
                long.Parse(split[i++])));

        return files;
    }
}