using System;
using System.IO;
using System.Net.Http;

namespace Starlight.Misc;

/// <summary>
///     Shared objects for the Starlight library.
/// </summary>
public static class Shared {
    /// <summary>
    ///     The path the the plugin directory.
    /// </summary>
    public static string PluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

    internal static readonly HttpClient Web = new();
}