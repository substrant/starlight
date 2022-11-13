using IWshRuntimeLibrary;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.Misc;

internal class Utility
{
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

    public static void DisperseActions(IList<Action> actions, int maxConcurrency)
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

    public static async Task DisperseActionsAsync<T>(IList<T> list, Action<T> action, int maxConcurrency)
    {
        await AsyncHelpers.RunAsync(() => DisperseActions(list.Select(x => new Action(() => action(x))).ToList(), maxConcurrency));
    }

    public static EventWaitHandle GetNativeEventWaitHandle(int handle)
    {
        return new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = new SafeWaitHandle((IntPtr)handle, false)
        };
    }
}