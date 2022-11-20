using Newtonsoft.Json;

namespace Starlight.App.Accounts;

public class Account
{
    [JsonProperty("userId")] public long UserId { get; protected set; }

    [JsonProperty("annotation")] public string Annotation { get; set; }

    [JsonProperty("token")] public string Token { get; set; }
}