using Starlight.Config;

namespace Starlight.Plugins;

public class PluginApi
{
    readonly PluginBase _plugin;
    readonly Settings _settings = Settings.GetSingleton();

    public PluginApi(PluginBase plugin)
    {
        _plugin = plugin;
    }

    public bool SetValue<T>(string name, T value)
    {
        return _settings.SetEntry(_plugin.Name + "." + name, value);
    }

    public bool GetValue<T>(string name, out T value)
    {
        return _settings.GetEntry(_plugin.Name + "." + name, out value);
    }

    public void SetDefaultValue<T>(string name, T value)
    {
        if (!_settings.GetEntry<T>(_plugin.Name + "." + name, out _))
            SetValue(name, value);
    }

    public void SaveConfig()
    {
        _settings.Save();
    }
}