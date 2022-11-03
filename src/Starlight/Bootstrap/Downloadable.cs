using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using Starlight.Except;
using Starlight.Misc;

namespace Starlight.Bootstrap;

public class Downloadable
{
    public readonly string VersionHash;

    public readonly string Name;

    public readonly string Checksum;

    public readonly long Size;

    public readonly long TrueSize;

    internal Downloadable(string versionHash, string name, string checksum, long size, long trueSize)
    {
        VersionHash = versionHash;
        Name = name;
        Checksum = checksum;
        TrueSize = trueSize;
        Size = size;
    }

    internal string Download(string dir)
    {
        var filePath = Path.Combine(dir, Name);

        using (var web = new HttpClient()) // Multithreading requires a separate client for each thread.
        {
            web.DownloadFile($"http://setup.rbxcdn.com/version-{VersionHash}-{Name}", filePath);
        }

        if (Validate(filePath))
            return filePath;

        var ex = new BadIntegrityException($"Downloaded file {Name} is corrupt!");
        // TODO: Add logging
        throw ex;
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