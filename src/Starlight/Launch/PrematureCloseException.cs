using System;
using Starlight.Bootstrap;

namespace Starlight.Launch;

/// <summary>
///     Thrown when a native process used by Starlight prematurely closes.
/// </summary>
public sealed class PrematureCloseException : Exception
{
    public PrematureCloseException(Client client, int? procId)
    {
        Data.Add("VersionHash", client.VersionHash);
        Data.Add("ProcessId", procId);
    }
}