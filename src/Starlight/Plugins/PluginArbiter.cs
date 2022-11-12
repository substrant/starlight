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

    public static PluginBase GetPlugin(string name)
    {
        return Plugins.FirstOrDefault(x => x.Name == name);
    }

    public static IEnumerable<PluginBase> GetEnabledPlugins()
    {
        return Plugins.Where(x => x.Enabled);
    }

    internal static void LoadPluginsInAssembly(Assembly asm)
    {
        var asmName = Path.GetFileName(asm.Location);
        //Logger.Out($"Loading plugin assembly '{asmName}'", Level.Trace);

        try
        {
            foreach (var type in asm.GetTypes()
                         .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginBase))))
            {
                //Logger.Out($"Loading plugin '{type.FullName}' in '{asmName}'", Level.Debug);

                var plugin = (PluginBase)Activator.CreateInstance(type);
                plugin.Enabled = true;
            }
        }
        catch (ReflectionTypeLoadException)
        {
            //Logger.Out($"Couldn't get types in plugin assembly '{asmName}'", Level.Warn);
        }
    }

    public static void LoadPlugins()
    {
        if (_loaded)
            //Logger.Out("Plugins are already loaded", Level.Trace);
            return;
        _loaded = true;

        if (!Directory.Exists(Shared.PluginDir))
            //Logger.Out($"Plugin directory at '{Shared.PluginDir}' doesn't exist", Level.Debug);
            Directory.CreateDirectory(Shared.PluginDir);

        //Logger.Out("Enumerating plugins", Level.Trace);
        foreach (var file in Directory.GetFiles(Shared.PluginDir, "*.dll"))
            try
            {
                var asm = Assembly.LoadFile(file);
                LoadPluginsInAssembly(asm);
            }
            catch (BadImageFormatException)
            {
                // Logger.Out($"Plugin '{file}' is not a .NET DLL", Level.Warn);
            }
    }
}