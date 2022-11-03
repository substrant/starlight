using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Starlight.Misc;

namespace Starlight.Config;

public class Settings
{
    static Settings _singleton;

    readonly Dictionary<string, ConfEntry> _root = new();

    Settings()
    {
        if (!File.Exists(Shared.ConfigFile))
        {
            File.WriteAllText(Shared.ConfigFile, "{}");
            return;
        }

        var rawRoot = JObject.Parse(File.ReadAllText(Shared.ConfigFile));
        foreach (var entry in rawRoot)
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (entry.Value?.Type)
            {
                case JTokenType.Boolean:
                case JTokenType.String:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.TimeSpan:
                case JTokenType.Uri:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Null:
                    _root[entry.Key] = new ConfEntry(entry.Key, entry.Value);
                    break;
            }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Settings GetSingleton()
    {
        return _singleton ??= new Settings();
    }

    public bool GetEntry<T>(string name, out T value)
    {
        var exists = _root.TryGetValue(name, out var entry);

        if (!exists || (entry.Type != typeof(object) && entry.Type != typeof(T)))
        {
            value = default;
            return false;
        }

        value = (T)entry.Value;
        return true;
    }

    public bool SetEntry<T>(string name, T value)
    {
        if (_root.TryGetValue(name, out var entry))
        {
            if (entry.Type != typeof(T))
                return false;

            entry.Value = value;
        }
        else
        {
            _root[name] = new ConfEntry(name, value);
        }

        return true;
    }

    public void Save()
    {
        var rawRoot = new JObject();

        foreach (var entry in _root.Values)
            rawRoot.Add(entry.Name, entry.Value is null ? null : JToken.FromObject(entry.Value));

        File.WriteAllText(Shared.ConfigFile, rawRoot.ToString(Formatting.Indented));
    }
}