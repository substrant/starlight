namespace Starlight.Bootstrap;

/// <summary>
///     Represents an (un)installation configuration.
/// </summary>
public class InstallConfig
{
    /// <summary>
    ///     The default installation configuration.
    /// </summary>
    public static InstallConfig Default = new();

    /// <summary>
    ///     A boolean indicating if a desktop shortcut should be created.
    /// </summary>
    public bool CreateDesktopShortcut = true;

    /// <summary>
    ///     A boolean indicating if a start menu shortcut should be created.
    /// </summary>
    public bool CreateStartMenuShortcut = true;

    /// <summary>
    ///     The max concurrent threads running while downloading.
    /// </summary>
    public int DownloadConcurrency = 3;

    /// <summary>
    ///     A boolean indicating if the scheme should be registered.
    /// </summary>
    public bool RegisterClass = true;

    /// <summary>
    ///     A boolean indicating if the client's environment should be registered.
    /// </summary>
    public bool RegisterClient = true;

    /// <summary>
    ///     The max concurrent threads running while unzipping.
    /// </summary>
    public int UnzipConcurrency = 3;
}