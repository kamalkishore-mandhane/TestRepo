using Aspose.Pdf;
using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using PdfUtil.WPFUI.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CommonDialogResult = System.Windows.Forms.DialogResult;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for LockPDFDialog.xaml
    /// </summary>
    partial class LockPDFDialog : BaseWindow
    {
        public LockPDFDialog(PdfUtilView view)
        {
            InitializeComponent();
            if (view.IsLoaded)
            {
                Owner = view;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            var viewModel = Application.Current.MainWindow.DataContext as PdfUtilViewModel;
            if (viewModel != null)
            {
                DataContext = viewModel.LockPDFViewModel.Clone();
            }
        }

        public CommonDialogResult Result { get; set; }

        public new CommonDialogResult Show()
        {
            BaseShowWindow();
            return Result;
        }

        private void lockBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = CheckDialogContent();
            if (Result == CommonDialogResult.OK || Result == CommonDialogResult.Cancel)
            {
                var viewModel = Application.Current.MainWindow.DataContext as PdfUtilViewModel;
                if (viewModel != null)
                {
                    viewModel.LockPDFViewModel = DataContext as LockPDFViewModel;
                }

                Close();
                e.Handled = true;
            }
        }

        private void LockPDFDialog_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as LockPDFViewModel;
            var element = Keyboard.FocusedElement;
            if ((element is PasswordBox || viewModel == null) && e.Key != Key.Enter && e.Key != Key.Escape)
            {
                return;
            }

            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                Result = CheckDialogContent();
                if (Result == CommonDialogResult.OK || Result == CommonDialogResult.Cancel)
                {
                    var pdfViewModel = Application.Current.MainWindow.DataContext as PdfUtilViewModel;
                    if (pdfViewModel != null)
                    {
                        pdfViewModel.LockPDFViewModel = DataContext as LockPDFViewModel;
                    }
                    Close();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape) 
            {
                Result = CommonDialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private CommonDialogResult CheckDialogContent()
        {
            var viewModel = DataContext as LockPDFViewModel;
            if (viewModel == null)
            {
                return CommonDialogResult.Abort;
            }

            if (openPDFCheckbox.IsChecked == true)
            {
                if (!VerifyPasswordFormat(true))
                {
                    return CommonDialogResult.Retry;
                }
                viewModel.IsSetOpenPassword = true;
            }
            if (changePDFCheckbox.IsChecked == true)
            {
                if (!VerifyPasswordFormat(false))
                {
                    return CommonDialogResult.Retry;
                }

                if (openPasswordBox.Password.Equals(changePasswordBox.Password))
                {
                    FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, Properties.Resources.PASSWORD_CONSISTENCY_WARNING);
                    return CommonDialogResult.Retry;
                }

                viewModel.IsSetPermissionPassword = true;
            }

            if (viewModel.IsSetOpenPassword || viewModel.IsSetPermissionPassword)
            {
                if (viewModel.IsSetPermissionPassword)
                {
                    if (FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, Properties.Resources.WARNING_LOCK_PDF_WARNING))
                    {
                        return CommonDialogResult.OK;
                    }
                    else
                    {
                        return CommonDialogResult.Retry;
                    }
                }

                return CommonDialogResult.OK;
            }
            else
            {
                return CommonDialogResult.Cancel;
            }
        }

        private bool VerifyPasswordFormat(bool isOpenPsd)
        {
            var passwordBox = isOpenPsd ? openPasswordBox : changePasswordBox;
            var verifyBox = isOpenPsd ? openPasswordVerifyBox : changePasswordVerifyBox;
            if (string.IsNullOrEmpty(passwordBox.Password))
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, isOpenPsd ? Properties.Resources.WARNING_OPEN_PASSWORD_EMPTY : Properties.Resources.WARNING_PERMISSION_PASSWORD_EMPTY);
                passwordBox.FocusPasswordBox();
                return false;
            }
            else if (string.IsNullOrEmpty(verifyBox.Password))
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, isOpenPsd ? Properties.Resources.WARNING_OPEN_PASSWORD_EMPTY : Properties.Resources.WARNING_PERMISSION_PASSWORD_EMPTY);
                verifyBox.FocusPasswordBox();
                return false;
            }
            else if (!passwordBox.Password.Equals(verifyBox.Password))
            {
                FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, isOpenPsd ? Properties.Resources.WARNING_OPEN_PASSWORD_NOT_MATCH : Properties.Resources.WARNING_PERMISSION_PASSWORD_NOT_MATCH);
                passwordBox.FocusPasswordBox();
                return false;
            }
            // Other password format need verified.

            return true;
        }

        private void openPDFCheckbox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox.IsChecked == false)
            {
                openPasswordBox.Clear();
                openPasswordVerifyBox.Clear();
            }
            else
            {
                openPasswordBox.FocusPasswordBox();
            }
        }

        private void changePDFCheckbox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox.IsChecked == false)
            {
                changePasswordBox.Clear();
                changePasswordVerifyBox.Clear();
            }
            else
            {
                changePasswordBox.FocusPasswordBox();
            }
        }

        private void LockPDFDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void LockPDFDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }

    public enum AllowChanges
    {
        None,
        ModifyPagesPermission, // Insert pages, delete pages and rotate pages
        ModifySignaturePermission, // Fill in form fields and sign existing signature fields
        ModifyCommentsPermission, // Comments, fill in form fields and sign existing signature fields
        AnyExceptExtracting // Any except extracting pages
    }

    public enum AllowPrinting
    {
        None,
        LowResolution, // Low Resolution(150 dpi)
        HighResolution // High Resolution
    }

    public class LockPDFViewModel: INotifyPropertyChanged
    {
        private bool _isSetOpenPassword = false;
        private bool _isSetPermissionPassword = false;
        private string _openPassword = string.Empty;
        private string _openVerifyPassword = string.Empty;
        private string _permissionPassword = string.Empty;
        private string _permissionVerifyPassword = string.Empty;
        private bool _isAllowCopyingChecked = false; // Allow copying of text, images, and other content
        private bool _isAllowScreenReaderChecked = false; // Allow text access for screen reader devices for visually impaired
        private AllowChanges _curAllowChanges = AllowChanges.None;
        private AllowPrinting _curAllowPrinting = AllowPrinting.None;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsSetOpenPassword
        {
            get
            {
                return _isSetOpenPassword;
            }
            set
            {
                if (_isSetOpenPassword != value)
                {
                    _isSetOpenPassword = value;
                    OnPropertyChanged(nameof(IsSetOpenPassword));
                }
            }
        }

        public bool IsSetPermissionPassword
        {
            get
            {
                return _isSetPermissionPassword;
            }
            set
            {
                if (_isSetPermissionPassword != value)
                {
                    _isSetPermissionPassword = value;
                    OnPropertyChanged(nameof(IsSetPermissionPassword));
                }
            }
        }

        public string OpenPassword
        {
            get
            {
                if (!IsSetOpenPassword)
                {
                    return string.Empty;
                }

                return _openPassword;
            }
            set
            {
                if (_openPassword != value)
                {
                    _openPassword = value;
                    OnPropertyChanged(nameof(OpenPassword));
                }
            }
        }

        public string OpenVerifyPassword
        {
            get
            {
                if (!IsSetOpenPassword)
                {
                    return string.Empty;
                }

                return _openVerifyPassword;
            }
            set
            {
                if (_openVerifyPassword != value)
                {
                    _openVerifyPassword = value;
                    OnPropertyChanged(nameof(OpenVerifyPassword));
                }
            }
        }

        public string PermissionPassword
        {
            get
            {
                if (!IsSetPermissionPassword)
                {
                    return string.Empty;
                }

                return _permissionPassword;
            }
            set
            {
                if (_permissionPassword != value)
                {
                    _permissionPassword = value;
                    OnPropertyChanged(nameof(PermissionPassword));
                }
            }
        }

        public string PermissionVerifyPassword
        {
            get
            {
                if (!IsSetPermissionPassword)
                {
                    return string.Empty;
                }

                return _permissionVerifyPassword;
            }
            set
            {
                if (_permissionVerifyPassword != value)
                {
                    _permissionVerifyPassword = value;
                    OnPropertyChanged(nameof(PermissionVerifyPassword));
                }
            }
        }

        public bool IsAllowCopyingChecked
        {
            get
            {
                return _isAllowCopyingChecked;
            }
            set
            {
                if (_isAllowCopyingChecked != value)
                {
                    _isAllowCopyingChecked = value;
                    OnPropertyChanged(nameof(IsAllowCopyingChecked));
                    if (value)
                    {
                        _isAllowScreenReaderChecked = value;
                        OnPropertyChanged(nameof(IsAllowScreenReaderChecked));
                    }
                }
            }
        }

        public bool IsAllowScreenReaderChecked
        {
            get
            {
                return _isAllowScreenReaderChecked;
            }
            set
            {
                if (_isAllowScreenReaderChecked != value)
                {
                    _isAllowScreenReaderChecked = value;
                    OnPropertyChanged(nameof(IsAllowScreenReaderChecked));
                }
            }
        }

        public AllowChanges CurAllowChanges
        {
            get
            {
                return _curAllowChanges;
            }
            set
            {
                if (_curAllowChanges != value)
                {
                    _curAllowChanges = value;
                    OnPropertyChanged(nameof(CurAllowChanges));
                }
            }
        }

        public AllowPrinting CurAllowPrinting
        {
            get
            {
                return _curAllowPrinting;
            }
            set
            {
                if (_curAllowPrinting != value)
                {
                    _curAllowPrinting = value;
                    OnPropertyChanged(nameof(CurAllowPrinting));
                }
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private void ResetPermissions()
        {
            IsAllowCopyingChecked = false;
            IsAllowScreenReaderChecked = true;
            CurAllowChanges = AllowChanges.None;
            CurAllowPrinting = AllowPrinting.None;
        }

        public void ParserPasswordAndPermission(PdfLockStatus lockStatus)
        {
            IsSetOpenPassword = lockStatus.isSetOpenPassword;
            OpenPassword = IsSetOpenPassword ? lockStatus.openPassword : string.Empty;
            OpenVerifyPassword = OpenPassword;

            IsSetPermissionPassword = lockStatus.isSetPermissionPassword;
            PermissionPassword = IsSetPermissionPassword ? lockStatus.permissionPassword : string.Empty;
            PermissionVerifyPassword = PermissionPassword;

            if (!IsSetPermissionPassword)
            {
                ResetPermissions();
            }
            else
            {
                if ((lockStatus.permissions & Permissions.ExtractContentWithDisabilities) != 0)
                {
                    if ((lockStatus.permissions & Permissions.ExtractContent) != 0)
                    {
                        IsAllowCopyingChecked = true;
                    }
                    else
                    {
                        IsAllowScreenReaderChecked = true;
                    }
                }
                else
                {
                    IsAllowCopyingChecked = false;
                    IsAllowScreenReaderChecked = false;
                }

                if ((lockStatus.permissions & Permissions.PrintDocument) != 0)
                {
                    if ((lockStatus.permissions & Permissions.PrintingQuality) != 0)
                    {
                        CurAllowPrinting = AllowPrinting.HighResolution;
                    }
                    else
                    {
                        CurAllowPrinting = AllowPrinting.LowResolution;
                    }
                }
                else
                {
                    CurAllowPrinting = AllowPrinting.None;
                }

                if ((lockStatus.permissions & Permissions.AssembleDocument) != 0)
                {
                    CurAllowChanges = AllowChanges.ModifyPagesPermission;
                }
                else if ((lockStatus.permissions & Permissions.FillForm) != 0)
                {
                    if ((lockStatus.permissions & Permissions.ModifyTextAnnotations) != 0 && (lockStatus.permissions & Permissions.ModifyContent) != 0)
                    {
                        CurAllowChanges = AllowChanges.AnyExceptExtracting;
                    }
                    else if ((lockStatus.permissions & Permissions.ModifyTextAnnotations) != 0)
                    {
                        CurAllowChanges = AllowChanges.ModifyCommentsPermission;
                    }
                    else
                    {
                        CurAllowChanges = AllowChanges.ModifySignaturePermission;
                    }
                }
                else
                {
                    CurAllowChanges = AllowChanges.None;
                }
            }
        }
    }
}
