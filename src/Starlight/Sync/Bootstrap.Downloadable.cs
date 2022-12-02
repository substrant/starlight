using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Bootstrap;

public partial class Downloadable {
    /// <summary>Synchronous wrapper for <see cref="DownloadAsync" />.</summary>
    public void Download(string dir) {
        AsyncHelpers.RunSync(() => DownloadAsync(dir));
    }
}