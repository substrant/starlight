using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Starlight.Misc;

namespace Starlight.Plugins;

public class PluginArbiter
{
    static bool _loaded;

    internal static List<PluginBase> Plugins = new();

    internal static IEnumerable<PluginBase> GetEnabledPlugins()
    {
        return Plugins.Where(x => x.Enabled);
    }

    internal static void LoadPluginsInAssembly(Assembly asm)
    {
        try
        {
            foreach (var type in asm.GetTypes()
                         .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginBase))))
            {
                var plugin = (PluginBase)Activator.CreateInstance(type);
                Plugins.Add(plugin);
                plugin.Load();
            }
        }
        catch (ReflectionTypeLoadException)
        {
            // Whoever gets this to throw is stupid
        }
    }

    public static void LoadPlugins()
    {
        if (_loaded) return;
        _loaded = true;

        if (!Directory.Exists(Shared.PluginDir))
            Directory.CreateDirectory(Shared.PluginDir);

        LoadPluginsInAssembly(Assembly.GetExecutingAssembly());
        foreach (var file in Directory.GetFiles(Shared.PluginDir, "*.dll"))
            try
            {
                var asm = Assembly.LoadFile(file);
                LoadPluginsInAssembly(asm);
            }
            catch (BadImageFormatException)
            {
                // Retard probably put a non-managed DLL in there
            }
    }
}