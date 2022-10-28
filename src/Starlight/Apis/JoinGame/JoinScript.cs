using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Starlight.Apis.JoinGame;

public class JoinScript
{
    // IGNORE INTELLISENSE IT'S LEGALLY BLIND

    [JsonProperty("MachineAddress")] internal string Address;

    [JsonProperty("ServerPort")] internal int? Port;

    [JsonProperty("UdmuxEndpoints")] internal UdmuxEndpoint[] UdmuxEndpoints;

    public IPEndPoint GetEndpoint()
    {
        IPAddress ipAddr;
        int port;

        if
            (UdmuxEndpoints is not null) // No clue what this is but I tested it and it was requesting to the endpoint there. MachineAddress here would be a private IP so all I can think is that it's proxied or just weird like Roblox web devs made it.
        {
            var endpoint = UdmuxEndpoints.FirstOrDefault();
            if (endpoint is null)
                return null;

            IPAddress.TryParse(endpoint.Address, out ipAddr);
            port = endpoint.Port.GetValueOrDefault();
        }
        else
        {
            IPAddress.TryParse(Address, out ipAddr);
            port = Port.GetValueOrDefault();
        }

        return new IPEndPoint(ipAddr, port);
    }
}