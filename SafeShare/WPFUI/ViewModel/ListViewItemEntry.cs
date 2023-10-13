using SafeShare.WPFUI.Utils;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SafeShare.WPFUI.ViewModel
{
    [ObfuscationAttribute(Exclude = true, ApplyToMembers = true)]
    internal class ListViewItemEntry : UIObject
    {
        private string _name;
        private ImageSource _icon;
        private string _type;
        private string _fullPath;

        public ListViewItemEntry(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Notify(nameof(Name));
                }
            }
        }

        public override ImageSource IconImage
        {
            get
            {
                if (_icon == null)
                {
                    LoadViewIcons(FullPath, true);
                }
                return _icon;
            }
            protected set
            {
                _icon = value;
                Notify(nameof(IconImage));
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    _type = value;
                }
            }
        }

        public string FullPath
        {
            get
            {
                return _fullPath;
            }
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                }
            }
        }

        public long Size
        {
            get;
            set;
        }

        private void LoadViewIcons(string fullPath, bool linkOverlay)
        {
            Task.Factory.StartNew(() =>
            {
                NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
                int shfiSize = Marshal.SizeOf(shinfo.GetType());

                int flags = NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON;

                IntPtr retVal = NativeMethods.SHGetFileInfo(fullPath,
                    NativeMethods.FILE_ATTRIBUTE_NORMAL,
                    out shinfo,
                    shfiSize,
                    flags);

                if (retVal != IntPtr.Zero)
                {
                    var icon = System.Drawing.Icon.FromHandle(shinfo.icon).Clone() as System.Drawing.Icon;
                    _icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Notify(nameof(IconImage));
                }
                NativeMethods.DestroyIcon(shinfo.icon);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}