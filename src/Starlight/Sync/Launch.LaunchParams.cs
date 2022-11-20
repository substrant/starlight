using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Launch;

public partial class LaunchParams
{
    /// <summary>Synchronous wrapper for <see cref="GetCliParamsAsync" />.</summary>
    public string GetCliParams()
    {
        return AsyncHelpers.RunSync(GetCliParamsAsync);
    }

    /// <summary>Synchronous wrapper for <see cref="GetLaunchUriAsync" />.</summary>
    public string GetLaunchUri()
    {
        return AsyncHelpers.RunSync(GetLaunchUriAsync);
    }
}