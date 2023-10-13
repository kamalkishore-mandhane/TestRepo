using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SafeShare.WPFUI.View
{
    public enum CONVERT_ERROR_DIALOG
    {
        Continue,
        Skip,
        Cancel
    }

    /// <summary>
    /// Interaction logic for ConversionErrorDlgView.xaml
    /// </summary>
    public partial class ConversionErrorDlgView : BaseWindow
    {
        private int _errorCode;
        private string _fileName;
        private string _errorMsg;
        private string _conversionName;

        public ConversionErrorDlgView()
        {
            InitializeComponent();
        }

        private ConversionErrorDlgView(string conversionName, string fileName, int errorCode, string errorMsg)
            : this()
        {
            _errorCode = errorCode;
            _fileName = fileName;
            _errorMsg = errorMsg;
            _conversionName = conversionName;
        }

        public CONVERT_ERROR_DIALOG DialogChoice
        {
            get;
            set;
        }

        public bool NotShowAgain
        {
            get;
            set;
        } = false;

        public static bool ShowDialog(IntPtr owner, string conversionName, string fileName, int errorCode, string errorMsg, out CONVERT_ERROR_DIALOG choice, out bool notShowAgain)
        {
            var window = new ConversionErrorDlgView(conversionName, fileName, errorCode, errorMsg);
            if (owner != IntPtr.Zero)
            {
                window.SetWindowStartupCenter(owner);
            }

            bool ret = window.BaseShowWindow();

            choice = window.DialogChoice;
            notShowAgain = window.NotShowAgain;

            return ret;
        }

        private void ConversionErrorDlgView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            ContinueDescTextBox.Text = string.Format(ContinueDescTextBox.Text, _fileName);
            SkipDescTextBox.Text = string.Format(SkipDescTextBox.Text, _fileName);

            var fileNameWithQuo = string.Format("\"{0}\"", _fileName);
            ErrorDescMsgTextBox.Text = string.Format(ErrorDescMsgTextBox.Text, fileNameWithQuo);

            var conversionNameWithQuo = string.Format("\"{0}\"", _conversionName);
            ErrorDescTitleTextBox.Text = string.Format(ErrorDescTitleTextBox.Text, conversionNameWithQuo);

            ConvertErrorTextBlock.Text = string.Format(ConvertErrorTextBlock.Text, _conversionName);
            ErrorCodeTextBlock.Text = string.Format(ErrorCodeTextBlock.Text, _errorCode);
            ErrorDetailsDescTextBlock.Text = string.Format(ErrorDetailsDescTextBlock.Text, _errorMsg);

            ContinueRadioButton.IsChecked = true;
            SkipRadioButton.IsChecked = false;
        }

        private void ConversionErrorDlgView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void ConversionErrorDlgView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogChoice = CONVERT_ERROR_DIALOG.Cancel;
            DialogResult = false;
        }

        private void ShowDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailsButton.Visibility = Visibility.Collapsed;
            HideDetailsButton.Visibility = Visibility.Visible;
            ConvertErrorTextBlock.Visibility = Visibility.Visible;
            ErrorCodeTextBlock.Visibility = Visibility.Visible;
            ErrorDetailsDescTextBlock.Visibility = Visibility.Visible;
        }

        private void HideDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailsButton.Visibility = Visibility.Visible;
            HideDetailsButton.Visibility = Visibility.Collapsed;
            ConvertErrorTextBlock.Visibility = Visibility.Collapsed;
            ErrorCodeTextBlock.Visibility = Visibility.Collapsed;
            ErrorDetailsDescTextBlock.Visibility = Visibility.Collapsed;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ContinueRadioButton.IsChecked)
            {
                DialogChoice = CONVERT_ERROR_DIALOG.Continue;
            }
            else
            {
                DialogChoice = CONVERT_ERROR_DIALOG.Skip;
            }

            NotShowAgain = (bool)DoNotShowAgainRadioButton.IsChecked;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogChoice = CONVERT_ERROR_DIALOG.Cancel;
            DialogResult = false;
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