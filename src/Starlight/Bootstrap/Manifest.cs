using System.Collections.Generic;
using System.Linq;
using Starlight.Misc;

namespace Starlight.Bootstrap;

public class Manifest
{
    /// <summary>
    ///     The list of files contained in the manifest.
    /// </summary>
    public readonly IReadOnlyList<Downloadable> Files;

    /// <summary>
    ///     The hash for the installation manifest.
    /// </summary>
    public readonly string Hash;

    internal Manifest(string hash, string raw)
    {
        List<Downloadable> files = new();
        Hash = hash;

        // Parse manifest
        var split = raw.Split("\r\n", "\n").Where(x => x != string.Empty).ToArray();
        for (var i = 1; i < split.Length;)
            files.Add(new Downloadable(this, split[i++], split[i++], long.Parse(split[i++]), long.Parse(split[i++])));

        Files = files;
    }
}