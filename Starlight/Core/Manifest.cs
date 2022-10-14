using Starlight.Misc;
using System.Collections.Generic;
using System.Linq;

namespace Starlight.Core
{
    public class Manifest
    {
        public readonly IReadOnlyList<Downloadable> Files;
        
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
}
