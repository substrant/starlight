using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Starlight.Misc.Extensions;
using static Starlight.Misc.Shared;

namespace Starlight.Bootstrap;

public class Client
{
    public readonly string Launcher;

    public readonly string Location;

    public readonly string Player;

    public readonly ClientScope Scope;
    public readonly string VersionHash;

    IReadOnlyList<Downloadable> _files;

    public Client(string versionHash, ClientScope scope = ClientScope.Global, string launcherBin = null)
    {
        VersionHash = versionHash;
        Scope = scope;
        Location = Path.Combine(Bootstrapper.GetScopeDirectory(scope), "version-" + versionHash);
        Launcher = launcherBin ?? Path.Combine(Location, "RobloxPlayerLauncher.exe");
        Player = Path.Combine(Location, "RobloxPlayerBeta.exe");
    }

    public bool Exists => Directory.Exists(Location);

    public async Task<IReadOnlyList<Downloadable>> GetFiles()
    {
        if (_files is not null)
            return _files;

        var files = new List<Downloadable>();

        var raw = await Web.DownloadStringAsync($"http://setup.rbxcdn.com/version-{VersionHash}-rbxPkgManifest.txt");
        var split = raw.Split("\r\n", "\n").Where(x => x != string.Empty).ToArray();
        for (var i = 1; i < split.Length;)
            files.Add(new Downloadable(VersionHash, split[i++], split[i++], long.Parse(split[i++]),
                long.Parse(split[i++])));

        return _files = files;
    }
}