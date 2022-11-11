using System;

namespace Starlight.Bootstrap;

public sealed class ClientNotFoundException : Exception
{
    public ClientNotFoundException(Client client)
    {
        Data.Add("VersionHash", client.VersionHash);
    }
}