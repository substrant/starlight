using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IWshRuntimeLibrary;

namespace Starlight.Misc;

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
                threadFinishedEvent.WaitOne();
                curConcurrency--;
            }
        }

        while (curConcurrency > 0)
        {
            threadFinishedEvent.WaitOne();
            curConcurrency--;
        }
    }

    public static void DisperseActions<T>(IReadOnlyList<T> list, Action<T> action, int maxConcurrency)
    {
        DisperseActions(list.Select(x => new Action(() => action(x))).ToList(), maxConcurrency);
    }

    public static async Task DisperseActionsAsync<T>(IReadOnlyList<T> list, Action<T> action, int maxConcurrency)
    {
        await Task.Run(() => DisperseActions(list.Select(x => new Action(() => action(x))).ToList(), maxConcurrency));
    }

    public static bool CanShare(string path, FileShare flags)
    {
        try
        {
            using var sr = new FileStream(path, FileMode.Open, FileAccess.Read, flags);
            return sr.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void WaitShare(string path, FileShare flags, CancellationToken ct = default)
    {
        while (!CanShare(path, flags))
        {
            if (ct.IsCancellationRequested)
                return;

            Thread.Sleep(100);
        }
    }
}