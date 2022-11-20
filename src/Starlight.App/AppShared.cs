using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Starlight.Bootstrap;

namespace Starlight.App;

public static class AppShared
{
    private static Client _appClient = InitAppClient();

    private static Client InitAppClient()
    {
        try
        {
            var installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roblox");
            return Client.FromLocal(installPath);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static async Task<Client> GetSharedClientAsync(CancellationToken token = default)
    {
        var latestHash = await Bootstrapper.GetLatestVersionHashAsync(true, token);
        var installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roblox");

        if (_appClient?.VersionHash != latestHash)
            _appClient = Client.FromLocal(installPath, latestHash);

        return _appClient;
    }

    public static Client GetSharedClient()
    {
        return AsyncHelpers.RunSync(() => GetSharedClientAsync());
    }
}