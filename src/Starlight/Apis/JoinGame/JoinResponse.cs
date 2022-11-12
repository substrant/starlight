using Newtonsoft.Json;

namespace Starlight.Apis.JoinGame;

/// <summary>
///     Represents the server response for a <see cref="JoinRequest" />.
/// </summary>
public class JoinResponse
{
    internal JoinResponse()
    {
    }

    /// <summary>
    ///     Indicates whether the request was successful or not.
    /// </summary>
    [JsonIgnore] public bool Success;

    /// <summary>
    ///     The response status (not HTTP).
    /// </summary>
    [JsonProperty("status")] internal int? InternalStatus;

    /// <summary>
    ///     The status message of the response.
    /// </summary>
    [JsonProperty("message")] public string Message;

    /// <summary>
    ///     The <see cref="JoinScript"/> that the server returned.
    /// </summary>
    [JsonProperty("joinScript")] public JoinScript JoinScript;

    /// <summary>
    ///    The join request URL that the server returned.
    /// </summary>
    [JsonProperty("joinScriptUrl")] public string JoinRequestUrl;

    /// <summary>
    ///     The join status (not HTTP status) of the response.
    /// </summary>
    [JsonIgnore]
    public JoinStatus Status =>
        InternalStatus switch
        {
            0 => JoinStatus.Retry,
            1 => JoinStatus.Retry,
            2 => JoinStatus.Success,
            6 => JoinStatus.FullGame,
            10 => JoinStatus.UserLeft,
            _ => JoinStatus.Fail
        };
}