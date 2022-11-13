using Starlight.Bootstrap;
using Starlight.Misc;
using Starlight.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.Launch;

public static class Launcher
{
    /// <summary>
    /// Launch a client with the specified parameters.
    /// </summary>
    /// <param name="client">The client to launch.</param>
    /// <param name="info">The parameters to use.</param>
    /// <returns>An instance of the client.</returns>
    public static async Task<ClientInstance> LaunchAsync(Client client, LaunchParams info)
    {
        // Run pre-launch methods
        foreach (var plugin in PluginArbiter.GetEnabledPlugins())
            plugin.PreLaunch(info, ref client);

        // Ensure the client exists
        if (!client.Exists)
            throw new NotImplementedException();
        
        // Start Roblox
        Process proc = null;
        try
        {
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = client.Player,
                Arguments = "\"" + Path.GetFullPath(client.Player) + "\" " + await info.GetCliParamsAsync(),
                WorkingDirectory = client.Location
            });
        }
        finally
        {
            if (proc is null)
                throw new NotImplementedException();
        }

        // Get the instance
        ClientInstance inst = null;
        try
        {
            inst = new ClientInstance(client, proc);
        }
        finally
        {
            if (inst is null)
                throw new NotImplementedException();
        }

        var cancelSrc = new CancellationTokenSource();

#   pragma warning disable CS4014
        // Add a failsafe if launch fails
        var exitEvent = Utility.GetNativeEventWaitHandle(inst.Target.Handle);
        Task.Run(() =>
        {
            if (WaitHandle.WaitAny(new[] { exitEvent, cancelSrc.Token.WaitHandle }) == 0)
                cancelSrc.Cancel();
        }, cancelSrc.Token);
#   pragma warning restore CS4014

        // Run post-launch methods
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
        // TODO: Use a better method for this garbage
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
            throw new NotImplementedException();

        // Run post-window methods
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