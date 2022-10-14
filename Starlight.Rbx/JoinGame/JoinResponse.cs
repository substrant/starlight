using Newtonsoft.Json;

namespace Starlight.Rbx.JoinGame
{
    public enum JoinStatus
    {
        Fail,     // Request failed/rejected.
        Retry,    // Request acknowledged, either standby for a server to be available or just retry.
        Success,  // Request accepted.
        FullGame, // Request acknowledged, but the game is full.
        UserLeft, // When joining another user: the user left the game before you could join. I dislike this.
    }

    public class JoinResponse
    {
        [JsonIgnore]
        public bool Success;

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
                    _ => JoinStatus.Fail,
                };
            }
        }

        [JsonProperty("status")]
        internal int? InternalStatus;

        [JsonProperty("joinScriptUrl")]
        public string JoinScriptUrl;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("joinScript")]
        public JoinScript JoinScript;
    }
}
