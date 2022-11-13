namespace Starlight.Bootstrap;

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