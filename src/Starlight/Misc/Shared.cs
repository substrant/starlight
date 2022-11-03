using System;
using System.IO;
using System.Net.Http;

namespace Starlight.Misc;

public static class Shared
{
    public static string PluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

    public static string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StarlightConfig.json");

    public static readonly HttpClient Web = new();
}