using System;
using Starlight.Bootstrap;

namespace Starlight.Except;

/// <summary>
///     Thrown when <see cref="Bootstrapper.QueryClient" /> is unable to find a client with the specified hash.
/// </summary>
public sealed class ClientNotFoundException : Exception
{
    internal ClientNotFoundException(string versionHash)
    {
        Data.Add("VersionHash", versionHash);
    }
}