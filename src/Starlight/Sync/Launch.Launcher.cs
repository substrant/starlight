using Starlight.Bootstrap;
using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Launch;

public static partial class Launcher
{
    /// <summary>Synchronous wrapper for <see cref="LaunchAsync" />.</summary>
    public static ClientInstance Launch(Client client, LaunchParams info)
    {
        return AsyncHelpers.RunSync(() => LaunchAsync(client, info));
    }
}