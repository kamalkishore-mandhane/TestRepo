using System;
using System.Runtime.InteropServices;

namespace PdfUtil.WPFUI.Utils
{
    static partial class NativeMethods
    {
        [DllImport("UxTheme.dll")]
        public static extern int BeginPanningFeedback(IntPtr hwnd);

        [DllImport("UxTheme.dll")]
        public static extern int EndPanningFeedback(IntPtr hwnd, bool animateBack);

        [DllImport("UxTheme.dll")]
        public static extern int UpdatePanningFeedback(IntPtr hwnd, long offsetX, long offsetY, bool inInertia);

        public const int WM_GESTURE = 0x0119;
        public const int WM_GESTURENOTIFY = 0x011A;

        public const int GID_ZOOM = 3;
        public const int GID_PAN = 4;

        public const int GC_PAN = 0x00000001;
        public const int GC_PAN_WITH_SINGLE_FINGER_VERTICALLY = 0x00000002;
        public const int GC_PAN_WITH_SINGLE_FINGER_HORIZONTALLY = 0x00000004;
        public const int GC_PAN_WITH_GUTTER = 0x00000008;
        public const int GC_PAN_WITH_INTERTIA = 0x00000010;

        public const int GF_BEGIN = 0x00000001;
        public const int GF_INERTIA = 0x00000002;
        public const int GF_END = 0x00000004;

        private const int WM_TABLET_DEFBASE = 0x02C0;
        public const int WM_TABLET_QUERYSYSTEMGESTURESTATUS = WM_TABLET_DEFBASE + 12;

        public const int TABLET_DISABLE_FLICKS = 0x00010000;

        [StructLayout(LayoutKind.Sequential)]
        public struct GESTURECONFIG
        {
            public int id;
            public int want;
            public int block;
        }

        [DllImport("User32.dll")]
        public static extern bool SetGestureConfig(IntPtr hwnd, int reserved, int id, GESTURECONFIG[] config, int size);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTS
        {
            public short x;
            public short y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GESTUREINFO
        {
            public int size;
            public int flags;
            public int id;
            public IntPtr target;
            public POINTS pt;
            public int instanceID;
            public int sequenceID;
            public long arguments;
            public int extra;
        }

        [DllImport("User32.dll")]
        public static extern bool GetGestureInfo(IntPtr handle, ref GESTUREINFO info);
    }
}
