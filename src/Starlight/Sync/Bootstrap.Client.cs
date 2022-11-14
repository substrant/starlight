using System.Collections.Generic;
using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Bootstrap;

public partial class Client
{
    /// <summary>Synchronous wrapper for <see cref="GetFilesAsync" />.</summary>
    public IList<Downloadable> GetFiles()
    {
        return AsyncHelpers.RunSync(() => GetFilesAsync());
    }
}