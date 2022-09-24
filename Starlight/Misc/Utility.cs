using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using IWshRuntimeLibrary;

namespace Starlight
{
    internal class Utility
    {
        internal static string GetTempDir()
        {
            string szDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(szDir);
            return szDir;
        }

        internal static void CreateShortcut(string filePath, string target, string workingDir)
        {
            WshShell shell = new(); // nasty library; i wish COM didn't exist
            var shortcut = (IWshShortcut)shell.CreateShortcut(filePath);
            
            shortcut.TargetPath = target;
            shortcut.WorkingDirectory = workingDir;
            shortcut.Save();
        }

        internal static int SecureRandomInteger()
        {
            using RNGCryptoServiceProvider rng = new();
            byte[] seed = new byte[4];
            rng.GetBytes(seed);
            return new Random(BitConverter.ToInt32(seed, 0)).Next();
        }
        
        internal static bool TryGetCultureInfo(string name, out CultureInfo ci)
        {
            try
            {
                ci = new CultureInfo(name);
                return true;
            }
            catch (CultureNotFoundException)
            {
                ci = null;
                return false;
            }
        }
    }
}
