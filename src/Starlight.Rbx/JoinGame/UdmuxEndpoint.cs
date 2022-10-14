using Newtonsoft.Json;

namespace Starlight.Rbx.JoinGame
{
    public class UdmuxEndpoint
    {
        [JsonProperty("Address")]
        public string Address;

        [JsonProperty("Port")]
        public int? Port;
    }
}
