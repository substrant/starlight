using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Starlight.Misc;

namespace Starlight.Plugins;

/// <summary>
///     A class that handles loading and unloading of plugins.
/// </summary>
public class PluginArbiter
{
    static bool _loaded;

    internal static List<PluginBase> Plugins = new();

    /// <summary>
    ///     Get a plugin by its name.
    /// </summary>
    /// <param name="name">The plugin's name.</param>
    /// <returns>The found plugin, or null if not found.</returns>
    public static PluginBase GetPlugin(string name)
    {
        return Plugins.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    ///     Get a list of all loaded plugins.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<PluginBase> GetPlugins()
    {
        return Plugins.AsReadOnly();
    }

    /// <summary>
    ///     Get all enabled plugins.
    /// </summary>
    /// <returns>A list of enabled plugins.</returns>
    public static IEnumerable<PluginBase> GetEnabledPlugins()
    {
        return Plugins.Where(x => x.Enabled);
    }

    /// <summary>
    ///     Load all plugins in <see cref="Shared.PluginDir"/>.
    /// </summary>
    public static void LoadPlugins()
    {
        if (_loaded)
            return;
        _loaded = true;

        if (!Directory.Exists(Shared.PluginDir))
            Directory.CreateDirectory(Shared.PluginDir);
        
        foreach (var file in Directory.GetFiles(Shared.PluginDir, "*.dll"))
            try
            {
                var asm = Assembly.LoadFile(file);
                LoadPluginsInAssembly(asm);
            }
            catch (BadImageFormatException)
            {
            }
    }

    internal static void LoadPluginsInAssembly(Assembly asm)
    {
        try
        {
            foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginBase))))
            {

                var plugin = (PluginBase)Activator.CreateInstance(type);
                plugin.Enabled = true;
            }
        }
        catch (ReflectionTypeLoadException)
        {
        }
    }
}