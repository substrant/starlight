namespace Starlight.Bootstrap;

public class InstallConfig
{
    public static InstallConfig Default = new();

    public bool CreateDesktopShortcut = true;

    public bool CreateStartMenuShortcut = true;

    public int DownloadConcurrency = 3;

    public bool RegisterClass = true;

    public bool RegisterClient = true;

    public int UnzipConcurrency = 3;
}