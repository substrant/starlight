using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Starlight.Misc;
using Starlight.Misc.Extensions;

namespace Starlight.Bootstrap;

public class Downloadable
{
    public readonly string Checksum;

    public readonly string Name;

    public readonly long Size;

    public readonly long TrueSize;
    public readonly string VersionHash;

    internal Downloadable(string versionHash, string name, string checksum, long size, long trueSize)
    {
        VersionHash = versionHash;
        Name = name;
        Checksum = checksum;
        TrueSize = trueSize;
        Size = size;
    }

    internal async Task<string> DownloadAsync(string dir)
    {
        var filePath = Path.Combine(dir, Name);

        using (var web = new HttpClient()) // Multithreading requires a separate client for each thread.
        {
            await web.DownloadFileAsync($"http://setup.rbxcdn.com/version-{VersionHash}-{Name}", filePath);
        }

        if (Validate(filePath))
            return filePath;

        // yuck delete it
        File.Delete(filePath);

        var ex = new BadIntegrityException(this);
        // TODO: Add logging
        throw ex;
    }

    internal string Download(string dir)
    {
        return AsyncHelpers.RunSync(() => DownloadAsync(dir));
    }

    internal bool Validate(string filePath)
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length != Size)
            return false;

        using var stm = File.OpenRead(filePath);
        using var hasher = MD5.Create();
        var hash = BitConverter.ToString(hasher.ComputeHash(stm)).Replace("-", "").ToLower();

        return hash == Checksum;
    }
}