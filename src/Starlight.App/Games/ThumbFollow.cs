using System;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

using Starlight.Apis;

namespace Starlight.App.Games;

public class ThumbFollow {
    public static async Task<Guid?> GetServerId(long userId, long placeId) {
        using var thumbClient = new RestClient("https://thumbnails.roblox.com/").UseNewtonsoftJson();

        var thumb = await thumbClient.GetJsonAsync<ThumbResponse>(
            $"/v1/users/avatar-headshot?userIds={userId}&size=150x150&format=Png&isCircular=false");

        if (thumb is null)
            return null;

        var lookFor = thumb.Data[0].ImageUrl;

        var pageClient = new Page<GameInfo>($"https://games.roblox.com/v1/games/{placeId}/servers/Public/");

        for (var page = await pageClient.FetchNextAsync(); page != null; page = await pageClient.FetchNextAsync())
            foreach (var info in page) {
                var batchReq = new RestRequest("/v1/batch", Method.Post)
                    .AddJsonBody(info.PlayerTokens.Select(token => new {
                        format = "png",
                        requestId = $"0:{token}:AvatarHeadshot:150x150:png:regular",
                        size = "150x150",
                        targetId = 0,
                        token,
                        type = "AvatarHeadShot"
                    }));
                var batchRes = await thumbClient.ExecuteAsync<ThumbResponse>(batchReq);

                if (batchRes.Data?.Data == null)
                    continue;

                if (batchRes.Data.Data.Any(thumbRes => thumbRes.ImageUrl == lookFor))
                    return info.JobId;
            }

        return null;
    }

    class GameInfo {
        [JsonProperty("id")] public Guid? JobId;
        [JsonProperty("playerTokens")] public string[] PlayerTokens;
    }

    class ThumbData {
        [JsonProperty("imageUrl")] public string ImageUrl;
        [JsonProperty("state")] public string State;
        [JsonProperty("targetId")] public long TargetId;
    }

    class ThumbResponse {
        [JsonProperty("data")] public ThumbData[] Data;
    }
}