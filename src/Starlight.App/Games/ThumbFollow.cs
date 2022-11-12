using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Starlight.Apis;
using Starlight.Apis.Pages;

namespace Starlight.App.Games;

public class ThumbFollow
{
    class GameInfo
    {
        [JsonProperty("id")] public Guid? JobId;
        [JsonProperty("playerTokens")] public string[] PlayerTokens;
    }

    class ThumbData
    {
        [JsonProperty("targetId")] public long TargetId;
        [JsonProperty("state")] public string State;
        [JsonProperty("imageUrl")] public string ImageUrl;
    }

    class ThumbResponse
    {
        [JsonProperty("data")] public ThumbData[] Data;
    }

    // takes about 10 seconds to scan, really depends on your internet speed and game server counts
    public static async Task<Guid?> GetServerId(long userId, long placeId)
    {
        using var thumbClient = new RestClient("https://thumbnails.roblox.com/").UseNewtonsoftJson();
        
        var thumb = await thumbClient.GetJsonAsync<ThumbResponse>($"/v1/users/avatar-headshot?userIds={userId}&size=150x150&format=Png&isCircular=false");
        
        if (thumb is null)
            return null;

        var lookFor = thumb.Data[0].ImageUrl;

        var pageClient = new Page<GameInfo>(
            "https://games.roblox.com/",
            $"/v1/games/{placeId}/servers/Public/",
            new PageOptions { Limit = 100 });

        var page = await pageClient.FetchAsync();
        while (pageClient is not null)
        {
            foreach (var info in page)
            {
                var batchReq = new RestRequest("/v1/batch", Method.Post)
                    .AddJsonBody(info.PlayerTokens.Select(token => new
                    {
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
            pageClient = await pageClient.NextAsync();
        }

        return null;
    }
}