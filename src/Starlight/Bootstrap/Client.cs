using System.IO;

namespace Starlight.Bootstrap;

public class Client
{
    public readonly string Hash;

    public readonly string Launcher;
    public readonly string Location;

    public readonly string Player;

    internal Client(string path, string hash)
    {
        Location = path;
        Player = Path.Combine(path, "RobloxPlayerBeta.exe");
        Launcher = Path.Combine(path, "RobloxPlayerLauncher.exe");
        Hash = hash;
    }
}