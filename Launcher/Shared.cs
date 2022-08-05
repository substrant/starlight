/* 
 *  Shared.cs
 *  Author: RealNickk
*/

using System.Net.Http;

namespace Starlight
{
    public static class Output
    {
        public delegate void OutputFunction(string szOutStr, params string[] args);
        internal static OutputFunction WriteOut = null;

        public static void SetOutput(OutputFunction func)
        {
            WriteOut = func;
        }

        public static void WriteLineOut(string szOutStr)
        {
            WriteOut(szOutStr + '\n');
        }
    }

    internal static class Shared
    {
        internal static readonly HttpClient Web = new();
    }
}
