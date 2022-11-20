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
    ///     The max concurrent threads running while downloading.
    /// </summary>
    public int DownloadConcurrency = 3;

    /// <summary>
    ///     The directory to copy overloads from.
    /// </summary>
    public string OverloadDirectory;

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