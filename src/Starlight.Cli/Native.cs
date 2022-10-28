using System.Runtime.InteropServices;

namespace Starlight.Cli;

internal class Native
{
#if DEBUG
        [DllImport("kernel32.dll")]
        public static extern bool IsDebuggerPresent();
#endif

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(int hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern int FindWindow(string lpClassName, string lpWindowName);

    [DllImport("kernel32.dll")]
    public static extern int GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

    public const uint STD_OUTPUT_HANDLE = 0xFFFFFFF5;

    [DllImport("kernel32.dll")]
    public static extern uint GetStdHandle(uint nStdHandle);

    [DllImport("kernel32.dll")]
    public static extern void SetStdHandle(uint nStdHandle, uint handle);

    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();

    public const int SW_HIDE = 0;
}