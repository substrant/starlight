using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Starlight.Bootstrap;
using Starlight.Plugins;
using Starlight.PostLaunch;

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
            try
            {
                plugin.PreLaunch(info, ref client);
            }
            catch (Exception ex)
            {
                //Logger.Out($"Plugin {plugin.Name} threw an exception during PreLaunch.", Level.Error);
            }

        client ??= Bootstrapper.GetLatestClient();
        if (!client.Exists)
        {
            var ex = new ClientNotFoundException(client);
            throw ex;
        }

        // blah
        var cancelSrc = new CancellationTokenSource();

        // thanks roblox engineers for making my life harder
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = client.Player,
            Arguments = "\"" + Path.GetFullPath(client.Player) + "\" " + info.GetCliParams(),
            WorkingDirectory = client.Location
        });

        if (proc is null)
        {
            var ex = new PrematureCloseException(client, null);
            throw ex;
        }

        // Create an instance
        ClientInstance inst;
        try
        {
            inst = new ClientInstance(client, proc);
        }
        catch
        {
            var ex = new PrematureCloseException(client, proc.Id);
            throw ex;
        }

        var exitEvent = GetNativeEventWaitHandle(inst.Target.Handle);
        Task.Run(() =>
        {
            if (WaitHandle.WaitAny(new[] { exitEvent, cancelSrc.Token.WaitHandle }) == 0)
                cancelSrc.Cancel();
        }, cancelSrc.Token);

        try
        {
            foreach (var plugin in PluginArbiter.GetEnabledPlugins())
                plugin.PostLaunch(inst);
        }
        catch (Exception)
        {
            inst.Proc.Kill();
            throw;
        }

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
            // todo: logs n' pieces o' crap
            throw new PrematureCloseException(client, proc.Id);

        try
        {
            foreach (var plugin in PluginArbiter.GetEnabledPlugins())
                plugin.PostWindow(hWnd);
        }
        catch (Exception)
        {
            inst.Proc.Kill();
            throw;
        }

        return inst;
    }
}