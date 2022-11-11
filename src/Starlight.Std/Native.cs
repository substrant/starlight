using System;
using System.Runtime.InteropServices;

namespace Starlight.Std
{
    internal class Native
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, CmdShow nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, int wParam, IntPtr lParam);

        public enum CmdShow : int
        {
            Hide,
            Show = 5,
            Minimize
        }

        public const int WmSysCommand = 0x112;

        public const int ScMinimize = 0xF020;

        public const uint SwpNoZOrder = 0x0004;

        public const uint SwpNoOwnerZOrder = 0x0200;

        public const int GwlStyle = -16;

        public const uint WsPopupWindow = 0x80880000;
    }
}
