using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for PasswordAcquireDialog.xaml
    /// </summary>
    public partial class PasswordAcquireDialog : BaseWindow
    {
        public PasswordAcquireDialog(PdfUtilView view, string fileName, bool isOpenPassword)
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
            if (isOpenPassword)
            {
                enterPasswordText.Text = string.Format(Properties.Resources.PDF_OPEN_PASSWORD_ACQUIRE, fileName);
            }
            else
            {
                enterPasswordText.Text = string.Format(Properties.Resources.PDF_PERMISSION_PASSWORD_ACQUIRE, fileName);
            }
        }

        public string Password
        {
            get { return passwordControl.Password; }
        }

        public new bool Show()
        {
            passwordControl.FocusPasswordBox();
            return BaseShowWindow();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void PasswordAcquireDialog_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    okBtn_Click(null, null);
                    break;
                case Key.Escape:
                    DialogResult = false;
                    break;
                default:
                    break;
            }
        }

        private void PasswordAcquireDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void PasswordAcquireDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void PasswordAcquireDialog_ContentRendered(object sender, System.EventArgs e)
        {
            // WZ-13185, simulate mouse click to restore foreground
            if (NativeMethods.GetForegroundWindow() != new WindowInteropHelper(this).Handle)
            {
                var point = enterPasswordText.PointToScreen(default(Point));
                var oldPos = System.Windows.Forms.Cursor.Position;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)point.X, (int)point.Y);
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                System.Windows.Forms.Cursor.Position = oldPos;
            }
        }
    }
}
