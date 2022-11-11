using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Starlight.Apis.Pages;

public class Page<T>
{
    class PageBody
    {
        [JsonProperty("previousPageCursor")]
        public string PreviousPageCursor;

        [JsonProperty("nextPageCursor")]
        public string NextPageCursor;

        [JsonProperty("data")]
        public T[] Data;
    }

    PageBody _body;

    readonly RestClient _client;

    readonly string _baseUrl;

    readonly string _resource;
    
    readonly PageOptions _options;

    public Page(string baseUrl, string resource, PageOptions options = null)
    {
        _client = new RestClient(baseUrl).UseNewtonsoftJson();
        _baseUrl = baseUrl;
        _resource = resource;
        _options = options;
    }
    
    public async Task<T[]> FetchAsync(bool bypassCache = false)
    {
        if (_body is not null && !bypassCache)
            return _body.Data;

        var req = new RestRequest(_resource)
            .AddQueryParameter("limit", _options.Limit)
            .AddQueryParameter("cursor", _options.Cursor);

        foreach (var kv in _options.Other)
            req.AddQueryParameter(kv.Key, kv.Value);

        var res = await _client.ExecuteAsync<PageBody>(req);

        if (res.Data is null || res.StatusCode != HttpStatusCode.OK)
            return null;

        _body = res.Data;
        return _body.Data;
    }

    public async Task<Page<T>> NextAsync(bool bypassCache = false)
    {
        if (_body.NextPageCursor is null)
            return null;
        
        await FetchAsync(bypassCache);
        return new Page<T>(_baseUrl, _resource, new PageOptions
        {
            Limit = _options.Limit,
            Cursor = _body.NextPageCursor,
            Other = _options.Other
        });
    }
}