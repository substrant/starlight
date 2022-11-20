using System;
using System.Diagnostics;
using Starlight.Bootstrap;

namespace Starlight.Launch;

/// <summary>
///     Thrown when the Roblox player prematurely closes.
/// </summary>
public sealed class PrematureCloseException : Exception
{
    internal PrematureCloseException(Client client, Process proc) : base("The client prematurely closed.")
    {
        Data.Add("ProcId", proc?.Id);
        Data.Add("Location", client?.Location);
        Data.Add("VersionHash", client?.VersionHash);
    }
}