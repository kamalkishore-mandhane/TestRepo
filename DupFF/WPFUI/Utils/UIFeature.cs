using System;
using System.Runtime.InteropServices;

namespace DupFF.WPFUI.Utils
{
    static class UIFeature
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

        private static readonly Version _version;
        private static readonly int _dpi;

        static UIFeature()
        {
            _version = Environment.OSVersion.Version;

            if (!VistaFeature || NativeMethods.IsProcessDPIAware())
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

        public static bool VistaFeature
        {
            get
            {
                return _version.Major >= 6;
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
}
