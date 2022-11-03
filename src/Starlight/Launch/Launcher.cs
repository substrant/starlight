using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Starlight.Bootstrap;
using Starlight.Except;
using Starlight.Plugins;
using Starlight.PostLaunch;
using static Starlight.Misc.Native;

namespace Starlight.Launch;

public class Launcher
{
    static EventWaitHandle GetNativeEventWaitHandle(int handle)
    {
        return new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = new SafeWaitHandle((IntPtr)handle, false)
        };
    }

    public static ClientInstance Launch(LaunchParams info, Client client)
    {
        foreach (var plugin in PluginArbiter.GetEnabledPlugins())
            plugin.PreLaunch(info, ref client);

        client ??= Bootstrapper.GetLatestClient();
        if (!client.Exists)
        {
            var ex = new ClientNotFoundException(client.VersionHash);
            throw ex;
        }

        // blah
        var cancelSrc = new CancellationTokenSource();

        if (!OpenRoblox(client.Player, info, out var procInfo))
        {
            var ex = new PrematureCloseException();
            throw ex;
        }
        ResumeThread(procInfo.HThread);

        // Create an instance
        ClientInstance inst;
        try
        {
            inst = new ClientInstance(procInfo.DwProcessId);
        }
        catch
        {
            var ex = new PrematureCloseException();
            throw ex;
        }

        var exitEvent = GetNativeEventWaitHandle(inst.Target.Handle);
        Task.Run(() =>
        {
            if (WaitHandle.WaitAny(new[] { exitEvent, cancelSrc.Token.WaitHandle }) == 0)
                cancelSrc.Cancel();
        }, cancelSrc.Token);

        foreach (var plugin in PluginArbiter.GetEnabledPlugins())
            plugin.PostLaunch(inst);
        
        // Wait for Roblox's window to open
        // todo: better method for this garbage
        var hWnd = IntPtr.Zero;
        var windowTask = Task.Run(() =>
        {
            while ((hWnd = inst.Proc.MainWindowHandle) == IntPtr.Zero
                   && !cancelSrc.IsCancellationRequested)
                Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
        }, cancelSrc.Token);

        // ReSharper disable once MethodSupportsCancellation
        windowTask.Wait();

        if (cancelSrc.IsCancellationRequested)
        {
            // todo: logs n' pieces o' crap
            throw new PrematureCloseException();
        }

        foreach (var plugin in PluginArbiter.GetEnabledPlugins())
            plugin.PostWindow(hWnd);

        return inst;
    }

    internal static bool OpenRoblox(string robloxPath, LaunchParams info, out ProcInfo procInfo)
    {
        var startInfo = new StartupInfo();
        return CreateProcess(
            Path.GetFullPath(robloxPath),
            $"--play -a https://auth.roblox.com/v1/authentication-ticket/redeem -t {info.Ticket} -j {info.Request} -b {info.TrackerId} " +
            $"--launchtime={info.LaunchTime.ToUnixTimeMilliseconds()} --rloc {info.RobloxLocale.Name} --gloc {info.GameLocale.Name}",
            0,
            0,
            false,
            CreateSuspended,
            0,
            null,
            ref startInfo,
            out procInfo);
    }
}