using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FlatMessageBox.xaml
    /// </summary>
    public partial class FlatMessageBox : BaseWindow
    {
        public FlatMessageBox()
        {
            InitializeComponent();
            if (Application.Current?.MainWindow.IsLoaded ?? false)
            {
                Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Topmost = true;
            }
        }

        private FlatMessageBox(string content, ImageSource icon, bool yesnoButtons = true, bool showDoNotShowCB = false, string title = "")
            : this()
        {
            DialogTitle.Text = content;
            DialogIcon.Source = icon;
            if (icon == null)
            {
                DialogTitle.Width = DialogTitle.Width + DialogIcon.Width + DialogIcon.Margin.Left + DialogIcon.Margin.Right;
                DialogIcon.Visibility = Visibility.Collapsed;
            }
            if (!yesnoButtons)
            {
                ButtonNo.Visibility = Visibility.Collapsed;
            }
            if (showDoNotShowCB)
            {
                DoNotShowCB.Visibility = Visibility.Visible;
            }
            if (title.Length > 0)
            {
                this.Title = title;
            }
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ButtonNo_Click(null, null);
                    break;
                default:
                    break;
            }
        }

        private void SetButtonDefaulted(bool isDefaultYes)
        {
            ButtonYes.IsDefault = isDefaultYes;
            ButtonNo.IsDefault = !isDefaultYes;
        }

        private void HideSkin()
        {
            SkinBorder.Visibility = Visibility.Hidden;
        }

        private void SetButtonText(string acceptButtonText, string cancelButtonText)
        {
            if (!string.IsNullOrEmpty(acceptButtonText))
            {
                ButtonYes.Content = acceptButtonText;
            }

            if (!string.IsNullOrEmpty(cancelButtonText))
            {
                ButtonNo.Content = cancelButtonText;
            }
        }

        private void SetDialogTitleNormalStyle()
        {
            DialogTitle.Style = this.FindResource("NormalTextBlockStyle") as Style;
        }

        private void SetDialogTitleWidth(int width)
        {
            DialogTitle.Width = width;
        }

        public static bool ShowDialog(IntPtr owner, string title, ImageSource icon)
        {
            var window = new FlatMessageBox(title, icon);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }
            return window.BaseShowWindow();
        }

        public static bool ShowDialog(IntPtr owner, string title, ImageSource icon, bool isDefaultYes)
        {
            var window = new FlatMessageBox(title, icon);
            window.SetButtonDefaulted(isDefaultYes);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }
            return window.BaseShowWindow();
        }

        public static bool ShowDialog(IntPtr owner, string title, ImageSource icon, string acceptButtonText, string cancelButtonText, bool yesnoButtons)
        {
            var window = new FlatMessageBox(title, icon, yesnoButtons);
            window.SetButtonText(acceptButtonText, cancelButtonText);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }
            return window.BaseShowWindow();
        }

        public static void ShowCanBeOmittedDialog(IntPtr owner, string content, string title, ImageSource icon, bool isDefaultYes, string acceptButtonText, ref bool showDoNotShowCB)
        {
            if (showDoNotShowCB)
            {
                return;
            }

            var window = new FlatMessageBox(content, icon, false, true, title);
            window.SetButtonDefaulted(isDefaultYes);
            window.HideSkin();
            window.SetButtonText(acceptButtonText, string.Empty);
            window.SetDialogTitleNormalStyle();
            window.SetDialogTitleWidth(345);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }
            window.BaseShowWindow();
            showDoNotShowCB = window.DoNotShowCB.IsChecked.GetValueOrDefault();
        }

        private void FlatMessageBox_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void FlatMessageBox_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }

    public static class SetWindowStartupCenterExtension
    {
        public static void SetWindowStartupCenter(this Window window, IntPtr owner)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            IntPtr rootOwner;
            rootOwner = NativeMethods.GetAncestor(owner, NativeMethods.GA_ROOT);
            helper.Owner = rootOwner;
            NativeMethods.RECT rect;
            NativeMethods.GetWindowRect(rootOwner, out rect);

            window.SourceInitialized += delegate
            {
                int ownerLeft = rect.left;
                int ownerWidth = rect.right - rect.left;
                int ownerTop = rect.top;
                int ownerHeight = rect.bottom - rect.top;

                // Get transform matrix to transform owner window
                // size and location units into device-independent WPF
                // size and location units
                HwndSource source = HwndSource.FromHwnd(helper.Handle);
                if (source == null)
                {
                    return;
                }
                Matrix matrix = source.CompositionTarget.TransformFromDevice;
                Point ownerWPFSize = matrix.Transform(
                  new Point(ownerWidth, ownerHeight));
                Point ownerWPFPosition = matrix.Transform(
                  new Point(ownerLeft, ownerTop));

                // Center WPF window
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = ownerWPFPosition.X + (ownerWPFSize.X - window.ActualWidth) / 2;
                window.Top = ownerWPFPosition.Y + (ownerWPFSize.Y - window.ActualHeight) / 2;

                // Keep window in screen
                System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(owner);
                System.Drawing.Rectangle workingRect = currentScreen.WorkingArea;

                Point workingWPFLeftTop = matrix.Transform(new Point(workingRect.Left, workingRect.Top));
                Point workingWPFRightBottom = matrix.Transform(new Point(workingRect.Right, workingRect.Bottom));

                if (window.Left + window.ActualWidth > workingWPFRightBottom.X)
                {
                    window.Left = workingWPFRightBottom.X - window.ActualWidth;
                }

                if (window.Top + window.ActualHeight > workingWPFRightBottom.Y)
                {
                    window.Top = workingWPFRightBottom.Y - window.ActualHeight;
                }

                if (window.Left < workingWPFLeftTop.X)
                {
                    window.Left = workingWPFLeftTop.X;
                }

                if (window.Top < workingWPFLeftTop.Y)
                {
                    window.Top = workingWPFLeftTop.Y;
                }
            };
        }
    }
}
