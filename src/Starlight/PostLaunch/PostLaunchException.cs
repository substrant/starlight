using System;

namespace Starlight.PostLaunch;

public sealed class PostLaunchException : Exception
{
    public PostLaunchException(ClientInstance inst, string message) : base(message)
    {
        Data.Add("VersionHash", inst.Client.VersionHash);
        Data.Add("ProcessId", inst.Proc.Id);
    }
}