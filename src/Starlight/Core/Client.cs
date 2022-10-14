namespace Starlight.Core
{
    public class Client
    {
        public readonly string Directory;

        public readonly string Path;

        public readonly string LauncherPath;

        public readonly string Hash;

        internal Client(string path, string hash)
        {
            Directory = path;
            Path = System.IO.Path.Combine(path, "RobloxPlayerBeta.exe");
            LauncherPath = System.IO.Path.Combine(path, "RobloxPlayerLauncher.exe");
            Hash = hash;
        }
    }
}
