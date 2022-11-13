namespace Starlight.Bootstrap;

/// <summary>
///     The scope (or installation parent directory) of a client.
/// </summary>
public enum ClientScope
{
    /// <summary>
    ///     Installations will be in the %localappdata%\Roblox\Versions folder.
    /// </summary>
    Global,

    /// <summary>
    ///     Installations will be in a folder called "Roblox" inside the current working directory.
    /// </summary>
    Local
}