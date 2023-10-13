using ImgUtil.Util;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ImgUtil.WPFUI.ViewModel
{
    public class IntegrationViewModel : ObservableObject
    {
        private IntegrationView _integrationView;

        private bool _isDesktopChecked;
        private bool _isStartMenuChecked;

        private bool _isJpgChecked;
        private bool _isJpegChecked;
        private bool _isJfifChecked;
        private bool _isPngChecked;
        private bool _isBmpChecked;
        private bool _isDibChecked;
        private bool _isJp2Checked;
        private bool _isTifChecked;
        private bool _isTiffChecked;
        private bool _isPsdChecked;
        private bool _isWebpChecked;
        private bool _isGifChecked;
        private bool _isSvgChecked;

        private bool _desktopCheckboxVisible;
        private bool _startMenuCheckboxVisible;
        private bool _shortcutPanelVisible;
        private bool _associationPanelVisible;

        private Dictionary<string, bool> _assocDic;

        public IntegrationViewModel(IntegrationView view)
        {
            _integrationView = view;
        }

        [Obfuscation(Exclude = true)]
        public bool IsDesktopChecked
        {
            get
            {
                return _isDesktopChecked;
            }
            set
            {
                if (_isDesktopChecked != value)
                {
                    _isDesktopChecked = value;
                    Notify(nameof(IsDesktopChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsStartMenuChecked
        {
            get
            {
                return _isStartMenuChecked;
            }
            set
            {
                if (_isStartMenuChecked != value)
                {
                    _isStartMenuChecked = value;
                    Notify(nameof(IsStartMenuChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsJpgChecked
        {
            get
            {
                return _isJpgChecked;
            }
            set
            {
                if (_isJpgChecked != value)
                {
                    _isJpgChecked = value;
                    Notify(nameof(IsJpgChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsJpegChecked
        {
            get
            {
                return _isJpegChecked;
            }
            set
            {
                if (_isJpegChecked != value)
                {
                    _isJpegChecked = value;
                    Notify(nameof(IsJpegChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsJfifChecked
        {
            get
            {
                return _isJfifChecked;
            }
            set
            {
                if (_isJfifChecked != value)
                {
                    _isJfifChecked = value;
                    Notify(nameof(IsJfifChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsPngChecked
        {
            get
            {
                return _isPngChecked;
            }
            set
            {
                if (_isPngChecked != value)
                {
                    _isPngChecked = value;
                    Notify(nameof(IsPngChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsBmpChecked
        {
            get
            {
                return _isBmpChecked;
            }
            set
            {
                if (_isBmpChecked != value)
                {
                    _isBmpChecked = value;
                    Notify(nameof(IsBmpChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsDibChecked
        {
            get
            {
                return _isDibChecked;
            }
            set
            {
                if (_isDibChecked != value)
                {
                    _isDibChecked = value;
                    Notify(nameof(IsDibChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsJp2Checked
        {
            get
            {
                return _isJp2Checked;
            }
            set
            {
                if (_isJp2Checked != value)
                {
                    _isJp2Checked = value;
                    Notify(nameof(IsJp2Checked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsTifChecked
        {
            get
            {
                return _isTifChecked;
            }
            set
            {
                if (_isTifChecked != value)
                {
                    _isTifChecked = value;
                    Notify(nameof(IsTifChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsTiffChecked
        {
            get
            {
                return _isTiffChecked;
            }
            set
            {
                if (_isTiffChecked != value)
                {
                    _isTiffChecked = value;
                    Notify(nameof(IsTiffChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsPsdChecked
        {
            get
            {
                return _isPsdChecked;
            }
            set
            {
                if (_isPsdChecked != value)
                {
                    _isPsdChecked = value;
                    Notify(nameof(IsPsdChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsWebpChecked
        {
            get
            {
                return _isWebpChecked;
            }
            set
            {
                if (_isWebpChecked != value)
                {
                    _isWebpChecked = value;
                    Notify(nameof(IsWebpChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsGifChecked
        {
            get
            {
                return _isGifChecked;
            }
            set
            {
                if (_isGifChecked != value)
                {
                    _isGifChecked = value;
                    Notify(nameof(IsGifChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool IsSvgChecked
        {
            get
            {
                return _isSvgChecked;
            }
            set
            {
                if (_isSvgChecked != value)
                {
                    _isSvgChecked = value;
                    Notify(nameof(IsSvgChecked));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool DesktopCheckboxVisible
        {
            get
            {
                return _desktopCheckboxVisible;
            }
            set
            {
                if (_desktopCheckboxVisible != value)
                {
                    _desktopCheckboxVisible = value;
                    Notify(nameof(DesktopCheckboxVisible));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool StartMenuCheckboxVisible
        {
            get
            {
                return _startMenuCheckboxVisible;
            }
            set
            {
                if (_startMenuCheckboxVisible != value)
                {
                    _startMenuCheckboxVisible = value;
                    Notify(nameof(StartMenuCheckboxVisible));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool ShortcutPanelVisible
        {
            get
            {
                return _shortcutPanelVisible;
            }
            set
            {
                if (_shortcutPanelVisible != value)
                {
                    _shortcutPanelVisible = value;
                    Notify(nameof(ShortcutPanelVisible));
                }
            }
        }

        [Obfuscation(Exclude = true)]
        public bool AssociationPanelVisible
        {
            get
            {
                return _associationPanelVisible;
            }
            set
            {
                if (_associationPanelVisible != value)
                {
                    _associationPanelVisible = value;
                    Notify(nameof(AssociationPanelVisible));
                }
            }
        }

        public IntPtr Owner => _integrationView.WindowHandle;

        public void InitIntegration(bool canAddDesktopIcon, bool canAddStartMenu, int addFileAssociation)
        {
            DesktopCheckboxVisible = canAddDesktopIcon;
            StartMenuCheckboxVisible = canAddStartMenu;
            ShortcutPanelVisible = canAddDesktopIcon || canAddStartMenu;
            AssociationPanelVisible = addFileAssociation != 0;

            IsDesktopChecked = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Desktop);
            IsStartMenuChecked = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_PROGRAMS) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Programs);

            IsJpgChecked = CheckAssociation(ImageHelper.JpgExtension);
            IsJpegChecked = CheckAssociation(ImageHelper.JpegExtension);
            IsJfifChecked = CheckAssociation(ImageHelper.JfifExtension);
            IsPngChecked = CheckAssociation(ImageHelper.PngExtension);
            IsBmpChecked = CheckAssociation(ImageHelper.BmpExtension);
            IsDibChecked = CheckAssociation(ImageHelper.DibExtension);
            IsJp2Checked = CheckAssociation(ImageHelper.Jp2Extension);
            IsTifChecked = CheckAssociation(ImageHelper.TifExtension);
            IsTiffChecked = CheckAssociation(ImageHelper.TiffExtension);
            IsPsdChecked = CheckAssociation(ImageHelper.PsdExtension);
            IsWebpChecked = CheckAssociation(ImageHelper.WebpExtension);
            IsGifChecked = CheckAssociation(ImageHelper.GifExtension);
            IsSvgChecked = CheckAssociation(ImageHelper.SvgExtension);
            _assocDic = GenerateAssocDictionary();
        }

        public static bool CheckAssociation(string ext)
        {
            var associateName = AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, ext);
            return !string.IsNullOrEmpty(associateName) && (associateName.ToLower().Equals("imgutil") || associateName.Equals(Properties.Resources.IMAGE_UTILITY_TITLE));
        }

        public bool HasChanges(ref bool needAdmin)
        {
            // check if the shortcut needs to be changed. Only add shortcut need admin rights
            bool change = false;
            bool desktopExist = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Desktop);
            if (desktopExist != _isDesktopChecked)
            {
                // temporarily store the information in current user registry.
                RegeditOperation.SetAdminConfigRegistryStringValue(RegeditOperation.WzAddDesktopIconKey, _isDesktopChecked ? "1" : "0");
                needAdmin = !desktopExist;
                change = true;
            }

            bool startMenuExist = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_PROGRAMS) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Programs);
            if (startMenuExist != _isStartMenuChecked)
            {
                // temporarily store the information in current user registry.
                RegeditOperation.SetAdminConfigRegistryStringValue(RegeditOperation.WzAddStartMenuKey, _isStartMenuChecked ? "1" : "0");
                needAdmin = !startMenuExist;
                change = true;
            }

            var tempDic = GenerateAssocDictionary();
            foreach (var key in tempDic.Keys)
            {
                if (_assocDic[key] != tempDic[key])
                {
                    RegeditOperation.SetAdminConfigRegistryStringValue(key, tempDic[key] ? "1" : "0");
                    needAdmin = true;
                    change = true;
                }
            }

            return change;
        }

        private static string AssocQueryString(NativeMethods.AssocStr association, string extension)
        {
            // get association information
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.None, association, extension, null, null, ref length);
            if (ret != S_FALSE)
            {
                return string.Empty;
            }

            var sb = new StringBuilder((int)length);
            ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.None, association, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                return string.Empty;
            }

            return sb.ToString();
        }

        private bool IsLinkFileExistInCommonDir(int nFolder)
        {
            // check if shortcut exist in public directory
            int size = 260;
            var folderPath = new StringBuilder(size);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, folderPath, nFolder, false);
            var pdfUtilPath = Path.Combine(folderPath.ToString(), Properties.Resources.IMAGE_UTILITY_TITLE + ".lnk");
            return File.Exists(pdfUtilPath);
        }

        private bool IsLinkFileExistInCurrentUser(Environment.SpecialFolder specialFolder)
        {
            // check if shortcut exist in current user's directory
            var specialFolderPath = Environment.GetFolderPath(specialFolder);
            var pdfUtilPath = Path.Combine(specialFolderPath, Properties.Resources.IMAGE_UTILITY_TITLE + ".lnk");
            return File.Exists(pdfUtilPath);
        }

        private Dictionary<string, bool> GenerateAssocDictionary()
        {
            return new Dictionary<string, bool>
            {
                [ImageHelper.JpgExtension] = IsJpgChecked,
                [ImageHelper.JpegExtension] = IsJpegChecked,
                [ImageHelper.JfifExtension] = IsJfifChecked,
                [ImageHelper.PngExtension] = IsPngChecked,
                [ImageHelper.BmpExtension] = IsBmpChecked,
                [ImageHelper.DibExtension] = IsDibChecked,
                [ImageHelper.Jp2Extension] = IsJp2Checked,
                [ImageHelper.TifExtension] = IsTifChecked,
                [ImageHelper.TiffExtension] = IsTiffChecked,
                [ImageHelper.PsdExtension] = IsPsdChecked,
                [ImageHelper.WebpExtension] = IsWebpChecked,
                [ImageHelper.GifExtension] = IsGifChecked,
                [ImageHelper.SvgExtension] = IsSvgChecked
            };
        }

        public static bool IsImgUtilDefault()
        {
            return CheckAssociation(ImageHelper.JpgExtension)
                && CheckAssociation(ImageHelper.JpegExtension)
                && CheckAssociation(ImageHelper.JfifExtension)
                && CheckAssociation(ImageHelper.PngExtension)
                && CheckAssociation(ImageHelper.BmpExtension)
                && CheckAssociation(ImageHelper.DibExtension)
                && CheckAssociation(ImageHelper.Jp2Extension)
                && CheckAssociation(ImageHelper.TifExtension)
                && CheckAssociation(ImageHelper.TiffExtension)
                && CheckAssociation(ImageHelper.PsdExtension)
                && CheckAssociation(ImageHelper.WebpExtension)
                && CheckAssociation(ImageHelper.GifExtension)
                && CheckAssociation(ImageHelper.SvgExtension);
        }
    }
}
