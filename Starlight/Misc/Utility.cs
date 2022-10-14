using IWshRuntimeLibrary;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using static Starlight.Misc.Native;

namespace Starlight.Misc
{
    internal class Utility
    {
        public static string GetTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static void CreateShortcut(string filePath, string target, string workingDir)
        {
            WshShell shell = new(); // This is a nasty library; I wish COM didn't exist.
            var shortcut = (IWshShortcut)shell.CreateShortcut(filePath);
            
            shortcut.TargetPath = target;
            shortcut.WorkingDirectory = workingDir;
            shortcut.Save();
        }

        public static int SecureRandomInteger()
        {
            using RNGCryptoServiceProvider rng = new();
            var seed = new byte[4];
            rng.GetBytes(seed);
            return new Random(BitConverter.ToInt32(seed, 0)).Next();
        }

        public static bool TryGetCultureInfo(string name, out CultureInfo ci)
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

        public static (int, int)? ParseResolution(string res)
        {
            var parts = res.Split('x');
            if (parts.Length != 2)
                return null;

            var b = true;
            b &= int.TryParse(parts[0], out var p1);
            b &= int.TryParse(parts[1], out var p2);
            
            return b ? (p1, p2) : null;
        }

        public static Rectangle GetWindowBounds(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out var nRect))
                return Rectangle.Empty;

            return new Rectangle
            {
                X = nRect.Left,
                Y = nRect.Top,
                Width = nRect.Right - nRect.Left,
                Height = nRect.Bottom - nRect.Top
            };
        }
    }
}
