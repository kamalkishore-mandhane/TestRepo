using Microsoft.Win32;
using PdfUtil.WPFUI.View;
using System.Windows;
using System.Windows.Input;
using CommonDialogResult = System.Windows.Forms.DialogResult;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for UnlockPDFDialog.xaml
    /// </summary>
    public partial class UnlockPDFDialog : BaseWindow
    {
        public UnlockPDFDialog(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public string Password
        {
            get { return passwordBox.Password; }
        }

        public CommonDialogResult Result { get; set; }

        private void UnlockPDFDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            passwordBox.Focus();
        }

        public new CommonDialogResult Show()
        {
            BaseShowWindow();
            return Result;
        }

        private void unlockBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = CommonDialogResult.OK;
            Close();
            e.Handled = true;
        }

        private void UnlockPDFDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                Result = CommonDialogResult.OK;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Result = CommonDialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void UnlockPDFDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
