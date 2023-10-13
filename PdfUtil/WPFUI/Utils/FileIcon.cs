﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PdfUtil.WPFUI.Utils
{
    public enum IconSize
    {
        small, large, extraLarge, jumbo, thumbnail
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class FileIcon
    {
        private const string ImageFilter = ".jpg,.jpeg,.png,.gif,.bmp,.dib";
        private const string ExeFilter = ".exe,.lnk";
        private static Dictionary<string, ImageSource> _iconDic = new Dictionary<string, ImageSource>();
        private static Dictionary<string, ImageSource> _thumbDic = new Dictionary<string, ImageSource>();
        private static SysImageList _imgList = new SysImageList(SysImageListSize.jumbo);

        private class ThumbnailInfo
        {
            private IconSize _iconsize;
            private WriteableBitmap _bitmap;
            private string _fullPath;

            public ThumbnailInfo(WriteableBitmap bitmap, string path, IconSize size)
            {
                _bitmap = bitmap;
                _fullPath = path;
                _iconsize = size;
            }

            public IconSize Iconsize
            {
                get
                {
                    return _iconsize;
                }
            }

            public WriteableBitmap Bitmap
            {
                get
                {
                    return _bitmap;
                }
            }

            public string FullPath
            {
                get
                {
                    return _fullPath;
                }
            }
        }
        
        private static Icon GetFileIcon(string fileName, IconSize size)
        {
            NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();

            int flags = NativeMethods.SHGFI_SYSICONINDEX;
            if (fileName.IndexOf(":") == -1)
                flags = flags | NativeMethods.SHGFI_USEFILEATTRIBUTES;
            if (size == IconSize.small)
                flags = flags | NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON;
            else
                flags = flags | NativeMethods.SHGFI_ICON;

            NativeMethods.SHGetFileInfo(fileName, 0, out shinfo, Marshal.SizeOf(shinfo), flags);
            return Icon.FromHandle(shinfo.icon);
        }

        private static void CopyBitmap(BitmapSource source, WriteableBitmap target, bool dispatcher)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * ((source.Format.BitsPerPixel + 7) / 8);

            byte[] bits = new byte[height * stride];
            source.CopyPixels(bits, stride, 0);
            source = null;

            if (dispatcher)
            {
                target.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    WritePixels(target, height, width, stride, bits);
                }));
            }
            else
            {
                WritePixels(target, height, width, stride, bits);
            }
        }

        private static void WritePixels(WriteableBitmap target, int height, int width, int stride, byte[] bits)
        {
            var delta = target.Height - height;
            var newWidth = width > target.Width ? (int)target.Width : width;
            var newHeight = height > target.Height ? (int)target.Height : height;
            Int32Rect outRect = new Int32Rect(0, (int)(delta >= 0 ? delta : 0) / 2, newWidth, newWidth);
            try
            {
                target.WritePixels(outRect, bits, stride, 0);
            }
            catch { }
        }

        private static System.Drawing.Size GetDefaultSize(IconSize size)
        {
            switch (size)
            {
                case IconSize.jumbo: return new System.Drawing.Size(256, 256);
                case IconSize.thumbnail: return new System.Drawing.Size(256, 256);
                case IconSize.extraLarge: return new System.Drawing.Size(48, 48);
                case IconSize.large: return new System.Drawing.Size(32, 32);
                default: return new System.Drawing.Size(16, 16);
            }
        }

        private static Bitmap ResizeImage(Bitmap imgToResize, System.Drawing.Size size, int spacing)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)((sourceWidth * nPercent) - spacing * 4);
            int destHeight = (int)((sourceHeight * nPercent) - spacing * 4);

            int leftOffset = (int)((size.Width - destWidth) / 2);
            int topOffset = (int)((size.Height - destHeight) / 2);


            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.DrawLines(Pens.Silver, new PointF[] {
                new PointF(leftOffset - spacing, topOffset + destHeight + spacing), //BottomLeft
                new PointF(leftOffset - spacing, topOffset - spacing),                 //TopLeft
                new PointF(leftOffset + destWidth + spacing, topOffset - spacing)});//TopRight

            g.DrawLines(System.Drawing.Pens.Gray, new PointF[] {
                new PointF(leftOffset + destWidth + spacing, topOffset - spacing),  //TopRight
                new PointF(leftOffset + destWidth + spacing, topOffset + destHeight + spacing), //BottomRight
                new PointF(leftOffset - spacing, topOffset + destHeight + spacing),}); //BottomLeft

            g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        private static Bitmap ResizeJumbo(Bitmap imgToResize, System.Drawing.Size size, int spacing)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = 80;
            int destHeight = 80;

            int leftOffset = (int)((size.Width - destWidth) / 2);
            int topOffset = (int)((size.Height - destHeight) / 2);


            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.DrawLines(Pens.Silver, new PointF[] {
                new PointF(0 + spacing, size.Height - spacing), //BottomLeft
                new PointF(0 + spacing, 0 + spacing),                 //TopLeft
                new PointF(size.Width - spacing, 0 + spacing)});//TopRight

            g.DrawLines(Pens.Gray, new PointF[] {
                new PointF(size.Width - spacing, 0 + spacing),  //TopRight
                new PointF(size.Width - spacing, size.Height - spacing), //BottomRight
                new PointF(0 + spacing, size.Height - spacing)}); //BottomLeft

            g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        private static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr hBitmap = source.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
        }

        private static bool IsImage(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == "")
                return false;
            return (ImageFilter.IndexOf(ext) != -1 && File.Exists(fileName));
        }

        private static bool IsExecutable(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == "")
                return false;
            return (ExeFilter.IndexOf(ext) != -1 && File.Exists(fileName));
        }

        private static bool IsFolder(string path)
        {
            return path.EndsWith("\\") || Directory.Exists(path);
        }

        private static string ReturnKey(string fileName, IconSize size)
        {
            string key = Path.GetExtension(fileName).ToLower();

            if (IsExecutable(fileName))
                key = fileName.ToLower();
            if (IsImage(fileName) && size == IconSize.thumbnail)
                key = fileName.ToLower();
            if (IsFolder(fileName))
                key = fileName.ToLower();

            switch (size)
            {
                case IconSize.thumbnail: key += IsImage(fileName) ? "+T" : "+J"; break;
                case IconSize.jumbo: key += "+J"; break;
                case IconSize.extraLarge: key += "+XL"; break;
                case IconSize.large: key += "+L"; break;
                case IconSize.small: key += "+S"; break;
            }
            return key;
        }

        private Bitmap LoadJumbo(string lookup)
        {
            _imgList.ImageListSize = IsVistaUp() ? SysImageListSize.jumbo : SysImageListSize.extraLargeIcons;
            Icon icon = _imgList.Icon(_imgList.IconIndex(lookup, IsFolder(lookup)));
            Bitmap bitmap = icon.ToBitmap();
            icon.Dispose();

            System.Drawing.Color empty = System.Drawing.Color.FromArgb(0, 0, 0, 0);

            if (bitmap.Width < 256)
            {
                bitmap = ResizeImage(bitmap, new System.Drawing.Size(256, 256), 0);
            }
            else
            {
                if (bitmap.GetPixel(100, 100) == empty && bitmap.GetPixel(200, 200) == empty && bitmap.GetPixel(248, 248) == empty)
                {
                    _imgList.ImageListSize = SysImageListSize.largeIcons;
                    bitmap = ResizeJumbo(_imgList.Icon(_imgList.IconIndex(lookup)).ToBitmap(), new System.Drawing.Size(200, 200), 5);
                }
            }

            return bitmap;
        }

        private void PollIconCallback(object state)
        {
            ThumbnailInfo input = state as ThumbnailInfo;
            string fileName = input.FullPath;
            WriteableBitmap writeBitmap = input.Bitmap;
            IconSize size = input.Iconsize;

            try
            {
                Bitmap origBitmap = GetFileIcon(fileName, size).ToBitmap();
                Bitmap inputBitmap = origBitmap;
                inputBitmap = (size == IconSize.jumbo || size == IconSize.thumbnail) ? ResizeJumbo(origBitmap, GetDefaultSize(size), 5) : ResizeImage(origBitmap, GetDefaultSize(size), 0);
                BitmapSource inputBitmapSource = LoadBitmap(inputBitmap);
                origBitmap.Dispose();
                inputBitmap.Dispose();

                CopyBitmap(inputBitmapSource, writeBitmap, true);
            }
            catch { }
        }

        private void PollThumbnailCallback(object state)
        {
            ThumbnailInfo input = state as ThumbnailInfo;
            string fileName = input.FullPath;
            WriteableBitmap writeBitmap = input.Bitmap;
            IconSize size = input.Iconsize;

            try
            {
                Bitmap origBitmap = new Bitmap(fileName);
                Bitmap inputBitmap = ResizeImage(origBitmap, GetDefaultSize(size), 5);
                BitmapSource inputBitmapSource = LoadBitmap(inputBitmap);
                origBitmap.Dispose();
                inputBitmap.Dispose();

                CopyBitmap(inputBitmapSource, writeBitmap, true);
            }
            catch { }
        }

        private ImageSource AddToDic(string fileName, IconSize size)
        {
            string key = ReturnKey(fileName, size);

            if (size == IconSize.thumbnail || IsExecutable(fileName))
            {
                if (!_thumbDic.ContainsKey(key))
                {
                    lock (_thumbDic)
                    {
                        _thumbDic.Add(key, GetImage(fileName, size));
                    }
                }
                return _thumbDic[key];
            }
            else
            {
                if (!_iconDic.ContainsKey(key))
                {
                    lock (_iconDic)
                    {
                        _iconDic.Add(key, GetImage(fileName, size));
                    }
                }
                return _iconDic[key];
            }

        }

        public ImageSource GetImage(string fileName, int iconSize)
        {
            IconSize size;

            if (iconSize <= 16) size = IconSize.small;
            else if (iconSize <= 32) size = IconSize.large;
            else if (iconSize <= 48) size = IconSize.extraLarge;
            else if (iconSize <= 72) size = IconSize.jumbo;
            else size = IconSize.thumbnail;

            return AddToDic(fileName, size);
        }

        public static bool IsVistaUp()
        {
            return (Environment.OSVersion.Version.Major >= 6);
        }

        private BitmapSource GetImage(string fileName, IconSize size)
        {
            Icon icon;
            string key = ReturnKey(fileName, size);
            string lookup = Path.GetFileName(fileName);
            if (!key.StartsWith("."))
                lookup = fileName;

            if (IsExecutable(fileName))
            {
                WriteableBitmap bitmap = new WriteableBitmap(AddToDic(Path.GetFileName(fileName), size) as BitmapSource);
                ThreadPool.QueueUserWorkItem(new WaitCallback(PollIconCallback), new ThumbnailInfo(bitmap, fileName, size));
                return bitmap;
            }
            else
            {
                switch (size)
                {
                    case IconSize.thumbnail:
                        if (IsImage(fileName))
                        {
                            WriteableBitmap bitmap = new WriteableBitmap(AddToDic(fileName, IconSize.jumbo) as BitmapSource);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(PollThumbnailCallback), new ThumbnailInfo(bitmap, fileName, size));
                            return bitmap;
                        }
                        else
                        {
                            return GetImage(lookup, IconSize.jumbo);
                        }
                    case IconSize.jumbo:
                        return LoadBitmap(LoadJumbo(lookup));
                    case IconSize.extraLarge:
                        _imgList.ImageListSize = SysImageListSize.extraLargeIcons;
                        icon = _imgList.Icon(_imgList.IconIndex(lookup, IsFolder(fileName)));
                        return LoadBitmap(icon.ToBitmap());
                    default:
                        icon = GetFileIcon(lookup, size);
                        return LoadBitmap(icon.ToBitmap());
                }
            }
        }
    }

    public static class IconHelper
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;

        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            // Change the extended window style to not show a window icon
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE |
                  SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }
}
