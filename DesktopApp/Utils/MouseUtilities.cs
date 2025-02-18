using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace DesktopApp.Utils
{
    public class MouseUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public int X;
            public int Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();
            GetCursorPos(ref mouse);
            return relativeTo.PointFromScreen(new Point((double)mouse.X, (double)mouse.Y));
        }
    }
}