using System.Diagnostics;
using System.Net.Http;

using HackerFramework;

namespace Starlight
{
    internal static class Shared
    {
        internal static readonly HttpClient Web = new();

        internal static Process RbxProc;

        internal static Target Rbx;
    }
}