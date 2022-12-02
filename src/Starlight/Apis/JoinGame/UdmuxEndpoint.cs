using Newtonsoft.Json;

namespace Starlight.Apis.JoinGame;

internal class UdmuxEndpoint {
    [JsonProperty("Address")] public string Address;
    [JsonProperty("Port")] public int? Port;
}