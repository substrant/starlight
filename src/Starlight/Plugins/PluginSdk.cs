using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Starlight.Plugins;
#nullable enable

/// <summary>
///     Provides a software development kit for plugins.
/// </summary>
public class PluginSdk
{
    private readonly JObject _config;
    private readonly string _configPath;

    internal PluginSdk(Assembly pluginAssembly)
    {
        if (!File.Exists(pluginAssembly.Location))
            throw new ArgumentException("Invalid plugin assembly.", nameof(pluginAssembly));

        var pluginDir = Path.GetDirectoryName(pluginAssembly.Location)!;
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

    /// <summary>
    ///     A boolean value indicating if the plugin's configuration has unsaved changes.
    /// </summary>
    public bool UnsavedConfig { get; private set; }

    public void SetDefaultValue<T>(string name, T value)
    {
        if (GetValue<T>(name, out _))
            return;

        SetValue(name, value);

        if (!UnsavedConfig)
            UnsavedConfig = true;
    }

    /// <summary>
    ///     Save the unsaved changes to the configuration.
    /// </summary>
    public void SaveConfig()
    {
        File.WriteAllText(_configPath, _config.ToString());
        UnsavedConfig = false;
    }

    /// <summary>
    ///     Merge the configuration with a given JObject.
    /// </summary>
    /// <param name="obj">The object to merge the configuration with.</param>
    public void MergeConfig(JObject obj)
    {
        _config.Merge(obj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
            PropertyNameComparison = StringComparison.Ordinal
        });
    }

    /// <summary>
    ///     Set an entry in the configuration.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public void SetValue<T>(string name, T? value)
    {
        _config[name] = value is null ? JValue.CreateNull() : JToken.FromObject(value);
    }

    /// <summary>
    ///     Retrieve an entry in the configuration.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <returns>A boolean indicating whether or not retrieval succeeded.</returns>
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
}