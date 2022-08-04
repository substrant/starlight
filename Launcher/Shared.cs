/* 
 *  Shared.cs
 *  Author: RealNickk
*/

using System.Net.Http;

namespace Starlight
{
    internal static class Shared
    {
        internal static readonly HttpClient Web = new();
    }
}
