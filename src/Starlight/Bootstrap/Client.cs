using Newtonsoft.Json.Linq;
using Starlight.Misc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Starlight.Misc.Shared;

namespace Starlight.Bootstrap;

public class Client
{
    public readonly string VersionHash;

    public readonly ClientScope Scope;

    public readonly string Location;

    public bool Exists => Directory.Exists(Location);

    public readonly string Launcher;

    public readonly string Player;

    IReadOnlyList<Downloadable> _files;

    public IReadOnlyList<Downloadable> GetFiles()
    {
        if (_files is not null)
            return _files;

        var files = new List<Downloadable>();

        string raw;
        lock (Web)
        {
            raw = Web.DownloadString($"http://setup.rbxcdn.com/version-{VersionHash}-rbxPkgManifest.txt");
        }

        var split = raw.Split("\r\n", "\n").Where(x => x != string.Empty).ToArray();
        for (var i = 1; i < split.Length;)
            files.Add(new Downloadable(VersionHash, split[i++], split[i++], long.Parse(split[i++]), long.Parse(split[i++])));

        return _files = files;
    }

    public Client(string versionHash, ClientScope scope = ClientScope.Global, string launcherBin = null)
    {
        VersionHash = versionHash;
        Scope = scope;
        Location = Path.Combine(Bootstrapper.GetScopeDirectory(scope), "version-" + versionHash);
        Launcher = launcherBin ?? Path.Combine(Location, "RobloxPlayerLauncher.exe");
        Player = Path.Combine(Location, "RobloxPlayerBeta.exe");
    }
}