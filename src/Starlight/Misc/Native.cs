using System;
using System.Runtime.InteropServices;

namespace Starlight.Misc;

internal class Native
{
    public const uint CreateSuspended = 0x4;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, uint lpProcessAttributes,
        uint lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, uint lpEnvironment,
        string lpCurrentDirectory, [In] ref StartupInfo lpStartupInfo,
        [Out] out ProcInfo lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern int ResumeThread(int hThread);

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public struct StartupInfo
    {
        public uint Cb;
        public string LpReserved;
        public string LpDesktop;
        public string LpTitle;
        public uint DwX;
        public uint DwY;
        public uint DwXSize;
        public uint DwYSize;
        public uint DwXCountChars;
        public uint DwYCountChars;
        public uint DwFillAttribute;
        public uint DwFlags;
        public short WShowWindow;
        public short CbReserved2;
        public IntPtr LpReserved2;
        public IntPtr HStdInput;
        public IntPtr HStdOutput;
        public IntPtr HStdError;
    }

    public struct ProcInfo
    {
        public int HProcess;
        public int HThread;
        public int DwProcessId;
        public int DwThreadId;
    }
}