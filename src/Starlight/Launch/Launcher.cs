using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Starlight.Bootstrap;
using Starlight.Misc;
using Starlight.Plugins;

namespace Starlight.Launch;

/// <summary>
///     Contains methods for launching Roblox.
/// </summary>
public static partial class Launcher
{
    [DllImport("kernel32.dll")]
    static extern int CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(int hObject);

    /// <summary>
    ///     Launch a client with the specified parameters.
    /// </summary>
    /// <param name="client">The client to launch.</param>
    /// <param name="info">The parameters to use.</param>
    /// <param name="token">The frame delay in hertz.</param>
    /// <returns>An instance of the client.</returns>
    /// <exception cref="ClientNotFoundException" />
    /// <exception cref="PrematureCloseException" />
    /// <exception cref="TaskCanceledException" />
    /// <exception cref="Exception">Thrown when a plugin throws an exception.</exception>
    public static async Task<ClientInstance> LaunchAsync(Client client, LaunchParams info,
        CancellationToken token = default)
    {
        // Run pre-launch methods
        foreach (var plugin in PluginArbiter.GetEnabledPlugins())
        {
            var newClient = await plugin.PreLaunch(client, info, token);
            if (newClient != null)
                client = newClient;
        }

        if (token.IsCancellationRequested)
            throw new TaskCanceledException();
        
        // Ensure the client exists
        if (!client.Exists)
            throw new ClientNotFoundException(client);

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
                throw new PrematureCloseException(client, null);
        }
        
        // Wait for roblox singleton event to fire (app init)
        var singletonEvent = CreateEvent(IntPtr.Zero, false, false, "ROBLOX_singletonEvent");
        using var singletonWaitHandle = Utility.GetNativeEventWaitHandle(singletonEvent);
        singletonWaitHandle.WaitOne();
        CloseHandle(singletonEvent);
        
        // Get the instance
        ClientInstance inst = null;
        try
        {
            inst = new ClientInstance(client, proc);
        }
        finally
        {
            if (inst is null)
            {
                if (!proc.HasExited)
                    proc.Kill();
                throw new PrematureCloseException(client, proc);
            }
        }

        if (token.IsCancellationRequested)
        {
            if (!proc.HasExited)
                proc.Kill();
            throw new TaskCanceledException();
        }

        var cancelSrc = new CancellationTokenSource();

# pragma warning disable CS4014
        // Add a failsafe if launch fails
        using var exitEvent = Utility.GetNativeEventWaitHandle(inst.Target.Handle);
        Task.Run(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            if (WaitHandle.WaitAny(new[] { exitEvent, cancelSrc.Token.WaitHandle }) == 0)
                cancelSrc.Cancel();
        }, token);
# pragma warning restore CS4014

        // Run post-launch methods
        try
        {
            foreach (var plugin in PluginArbiter.GetEnabledPlugins())
                await plugin.PostLaunch(inst, token);
        }
        catch (Exception)
        {
            proc.Kill();
            throw;
        }

        // Wait for Roblox's window to open
        // TODO: Use a better method for this garbage
        var hWnd = IntPtr.Zero;
        var windowTask = Task.Run(() =>
        {
            while ((hWnd = proc.MainWindowHandle) == IntPtr.Zero
                   && !cancelSrc.IsCancellationRequested)
                Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
        }, token);

        // ReSharper disable once MethodSupportsCancellation
        windowTask.Wait();

        if (token.IsCancellationRequested)
        {
            if (!proc.HasExited)
                proc.Kill();
            throw new TaskCanceledException();
        }

        if (cancelSrc.IsCancellationRequested)
        {
            if (!proc.HasExited)
                proc.Kill();
            throw new PrematureCloseException(client, proc);
        }

        // Run post-window methods
        try
        {
            foreach (var plugin in PluginArbiter.GetEnabledPlugins())
                await plugin.PostWindow(hWnd, token);
        }
        catch (Exception)
        {
            if (!proc.HasExited)
                proc.Kill();
            throw;
        }
        
        return inst;
    }
}