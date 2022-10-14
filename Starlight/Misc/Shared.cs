using System.Net.Http;

namespace Starlight.Misc
{
    internal static class Shared
    {
        internal static readonly HttpClient Web = new();
    }
}