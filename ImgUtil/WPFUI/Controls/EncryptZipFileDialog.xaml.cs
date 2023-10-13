using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for EncryptZipFileDialog.xaml
    /// </summary>
    public partial class EncryptZipFileDialog : BaseWindow
    {
        private ImgUtilView _view;

        public EncryptZipFileDialog(ImgUtilView view)
        {
            InitializeComponent();

            _view = view;
            if (_view.IsLoaded)
            {
                Owner = _view;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            encryptRadioButton.IsEnabled = WinzipMethods.AllowedEncrypt(new WindowInteropHelper(this).Handle);
            passwordBox.IsEnabled = encryptRadioButton.IsEnabled;
            confirmPasswordBox.IsEnabled = encryptRadioButton.IsEnabled;
            if (encryptRadioButton.IsEnabled)
            {
                passwordBox.FocusPasswordBox();
                encryptRadioButton.IsChecked = true;
                justZipRadioButton.IsChecked = false;
            }
            else
            {
                encryptRadioButton.IsChecked = false;
                justZipRadioButton.IsChecked = true;
            }
            
            if (WinzipMethods.IsAlwaysEncrypt(new WindowInteropHelper(this).Handle))
            {
                justZipRadioButton.IsChecked = false;
                justZipRadioButton.IsEnabled = false;
            }
        }

        public new DialogResult Show()
        {
            bool ret = BaseShowWindow();
            return ret ? System.Windows.Forms.DialogResult.OK : System.Windows.Forms.DialogResult.Cancel;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDialogContent())
            {
                this.DialogResult = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private bool CheckDialogContent()
        {
            if (encryptRadioButton.IsChecked == true)
            {
                if (string.IsNullOrEmpty(passwordBox.Password))
                {
                    FlatMessageWindows.DisplayWarningMessage(_view.WindowHandle, Properties.Resources.WARNING_ENCRYPT_PASSWORD_EMPTY);
                    passwordBox.FocusPasswordBox();
                    return false;
                }

                if (string.IsNullOrEmpty(confirmPasswordBox.Password))
                {
                    FlatMessageWindows.DisplayWarningMessage(_view.WindowHandle, Properties.Resources.WARNING_ENCRYPT_PASSWORD_EMPTY);
                    confirmPasswordBox.FocusPasswordBox();
                    return false;
                }

                if (!passwordBox.Password.Equals(confirmPasswordBox.Password))
                {
                    FlatMessageWindows.DisplayWarningMessage(_view.WindowHandle, Properties.Resources.WARNING_ENCRYPT_PASSWORD_NOT_MATCH);
                    passwordBox.FocusPasswordBox();
                    return false;
                }

                if (!IsPasswordValidate(passwordBox.Password))
                {
                    passwordBox.FocusPasswordBox();
                    return false;
                }
            }

            return true;
        }

        private bool IsPasswordValidate(string password)
        {
            return WinzipMethods.CheckPasswordPolicyCompliance(new WindowInteropHelper(this).Handle, password, false);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                return;
            }
            else if (e.Key == Key.Enter)
            {
                if (CheckDialogContent())
                {
                    this.DialogResult = true;
                    return;
                }
                else
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        public string GetPassword()
        {
            if (encryptRadioButton.IsChecked == true)
            {
                return passwordBox.Password;
            }
            else
            {
                return string.Empty;
            }
        }

        private void EncryptZipFileDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void EncryptZipFileDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
