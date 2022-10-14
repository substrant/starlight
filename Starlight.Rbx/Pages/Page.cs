using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Starlight.Rbx.Pages
{
    public class Page<T>
    {
        readonly PageOptions _options;
        
        readonly RestClient _client;
        
        readonly string _path;

        string _previousPageCursor;
        
        string _nextPageCursor;

        JToken _token;

        int _offset;

        public Page(RestClient client, string path, PageOptions opt = null)
        {
            _client = client;
            _path = path;
            _options = opt ?? new PageOptions();
        }

        async Task InternalFetch()
        {
            if (_token is not null) return;

            var req = new RestRequest(_path)
                .AddQueryParameter("limit", _options.Limit)
                .AddQueryParameter("cursor", _options.Cursor);

            foreach (var kv in _options.Other)
                req.AddQueryParameter(kv.Key, kv.Value);

            var res = await _client.ExecuteAsync(req);
            var parsed = JObject.Parse(res.Content);

            _previousPageCursor = parsed?["previousPageCursor"]?.ToString();
            _nextPageCursor = parsed?["nextPageCursor"]?.ToString();

            _token = parsed?["data"];
        }

        public async Task<List<T>> FetchAsync()
        {
            await InternalFetch();
            return _token.Select(item => item.ToObject<T>()).ToList();
        }

        public List<T> Fetch() =>
            FetchAsync().Result;

        public async Task<List<T>> GetNextAsync(int max = 0)
        {
            await InternalFetch();

            int i;
            var data = new List<T>();
            for (i = _offset; i < _token.Count(); i++)
            {
                if (max != 0 && i >= max)
                    break;
                data.Add(_token[i].ToObject<T>());
            }

            _offset = i;

            if (string.IsNullOrEmpty(_nextPageCursor))
                return data;
            
            if (max != 0 && i != max)
                data.AddRange(await (await NextAsync()).GetNextAsync(max - i));
            else if (max == 0)
                data.AddRange(await (await NextAsync()).GetNextAsync());

            return data;
        }
        
        public List<T> GetNext(int max = 0) =>
            GetNextAsync(max).Result;

        public async Task<Page<T>> NextAsync()
        {
            await InternalFetch();
            _options.Cursor = _nextPageCursor;
            return new Page<T>(_client, _path, _options);
        }

        public Page<T> Next() =>
            NextAsync().Result;
        
        public async Task<Page<T>> PreviousAsync()
        {
            await InternalFetch();
            _options.Cursor = _previousPageCursor;
            return new Page<T>(_client, _path, _options);
        }

        public Page<T> Previous() =>
            PreviousAsync().Result;
    }
}
