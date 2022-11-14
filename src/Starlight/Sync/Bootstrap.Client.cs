using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Starlight.Apis.JoinGame;
using Starlight.Misc;
using Starlight.Misc.Profiling;

// ReSharper disable once CheckNamespace
namespace Starlight.Bootstrap;

public partial class Client
{
    /// <summary>Synchronous wrapper for <see cref="GetFilesAsync"/>.</summary>
    public IList<Downloadable> GetFiles()
    {
        return AsyncHelpers.RunSync(() => GetFilesAsync());
    }
}