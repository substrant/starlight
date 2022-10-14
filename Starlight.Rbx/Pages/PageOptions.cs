using System.Collections.Generic;

namespace Starlight.Rbx.Pages
{
    public class PageOptions
    {
        public int Limit { get; set; } = 100;

        public string Cursor { get; set; }

        internal readonly Dictionary<string, string> Other = new();

        public void Add(string key, object value) =>
            Other[key] = value.ToString();

        public PageOptions() { }

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
    }
}
