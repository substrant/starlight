using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Misc;
using Starlight.Misc.Profiling;

// ReSharper disable once CheckNamespace
namespace Starlight.Launch;

public static partial class Launcher
{
    /// <summary>Synchronous wrapper for <see cref="LaunchAsync"/>.</summary>
    public static ClientInstance Launch(Client client, LaunchParams info)
    {
        return AsyncHelpers.RunSync(() => LaunchAsync(client, info));
    }
}