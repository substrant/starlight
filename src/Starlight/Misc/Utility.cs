using IWshRuntimeLibrary;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static Starlight.Misc.Native;

namespace Starlight.Misc
{
    internal class Utility
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                ci = new CultureInfo(name, false);
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

        public static void DisperseActions(IReadOnlyList<Action> actions, int maxConcurrency)
        {
            var curConcurrency = 0;
            var threadFinishedEvent = new AutoResetEvent(false);
            
            foreach (var action in actions)
            {
                var thread = new Thread(() =>
                {
                    action();
                    threadFinishedEvent.Set();
                });
                
                thread.Start();
                curConcurrency++;
                
                while (curConcurrency >= maxConcurrency)
                {
                    Log.Debug("DisperseActions: Waiting for available thread slot...");
                    threadFinishedEvent.WaitOne();
                    curConcurrency--;
                }
            }
            
            while (curConcurrency > 0)
            {
                Log.Debug($"DisperseActions: {curConcurrency} threads left");
                threadFinishedEvent.WaitOne();
                curConcurrency--;
            }
        }
        
        public static async Task DisperseActionsAsync(IReadOnlyList<Action> actions, int maxConcurrency) =>
            await Task.Run(() => DisperseActions(actions, maxConcurrency));
        
        public static void DisperseActions<T>(IReadOnlyList<T> list, Action<T> action, int maxConcurrency) =>
            DisperseActions(list.Select(x => new Action(() => action(x))).ToList(), maxConcurrency);

        public static async Task DisperseActionsAsync<T>(IReadOnlyList<T> list, Action<T> action, int maxConcurrency) =>
            await Task.Run(() => DisperseActions(list, action, maxConcurrency));
    }
}
