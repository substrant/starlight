using System.Collections.Generic;

namespace Starlight.Apis.Pages;

public class PageOptions
{
    internal readonly Dictionary<string, string> Other = new();

    public PageOptions()
    {
    }

    public PageOptions(int limit, string cursor)
    {
        Limit = limit;
        Cursor = cursor;
    }

    public PageOptions(string cursor, int limit)
    {
        Limit = limit;
        Cursor = cursor;
    }

    public int Limit { get; set; } = 100;

    public string Cursor { get; set; }

    public void Add(string key, object value)
    {
        Other[key] = value.ToString();
    }
}