using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using log4net;
using Starlight.Except;
using Starlight.Misc;

namespace Starlight.Bootstrap;

public class Downloadable
{
    // ReSharper disable once PossibleNullReferenceException
    internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    readonly Manifest _manifest;

    public readonly string Checksum;

    public readonly string Name;

    public readonly long Size;

    public readonly long TrueSize;

    internal Downloadable(Manifest manifest, string name, string checksum, long size, long trueSize)
    {
        _manifest = manifest;
        Name = name;
        Checksum = checksum;
        TrueSize = trueSize;
        Size = size;
    }

    internal string Download(string dir)
    {
        var outPath = Path.Combine(dir, Name);

        using (var web = new HttpClient()) // Multithreading requires a separate client for each thread.
        {
            web.DownloadFile($"http://setup.rbxcdn.com/version-{_manifest.Hash}-{Name}", outPath);
        }

        if (Check(dir))
            return outPath;

        var ex = new BadIntegrityException($"Downloaded file {Name} is corrupt!");
        Log.Fatal($"Download: Corrupt file: {Name}", ex);
        throw ex;
    }

    internal async Task<string> DownloadAsync(string dir)
    {
        return await Task.Run(() => Download(dir));
    }

    internal bool Check(string dir)
    {
        var outPath = Path.Combine(dir, Name);
        if (!File.Exists(outPath))
            return false;

        if (new FileInfo(outPath).Length != Size)
            return false;

        using var stm = File.OpenRead(outPath);
        using var hasher = MD5.Create();
        var hash = BitConverter.ToString(hasher.ComputeHash(stm)).Replace("-", "").ToLower();

        return hash == Checksum;
    }
}