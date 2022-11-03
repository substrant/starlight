namespace Starlight.Bootstrap;

public class InstallConfig
{
    public static InstallConfig Default = new();

    public int DownloadConcurrency = 5;

    public bool RegisterClass = true;

    public bool RegisterClient = true;

    public int UnzipConcurrency = 5;

    public bool CreateStartMenuShortcut = true;

    public bool CreateDesktopShortcut = true;
}