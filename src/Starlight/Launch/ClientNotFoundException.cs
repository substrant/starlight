using System;

using Starlight.Bootstrap;

namespace Starlight.Launch;

/// <summary>
///     Thrown when the given client doesn't exist.
/// </summary>
public sealed class ClientNotFoundException : Exception {
    internal ClientNotFoundException(Client client) : base("The specified client doesn't exist.") {
        Data.Add("Location", client?.Location);
        Data.Add("VersionHash", client?.VersionHash);
    }
}