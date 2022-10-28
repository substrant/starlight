using Newtonsoft.Json;

namespace Starlight.Apis.JoinGame;

public class JoinResponse
{
    [JsonProperty("status")] internal int? InternalStatus;

    [JsonProperty("joinScript")] public JoinScript JoinScript;

    [JsonProperty("joinScriptUrl")] public string JoinScriptUrl;

    [JsonProperty("message")] public string Message;

    [JsonIgnore] public bool Success;

    [JsonIgnore]
    public JoinStatus Status
    {
        get
        {
            return InternalStatus switch
            {
                0 => JoinStatus.Retry,
                1 => JoinStatus.Retry,
                2 => JoinStatus.Success,
                6 => JoinStatus.FullGame,
                10 => JoinStatus.UserLeft,
                _ => JoinStatus.Fail
            };
        }
    }
}