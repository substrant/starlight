using System;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Starlight.Apis.JoinGame;

/// <summary>
///     Represents a request to join a Roblox game.
/// </summary>
public class JoinRequest
{
    /// <summary>
    ///     <para>The game's access code.</para>
    ///     <strong>Note:</strong> Applies only when joining a reserved or private server.
    /// </summary>
    [JsonProperty("accessCode")] public Guid? AccessCode;

    /// <summary>
    ///     The tracker ID used to trace the join request to a browser.
    /// </summary>
    [JsonProperty("browserTrackerId")] public long? BrowserTrackerId;

    /// <summary>
    ///     Determines if the joining user is joining another user.
    /// </summary>
    [JsonProperty("isPlayTogetherGame")] public bool? IsPlayTogetherGame;

    /// <summary>
    ///     Determines if the join is through a teleport.
    /// </summary>
    [JsonProperty("isTeleport")] public bool? IsTeleport;

    /// <summary>
    ///     The server instance ID (Job ID) to join.
    /// </summary>
    [JsonProperty("gameId")] public Guid? JobId;

    /// <summary>
    ///     <para>The game's link code.</para>
    ///     <strong>Note:</strong> Applies only when joining a reserved or private server.
    /// </summary>
    [JsonProperty("linkCode")] public string LinkCode;

    /// <summary>
    ///     The place ID to join.
    /// </summary>
    [JsonProperty("placeId")] public long? PlaceId;

    /// <summary>
    ///     The type of join request to make.
    /// </summary>
    [JsonIgnore] public JoinType ReqType = JoinType.Auto;

    /// <summary>
    ///     Instantiate a join request.
    /// </summary>
    public JoinRequest()
    {
    }

    [JsonIgnore]
    internal string Endpoint
    {
        get
        {
            return ReqType switch
            {
                JoinType.Auto => "v1/join-game",
                JoinType.Specific => "/v1/join-game-instance",
                JoinType.Private => "/v1/join-private-game",
                _ => null
            };
        }
    }

    [JsonProperty("request")]
    internal string RequestName
    {
        get
        {
            return ReqType switch
            {
                JoinType.Auto => "RequestGame",
                JoinType.Specific => "RequestGame",
                JoinType.Private => "RequestPrivateGame",
                _ => null
            };
        }
    }

    /// <summary>
    ///     Instantiate a join request from <paramref name="launchUri" />.
    /// </summary>
    public static JoinRequest FromUri(Uri launchUri)
    {
        var query = HttpUtility.ParseQueryString(launchUri.Query);
        return JObject.FromObject(query.Cast<string>().ToDictionary(k => k, v => query[v])).ToObject<JoinRequest>();
    }

    /// <summary>
    ///     Serialize the current object into a launch URI.
    /// </summary>
    public override string ToString()
    {
        var query = new StringBuilder();
        foreach (var pair in JObject.FromObject(this))
        {
            if (pair.Value?.Type == JTokenType.Null)
                continue;

            query.Append($"{pair.Key}={HttpUtility.UrlEncode(pair.Value?.ToString())}&");
        }

        var str = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?" + query;
        return str.Substring(0, str.Length - 1);
    }
}