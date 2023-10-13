using System;
using System.Runtime.InteropServices;

namespace ImgUtil.WPFUI
{
    public static class UIFeature
    {
        private static class NativeMethods
        {
            public const int LOGPIXELSX = 88;

            [DllImport("user32.dll")]
            public extern static bool IsProcessDPIAware();

            [DllImport("user32.dll")]
            public extern static IntPtr GetDC(IntPtr hwnd);

            [DllImport("user32.dll")]
            public extern static int ReleaseDC(IntPtr hwnd, IntPtr hdc);

            [DllImport("gdi32.dll")]
            public extern static int GetDeviceCaps(IntPtr hdc, int index);
        }

        private static readonly int _dpi;

        static UIFeature()
        {
            if (NativeMethods.IsProcessDPIAware())
            {
                IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
                _dpi = NativeMethods.GetDeviceCaps(hdc, NativeMethods.LOGPIXELSX);
                NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
            }
            else
            {
                _dpi = 96;
            }
        }

        public static int DPI
        {
            get
            {
                return _dpi;
            }
        }
    }

    static class DotNetVersion
    {
        private static readonly Version NetVersion;

        static DotNetVersion()
        {
            NetVersion = Environment.Version;
        }

        public static bool DotNet4Version
        {
            get
            {
                return NetVersion.Major >= 4;
            }
        }
    }
}
