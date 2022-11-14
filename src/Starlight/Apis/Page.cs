using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.Apis;

/// <summary>
///     <para>Represents a navigatable page collection on Roblox.</para>
///     Example pages: game servers, catalog items, inventory contents, etc.<br/>
///     <strong>Note:</strong> The generic object is deserialized using Json.NET (Newtonsoft.Json).
/// </summary>
/// <typeparam name="T">Any serializable class.</typeparam>
public partial class Page<T> : IDisposable where T : class
{
    readonly int _limit;
    readonly IReadOnlyDictionary<string, string> _extras;
    readonly RestClient _client;
    readonly Uri _resource;

    PageBody _lastFetchedBody;
    string _lastCursor;

    /// <summary>
    ///     The current page number.
    /// </summary>
    public int PageNumber;

    /// <summary>
    ///     <para>Creates a new page collection from <paramref name="resource"/>.</para>
    ///     <strong>Note:</strong> The page limit should be either 10, 25, 50, or 100.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    public Page(Uri resource, int limit = 100, IReadOnlyDictionary<string, string> extras = null)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        if (limit != 10 && limit != 25 && limit != 50 && limit != 100)
            throw new ArgumentException("The page limit must be either 10, 25, 50, or 100.", nameof(limit));

        _client = new RestClient(resource.Host).UseNewtonsoftJson();
        _resource = resource;
        _limit = limit;
        _extras = extras;
    }

    /// <summary>
    ///     <para>Creates a new page collection from <paramref name="resource"/> and authenticate with <paramref name="session"/>.</para>
    ///     <strong>Note:</strong> The page limit should be either 10, 25, 50, or 100.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    public Page(Session session, Uri resource, int limit = 100, IReadOnlyDictionary<string, string> extras = null)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        if (limit != 10 && limit != 25 && limit != 50 && limit != 100)
            throw new ArgumentException("The page limit must be either 10, 25, 50, or 100.", nameof(limit));

        _client = new RestClient(resource.Host).UseNewtonsoftJson()
            .AddCookie(".ROBLOSECURITY", session.AuthToken, "/", ".roblox.com");
        _resource = resource;
        _limit = limit;
        _extras = extras;
    }

    /// <summary>
    ///     <para>Creates a new page collection from <paramref name="resource"/>.</para>
    ///     <strong>Note:</strong> The page limit should be either 10, 25, 50, or 100.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    public Page(string resource, int limit = 100, IReadOnlyDictionary<string, string> extras = null)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentNullException(nameof(resource));

        if (limit != 10 && limit != 25 && limit != 50 && limit != 100)
            throw new ArgumentException("The page limit must be either 10, 25, 50, or 100.", nameof(limit));

        _resource = new Uri(resource);
        _client = new RestClient(_resource.Host).UseNewtonsoftJson();
        _limit = limit;
        _extras = extras;
    }

    /// <summary>
    ///     <para>Creates a new page collection from <paramref name="resource"/> and authenticate with <paramref name="session"/>.</para>
    ///     <strong>Note:</strong> The page limit should be either 10, 25, 50, or 100.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    public Page(Session session, string resource, int limit = 100, IReadOnlyDictionary<string, string> extras = null)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentNullException(nameof(resource));

        if (limit != 10 && limit != 25 && limit != 50 && limit != 100)
            throw new ArgumentException("The page limit must be either 10, 25, 50, or 100.", nameof(limit));

        _resource = new Uri(resource);
        _client = new RestClient(_resource.Host).UseNewtonsoftJson()
            .AddCookie(".ROBLOSECURITY", session.AuthToken, "/", ".roblox.com");
        _limit = limit;
        _extras = extras;
    }

    /// <exception cref="TaskCanceledException"/>
    internal async Task<T[]> InternalFetchAsync(string cursor, CancellationToken token = default)
    {
        var req = new RestRequest(_resource.AbsolutePath)
            .AddQueryParameter("limit", _limit)
            .AddQueryParameter("cursor", cursor);
        if (_extras is not null)
        {
            foreach (var kv in _extras)
                req.AddQueryParameter(kv.Key, kv.Value);
        }
        var res = await _client.ExecuteAsync<PageBody>(req, token);
        
        if (res.Data is null || res.StatusCode != HttpStatusCode.OK)
            return null;

        _lastCursor = cursor;
        _lastFetchedBody = res.Data;
        return res.Data.Data;
    }

    /// <summary>
    ///     Fetch the page contents for the new page number.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="TaskCanceledException"/>
    public async Task<T[]> FetchAsync(int newPageNumber, CancellationToken token = default)
    {
        if (newPageNumber < 0)
            throw new ArgumentException("The page number must be greater than or equal to 0.", nameof(newPageNumber));

        var delta = PageNumber - newPageNumber;
        T[] data = null;

        if (delta != 0)
        {
            var absDelta = Math.Abs(delta);
            var sign = Math.Sign(delta);

            for (var i = 0; i < absDelta; i++)
            {
                data = sign switch
                {
                    -1 => await InternalFetchAsync(_lastFetchedBody?.PreviousPageCursor, token),
                    1 => await InternalFetchAsync(_lastFetchedBody?.NextPageCursor, token),
                    _ => data
                };
            }
        }
        else
        {
            data = await InternalFetchAsync(_lastCursor, token);
        }

        PageNumber = newPageNumber;
        return data;
    }

    /// <summary>
    ///     Fetch the page contents for the next page.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="TaskCanceledException"/>
    public async Task<T[]> FetchNextAsync(CancellationToken token = default)
    {
        PageNumber++;
        return await FetchAsync(PageNumber, token);
    }

    /// <summary>
    ///     Fetch the page contents for the previous page.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="TaskCanceledException"/>
    public async Task<T[]> FetchPreviousAsync(CancellationToken token = default)
    {
        if (PageNumber != 0)
            PageNumber--;
        return await FetchAsync(PageNumber, token);
    }

    /* IDisposable implementation */

    bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
            return;

        _disposed = true;
        _client.Dispose();
    }

    /// <summary>
    ///     Clean up and release the client.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    class PageBody
    {
        [JsonProperty("data")] public T[] Data;
        [JsonProperty("nextPageCursor")] public string NextPageCursor;
        [JsonProperty("previousPageCursor")] public string PreviousPageCursor;
    }
}