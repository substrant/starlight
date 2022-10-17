using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Starlight.Rbx.JoinGame
{
    public class JoinRequest
    {
        // This file is very cancerous. Proceed with caution.

        [JsonIgnore]
        public JoinType? ReqType = JoinType.Auto;

        [JsonIgnore]
        internal string Endpoint
        {
            get
            {
                return ReqType switch // gamejoin.roblox.com
                {
                    JoinType.Auto => "v1/join-game",
                    JoinType.Specific => "/v1/join-game-instance",
                    JoinType.Private => "/v1/join-private-game",
                    _ => null
                };
            }
        }

        [JsonIgnore]
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

        [JsonProperty("gameId")]
        public Guid? JobId
        {
            get
            {
                if (Options.TryGetValue("gameId", out var gameId) && Guid.TryParse(gameId, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("gameId");
                else
                    Options["gameId"] = value.ToString();
            }
        }

        [JsonProperty("isTeleport")]
        public bool? IsTeleport
        {
            get
            {
                if (Options.TryGetValue("isTeleport", out var isTeleport) && bool.TryParse(isTeleport, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("isTeleport");
                else
                    Options["isTeleport"] = value.ToString();
            }
        }

        [JsonProperty("placeId")]
        public long? PlaceId
        {
            get
            {
                if (Options.TryGetValue("placeId", out var placeId) && long.TryParse(placeId, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("placeId");
                else
                    Options["placeId"] = value.ToString();
            }
        }

        [JsonProperty("accessCode")]
        public Guid? AccessCode
        {
            get
            {
                if (Options.TryGetValue("accessCode", out var accessCode) && Guid.TryParse(accessCode, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("accessCode");
                else
                    Options["accessCode"] = value.ToString();
            }
        }

        [JsonProperty("linkCode")]
        public string LinkCode
        {
            get
            {
                Options.TryGetValue("linkCode", out var x);
                return x;
            }
            set
            {
                if (value is null)
                    Options.Remove("linkCode");
                else
                    Options["linkCode"] = value;
            }
        }

        [JsonProperty("isPlayTogetherGame")]
        public bool? IsPlayTogetherGame
        {
            get
            {
                if (Options.TryGetValue("isPlayTogetherGame", out var playTogether) && bool.TryParse(playTogether, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("isPlayTogetherGame");
                else
                    Options["isPlayTogetherGame"] = value.ToString();
            }
        }

        [JsonProperty("browserTrackerId")]
        public long? BrowserTrackerId
        {
            get
            {
                if (Options.TryGetValue("browserTrackerId", out var tracker) && long.TryParse(tracker, out var x))
                    return x;
                return null;
            }
            set
            {
                if (value is null)
                    Options.Remove("browserTrackerId");
                else
                    Options["browserTrackerId"] = value.ToString();
            }
        }

        [JsonIgnore]
        internal Dictionary<string, string> Options;

        public async Task<JoinResponse> ExecuteAsync(Session session, int maxTries = 0)
        {
            var tries = -1;
            for (;;)
            {
                var reqBody = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var req = new RestRequest(Endpoint, Method.Post).AddHeader("Content-Type", "application/json")
                    .AddHeader("User-Agent", "Roblox/WinInet") // pov you use undocumented endpoints
                    .AddBody(reqBody);

                var res = await session.GameClient.ExecuteAsync(req); // response body should look like this: https://i.imgur.com/GZChdWo.png
                var body = JsonConvert.DeserializeObject<JoinResponse>(res.Content);

                if (body.Status == JoinStatus.Retry && tries != maxTries)
                {
                    await Task.Delay(2000); // Wait a bit.
                    tries++;
                    continue;
                }

                if (res.StatusCode == HttpStatusCode.OK && body.Status != JoinStatus.Fail) body.Success = true;

                return body;
            }
        }

        public JoinResponse Execute(Session session, int maxTries = 0) =>
            ExecuteAsync(session, maxTries).Result;

        public string Serialize()
        {
            const string urlTemplate = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?{0}";
            return string.Format(urlTemplate, string.Join("&", Options.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value.ToString())}")));
        }
        
        public JoinRequest()
        {
            Options = new Dictionary<string, string>();
        }

        public JoinRequest(Uri launchUri)
        {
            var query = HttpUtility.ParseQueryString(launchUri.Query);
            Options = query.Cast<string>().ToDictionary(k => k, k => query[k]);
        }
    }
}
