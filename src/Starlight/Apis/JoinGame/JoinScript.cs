using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Starlight.Apis.JoinGame;

public class JoinScript
{
    [JsonProperty("MachineAddress")] internal string Address;
    [JsonProperty("ServerPort")] internal int? Port;
    [JsonProperty("UdmuxEndpoints")] internal UdmuxEndpoint[] UdmuxEndpoints;

    public IPEndPoint GetEndpoint()
    {
        IPAddress ipAddr;
        int port;

        if (UdmuxEndpoints is not null)
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