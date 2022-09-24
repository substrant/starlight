using System;
using System.Runtime.InteropServices;

namespace Starlight.Cli
{
    internal class Native
    {
#if DEBUG
        [DllImport("kernel32.dll")]
        public static extern bool IsDebuggerPresent();
#endif

        [DllImport("kernel32.dll")]
        public static extern uint GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(uint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern uint FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll")]
        public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, out bool isDebuggerPresent);

        public const int SW_HIDE = 0;
    }
}
