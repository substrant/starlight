/* 
 *  FileSystem.cs
 *  Author: RealNickk
*/

using System.IO;

namespace Starlight
{
    internal class FileSystem
    {
        public static string GetTempDir()
        {
            string szDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(szDir);
            return szDir;
        }
    }
}
