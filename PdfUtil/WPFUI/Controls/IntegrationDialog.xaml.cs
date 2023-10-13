using Microsoft.Win32;
using PdfUtil.Util;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommonDialogResult = System.Windows.Forms.DialogResult;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for IntegrationDialog.xaml
    /// </summary>

    public enum FileAssociateChoice
    {
        CURRENT_SETTING,
        PDF_EXPRESS,
        NO_APPLICATION
    }

    public partial class IntegrationDialog : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _canAddDesktopIcon;
        private bool _canAddStartMenu;
        private int _addFileAssociation;

        private FileAssociateChoice _fileAssocChoice;
        private bool _isDesktopChecked;
        private bool _isStartMenuChecked;
        private bool _isPdfExpressChecked;
        private bool _isPdfAssoc;

        private bool _isFirstTime;

        private IntPtr _windowHandle;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                    OnPropertyChanged(nameof(IsDesktopChecked));
                }
            }
        }

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
                    OnPropertyChanged(nameof(IsStartMenuChecked));
                }
            }
        }

        public bool IsPdfExpressChecked
        {
            get
            {
                return _isPdfExpressChecked;
            }
            set
            {
                if (_isPdfExpressChecked != value)
                {
                    _isPdfExpressChecked = value;
                    OnPropertyChanged(nameof(IsPdfExpressChecked));
                }
            }
        }

        public FileAssociateChoice FileAssocChoice
        {
            get
            {
                return _fileAssocChoice;
            }
            set
            {
                if (_fileAssocChoice != value)
                {
                    _fileAssocChoice = value;
                    OnPropertyChanged(nameof(FileAssocChoice));
                }
            }
        }

        public CommonDialogResult Result { get; set; }

        public IntegrationDialog(PdfUtilView view, bool canAddDesktopIcon, bool canAddStartMenu, int addFileAssociation, IntPtr windowHandle, bool isFirstTime)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _canAddDesktopIcon = canAddDesktopIcon;
            _canAddStartMenu = canAddStartMenu;
            _addFileAssociation = addFileAssociation;
            _fileAssocChoice = FileAssociateChoice.CURRENT_SETTING;

            _isFirstTime = isFirstTime;
            _windowHandle = windowHandle;
        }

        private void IntegrationDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            // if AddDesktopIcon MSI property is set to 1, show corresponding controls, otherwise hide
            if (_canAddDesktopIcon)
            {
                cbDesktop.Visibility = Visibility.Visible;
                IsDesktopChecked = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Desktop);
            }
            else
            {
                cbDesktop.Visibility = Visibility.Collapsed;
            }

            // if AddStartMenu MSI property is set to 1, show corresponding controls, otherwise hide
            if (_canAddStartMenu)
            {
                cbStartMenu.Visibility = Visibility.Visible;
                IsStartMenuChecked = IsLinkFileExistInCommonDir(NativeMethods.CSIDL_COMMON_PROGRAMS) || IsLinkFileExistInCurrentUser(Environment.SpecialFolder.Programs);
            }
            else
            {
                cbStartMenu.Visibility = Visibility.Collapsed;
            }

            // if both AddDesktopIcon and AddStartMenu MSI property are set to 0, hide shortcut StackPanel
            shortcutStackPanel.Visibility = (_canAddDesktopIcon || _canAddStartMenu) ? Visibility.Visible : Visibility.Collapsed;

            // if AddFileAssociation MSI property is set to 1 or 2, show association StackPanel, otherwise hide
            if (_addFileAssociation != 0)
            {
                associationStackPanel.Visibility =Visibility.Visible;
                var associateName = NativeMethods.AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, PdfHelper.PdfExtension);
                IsPdfExpressChecked = !string.IsNullOrEmpty(associateName) && (associateName.ToLower().Equals("pdfutil") || associateName.Equals(Properties.Resources.PDF_UTILITY_TITLE));
                _isPdfAssoc = IsPdfExpressChecked;
                FileAssocChoice = FileAssociateChoice.CURRENT_SETTING;
            }
            else
            {
                associationStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void IntegrationDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void IntegrationDialog_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Result = CommonDialogResult.OK;
                    Close();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    Result = CommonDialogResult.Cancel;
                    Close();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        public new CommonDialogResult Show()
        {
            BaseShowWindow();
            return Result;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = CommonDialogResult.OK;
            Close();
        }

        private void PdfExpressCheckbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            var tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);

            try
            {
                var handlerFile = Path.Combine(tempFolder, Properties.Resources.FILE_ASSOC_HANDLER_FILE_NAME + ".pdf");
                // only create file, we don't need the filestream, so close it.
                File.Create(handlerFile).Close();

                const string defaultCallBackUrl = "https://www.winzip.com/cloudservices.htm";
                const string title = "WinZip File Association";
                const string htmlFileName = "WinZipFileAssociation.html";
                var htmlPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), htmlFileName);
                var extraData = new WebAuthBroker.WebAuthExtraData();
                extraData.dataType = WebAuthBroker.WebAuthExtraDataType.FileAssociation;
                var dataString = Properties.Resources.FILE_ASSOC_HTML_TITLE + '\0';
                dataString += Properties.Resources.FILE_ASSOC_HTML_CLICK_TIP + '\0';
                dataString += Properties.Resources.FILE_ASSOC_HTML_CHOOSE_TIP + '\0';
                dataString += System.Globalization.CultureInfo.CurrentCulture.Name + "\\WinZip.gif\0";
                dataString += '\"' + handlerFile + '\"' + '\0';
                dataString += Properties.Resources.FILE_ASSOC_HTML_REDISPLAY_TEXT + '\0';
                dataString += ".pdf\0";
                dataString += "WinZip.PdfExpress\0";
                extraData.dataStringLength = dataString.Length;
                extraData.data = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(dataString);
                WebAuthBroker.BlockUIAndShowAuth(_windowHandle, IntPtr.Zero, title, 800, 600, new Uri(htmlPath).AbsoluteUri, new Uri(defaultCallBackUrl).AbsoluteUri, false, false, extraData);
            }
            // ignore exceptions
            catch
            {
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }

            var associateName = NativeMethods.AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, ".pdf");
            IsPdfExpressChecked = !string.IsNullOrEmpty(associateName) && (associateName.ToLower().Equals("pdfutil") || associateName.Equals(Properties.Resources.PDF_UTILITY_TITLE));
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

            if (_isPdfAssoc != IsPdfExpressChecked)
            {
                // temporarily store the information in current user registry.
                RegeditOperation.SetAdminConfigRegistryStringValue(PdfHelper.PdfExtension, IsPdfExpressChecked ? "1" : "0");
                needAdmin = true;
                change = true;
            }

            return change;
        }

        public bool HasAssociationChanges()
        {
            // check if the association needs to be changed
            if (_fileAssocChoice != FileAssociateChoice.CURRENT_SETTING)
            {
                var associateName = NativeMethods.AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, PdfHelper.PdfExtension);
                return !(_fileAssocChoice == FileAssociateChoice.PDF_EXPRESS && associateName.ToLower().Equals("pdfutil")
                    || _fileAssocChoice == FileAssociateChoice.NO_APPLICATION && string.IsNullOrEmpty(associateName));
            }
            return false;
        }

        private bool IsLinkFileExistInCommonDir(int nFolder)
        {
            // check if shortcut exist in public directory
            int size = 260;
            StringBuilder folderPath = new StringBuilder(size);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, folderPath, nFolder, false);
            var pdfUtilPath = Path.Combine(folderPath.ToString(), Properties.Resources.PDF_UTILITY_TITLE + ".lnk");
            return File.Exists(pdfUtilPath);
        }

        private bool IsLinkFileExistInCurrentUser(Environment.SpecialFolder specialFolder)
        {
            // check if shortcut exist in current user's directory
            var specialFolderPath = Environment.GetFolderPath(specialFolder);
            var pdfUtilPath = Path.Combine(specialFolderPath, Properties.Resources.PDF_UTILITY_TITLE + ".lnk");
            return File.Exists(pdfUtilPath);
        }

        public static bool IsPdfUtilDefault()
        {
            var associateName = NativeMethods.AssocQueryString(NativeMethods.AssocStr.FriendlyAppName, PdfHelper.PdfExtension);
            return !string.IsNullOrEmpty(associateName) && (associateName.ToLower().Equals("pdfutil") || associateName.Equals(Properties.Resources.PDF_UTILITY_TITLE));
        }

        public static void SetPdfUtilAsDefault()
        {
            RegeditOperation.SetAdminConfigRegistryStringValue(PdfHelper.PdfExtension, "1");
        }
    }
}
