using ImgUtil.WPFUI.Utils;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FlatMessageBox.xaml
    /// </summary>
    public partial class FlatMessageBox : BaseWindow
    {
        public FlatMessageBox()
        {
            InitializeComponent();

            if (Application.Current?.MainWindow.Visibility == Visibility.Visible)
            {
                Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Topmost = true;
            }
        }

        private FlatMessageBox(string content, ImageSource icon, bool yesnoButtons = true)
            : this()
        {
            DialogTitle.Text = content;
            DialogIcon.Source = icon;
            if (!yesnoButtons)
            {
                ButtonNo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private FlatMessageBox(string title, ImageSource icon, string acceptButtonText, string cancelButtonText, bool yesnoButtons)
            : this(title, icon, yesnoButtons)
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

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

        public static bool ShowDialog(IntPtr owner, string title, ImageSource icon, string acceptButtonText, string cancelButtonText, bool yesnoButtons)
        {
            var window = new FlatMessageBox(title, icon, acceptButtonText, cancelButtonText, yesnoButtons);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }
            return window.BaseShowWindow();
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
            IntPtr rootOwner = NativeMethods.GetAncestor(owner, NativeMethods.GA_ROOT);
            if (rootOwner == IntPtr.Zero)
            {
                rootOwner = owner;
            }
            helper.Owner = rootOwner;
            NativeMethods.GetWindowRect(rootOwner, out NativeMethods.RECT rect);

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
