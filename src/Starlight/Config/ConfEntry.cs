using System;
using Newtonsoft.Json.Linq;

namespace Starlight.Config;

public class ConfEntry
{
    public string Name;

    public Type Type;

    public object Value;

    public ConfEntry(string name, object value)
    {
        Name = name;
        Value = value;
        Type = value?.GetType();
    }

    public ConfEntry(string name, JToken value)
    {
        Name = name;
        Value = value?.ToObject<object>();
        Type = Value is not null ? Value.GetType() : typeof(object);
    }
}