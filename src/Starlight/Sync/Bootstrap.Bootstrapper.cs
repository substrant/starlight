using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Bootstrap;

public static partial class Bootstrapper
{
    /// <summary>Synchronous wrapper for <see cref="GetLatestVersionHashAsync" />.</summary>
    public static string GetLatestVersionHash(bool bypassCache = false)
    {
        return AsyncHelpers.RunSync(() => GetLatestVersionHashAsync(bypassCache));
    }

    /// <summary>Synchronous wrapper for <see cref="GetLatestClientAsync" />.</summary>
    public static Client GetLatestClient()
    {
        return AsyncHelpers.RunSync(() => GetLatestClientAsync());
    }

    /// <summary>Synchronous wrapper for <see cref="InstallAsync" />.</summary>
    public static void Install(Client client, InstallConfig cfg = null)
    {
        AsyncHelpers.RunSync(() => InstallAsync(client, null, cfg));
    }
}