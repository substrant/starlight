using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Starlight.Plugins;

public class PluginSdk
{
    readonly JObject _config;

    readonly string _configPath;
    readonly PluginBase _plugin;

    internal PluginSdk(Assembly pluginAssembly, PluginBase plugin)
    {
        string pluginDir;
        if (!File.Exists(pluginAssembly.Location) ||
            (pluginDir = Path.GetDirectoryName(pluginAssembly.Location)) is null) throw new NotImplementedException();

        _plugin = plugin;

        _configPath = Path.Combine(pluginDir, Path.GetFileNameWithoutExtension(pluginAssembly.Location) + ".json");

        if (File.Exists(_configPath))
        {
            _config = JObject.Parse(File.ReadAllText(_configPath));
        }
        else
        {
            File.WriteAllText(_configPath, "{}");
            _config = new JObject();
        }
    }

    public bool UnsavedConfig { get; private set; }

    public void SetDefaultValue<T>(string name, T value)
    {
        if (GetValue<T>(name, out _))
            return;

        SetValue(name, value);

        if (!UnsavedConfig)
            UnsavedConfig = true;
    }

    public void SaveConfig()
    {
        File.WriteAllText(_configPath, _config.ToString());
        UnsavedConfig = false;
    }

    public void MergeConfig(JObject obj)
    {
        _config.Merge(obj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
            PropertyNameComparison = StringComparison.Ordinal
        });
    }

#nullable enable
    public void SetValue<T>(string name, T? value)
    {
        _config[name] = value is null ? JValue.CreateNull() : JToken.FromObject(value);
    }

    public bool GetValue<T>(string name, out T? value)
    {
        if (_config[name] is { } tokenValue)
            try
            {
                value = tokenValue.ToObject<T>();
                return true;
            }
            catch (InvalidCastException)
            {
            }

        value = default;
        return false;
    }
#nullable disable
}