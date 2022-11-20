using System;
using System.Drawing;

namespace Starlight.Std;

internal class Utility
{
    public static Resolution? ParseResolution(string res)
    {
        var parts = res.Split('x');
        if (parts.Length != 2)
            return null;

        var b = true;
        b &= int.TryParse(parts[0], out var p1);
        b &= int.TryParse(parts[1], out var p2);

        return b ? new Resolution(p1, p2) : null;
    }

    public static Rectangle GetWindowBounds(IntPtr hWnd)
    {
        if (!Native.GetWindowRect(hWnd, out var rect))
            return Rectangle.Empty;

        return new Rectangle
        {
            X = rect.Left,
            Y = rect.Top,
            Width = rect.Right - rect.Left,
            Height = rect.Bottom - rect.Top
        };
    }

    public struct Resolution
    {
        public int X;

        public int Y;

        public Resolution(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}