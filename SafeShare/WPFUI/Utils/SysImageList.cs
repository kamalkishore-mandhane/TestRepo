using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SafeShare.WPFUI.Utils
{
    public enum SysImageListSize : int
    {
        largeIcons = 0x0,
        smallIcons = 0x1,
        extraLargeIcons = 0x2,
        jumbo = 0x4
    }

    [Flags]
    public enum ImageListDrawItemConstants : int
    {
        ILD_NORMAL = 0x0,
        ILD_TRANSPARENT = 0x1,
        ILD_BLEND25 = 0x2,
        ILD_SELECTED = 0x4,
        ILD_MASK = 0x10,
        ILD_IMAGE = 0x20,
        ILD_ROP = 0x40,
        ILD_PRESERVEALPHA = 0x1000,
        ILD_SCALE = 0x2000,
        ILD_DPISCALE = 0x4000
    }

    [Flags]
    public enum ShellIconStateConstants
    {
        ShellIconStateNormal = 0,
        ShellIconStateLinkOverlay = 0x8000,
        ShellIconStateSelected = 0x10000,
        ShellIconStateOpen = 0x2,
        ShellIconAddOverlays = 0x000000020,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGELISTDRAWPARAMS
    {
        public int cbSize;
        public IntPtr himl;
        public int i;
        public IntPtr hdcDst;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xBitmap;        // x offest from the upperleft of bitmap
        public int yBitmap;        // y offset from the upperleft of bitmap
        public int rgbBk;
        public int rgbFg;
        public int fStyle;
        public int dwRop;
        public int fState;
        public int Frame;
        public int crEffect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        private int left;
        private int top;
        private int right;
        private int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        private int x;
        private int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO
    {
        public IntPtr hbmImage;
        public IntPtr hbmMask;
        public int Unused1;
        public int Unused2;
        public RECT rcImage;
    }

    [ComImportAttribute()]
    [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        [PreserveSig]
        int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

        [PreserveSig]
        int ReplaceIcon(int i, IntPtr hicon, ref int pi);

        [PreserveSig]
        int SetOverlayImage(int iImage, int iOverlay);

        [PreserveSig]
        int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

        [PreserveSig]
        int Draw(ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(int i);

        [PreserveSig]
        int GetIcon(int i, int flags, ref IntPtr picon);

        [PreserveSig]
        int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

        [PreserveSig]
        int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

        [PreserveSig]
        int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int Clone(ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetImageRect(int i, ref RECT prc);

        [PreserveSig]
        int GetIconSize(ref int cx, ref int cy);

        [PreserveSig]
        int SetIconSize(int cx, int cy);

        [PreserveSig]
        int GetImageCount(ref int pi);

        [PreserveSig]
        int SetImageCount(int uNewCount);

        [PreserveSig]
        int SetBkColor(int clrBk, ref int pclr);

        [PreserveSig]
        int GetBkColor(ref int pclr);

        [PreserveSig]
        int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int EndDrag();

        [PreserveSig]
        int DragEnter(IntPtr hwndLock, int x, int y);

        [PreserveSig]
        int DragLeave(IntPtr hwndLock);

        [PreserveSig]
        int DragMove(int x, int y);

        [PreserveSig]
        int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int DragShowNolock(int fShow);

        [PreserveSig]
        int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetItemFlags(int i, ref int dwFlags);

        [PreserveSig]
        int GetOverlayImage(int iOverlay, ref int piIndex);
    };

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class SysImageList : IDisposable
    {
        private const int MAX_PATH = 260;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;
        private IntPtr hIml = IntPtr.Zero;
        private IImageList iImageList = null;
        private SysImageListSize size = SysImageListSize.smallIcons;
        private bool disposed = false;

        public IntPtr Handle
        {
            get
            {
                return this.hIml;
            }
        }

        public SysImageListSize ImageListSize
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                Create();
            }
        }

        public Size Size
        {
            get
            {
                int cx = 0;
                int cy = 0;
                if (iImageList == null)
                {
                    NativeMethods.ImageList_GetIconSize(hIml, ref cx, ref cy);
                }
                else
                {
                    iImageList.GetIconSize(ref cx, ref cy);
                }
                Size sz = new Size(cx, cy);
                return sz;
            }
        }

        public Icon Icon(int index)
        {
            Icon icon = null;

            IntPtr hIcon = IntPtr.Zero;
            if (iImageList == null)
            {
                hIcon = NativeMethods.ImageList_GetIcon(hIml, index, (int)ImageListDrawItemConstants.ILD_TRANSPARENT);
            }
            else
            {
                iImageList.GetIcon(index, (int)ImageListDrawItemConstants.ILD_TRANSPARENT, ref hIcon);
            }

            if (hIcon != IntPtr.Zero)
            {
                icon = System.Drawing.Icon.FromHandle(hIcon);
            }
            return icon;
        }

        public int IconIndex(string fileName)
        {
            return IconIndex(fileName, false);
        }

        public int IconIndex(string fileName, bool forceLoadFromDisk)
        {
            return IconIndex(fileName, forceLoadFromDisk, ShellIconStateConstants.ShellIconStateNormal);
        }

        public int IconIndex(string fileName, bool forceLoadFromDisk, ShellIconStateConstants iconState)
        {
            int flags = NativeMethods.SHGFI_SYSICONINDEX;
            int dwAttr = 0;
            if (size == SysImageListSize.smallIcons)
            {
                flags |= NativeMethods.SHGFI_SMALLICON;
            }

            if (!forceLoadFromDisk)
            {
                flags |= NativeMethods.SHGFI_USEFILEATTRIBUTES;
                dwAttr = FILE_ATTRIBUTE_NORMAL;
            }
            else
            {
                dwAttr = 0;
            }

            NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
            int shfiSize = (int)Marshal.SizeOf(shinfo.GetType());
            IntPtr retVal = NativeMethods.SHGetFileInfo(fileName, dwAttr, out shinfo, shfiSize, flags | (int)iconState);

            NativeMethods.DestroyIcon(shinfo.icon);
            if (retVal == IntPtr.Zero)
            {
                return 0;
            }
            return shinfo.iconIndex;
        }

        private void Create()
        {
            hIml = IntPtr.Zero;
            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            int ret = NativeMethods.SHGetImageList((int)size, ref iidImageList, ref iImageList);
            NativeMethods.SHGetImageListHandle((int)size, ref iidImageList, ref hIml);
        }

        public SysImageList(SysImageListSize size)
        {
            this.size = size;
            Create();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (iImageList != null)
                    {
                        Marshal.ReleaseComObject(iImageList);
                    }
                    iImageList = null;
                }
            }
            disposed = true;
        }

        ~SysImageList()
        {
            Dispose(false);
        }
    }
}