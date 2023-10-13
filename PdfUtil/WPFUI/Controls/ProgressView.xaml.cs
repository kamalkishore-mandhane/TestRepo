using Microsoft.Win32;
using PdfUtil.WPFUI.View;
using PdfUtil.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for ProgressView.xaml
    /// </summary>
    public partial class ProgressView : BaseWindow
    {
        private delegate void ProgressEventHander(ProgressEventArgs e);
        private event ProgressEventHander ProgressChangeEvent;
        private bool? _userCancel = null;
        private ProgressOperation _operation;

        public ProgressView(PdfUtilView view, ProgressOperation operation, bool disabelCancel = false)
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
            _operation = operation;

            // disable the cancel button for convert pdf to doc(x) since Aspose does not support cancel this kind of conversion.
            // It will be removed when Aspose fixes this.
            cancelButton.IsEnabled = !disabelCancel;
        }

        public void InvokeProgressEvent(ProgressEventArgs e)
        {
            ProgressChangeEvent?.Invoke(e);
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private void ProgressView_ProgressChangeEvent(ProgressEventArgs e)
        {
            if (e.Status == ProgressStatus.Completed)
            {
                cancelButton.IsEnabled = true;
                _userCancel = false;
                Close();
                return;
            }

            if (e.TotalItemsCount < 2)
            {
                progressBar.Style = FindResource("Shared.ProcessBarMarqueeStyle") as Style;
                progressBar.IsIndeterminate = true;
            }
            else
            {
                progressBar.Style = FindResource("Shared.ProcessBarStyle") as Style;
                progressBar.IsIndeterminate = false;
                progressBar.Maximum = e.TotalItemsCount;
                progressBar.Value = e.CurExtractItemIndex;
            }

            switch (_operation)
            {
                case ProgressOperation.Watermark:
                    contentText.Text = string.Format(Properties.Resources.CONVERTING_FILES, e.TotalItemsCount);
                    break;
                case ProgressOperation.ExtractImage:
                    if (!string.IsNullOrEmpty(e.FileName))
                    {
                        contentText.Text = string.Format(Properties.Resources.EXTRACT_IMAGE_PROGRESS_TEXT_CONTENT, e.FileName);
                    }
                    break;
                case ProgressOperation.SaveAs:
                    if (!string.IsNullOrEmpty(e.FileName))
                    {
                        contentText.Text = string.Format(Properties.Resources.SAVE_AS_PROGRESS_TEXT_CONTENT, e.FileName);
                    }
                    break;
                case ProgressOperation.ExtractPage:
                    if (!string.IsNullOrEmpty(e.FileName))
                    {
                        contentText.Text = string.Format(Properties.Resources.EXTRACT_PAGE_PROGRESS_TEXT_CONTENT, e.FileName);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            ProgressChangeEvent -= ProgressView_ProgressChangeEvent;
            ProgressChangeEvent += ProgressView_ProgressChangeEvent;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = Application.Current.MainWindow.DataContext as PdfUtilViewModel;
            if (viewModel != null)
            {
                viewModel.SendProgressCancelEvent();
            }

            _userCancel = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cancelButton.IsEnabled == false)
            {
                e.Cancel = true;
            }
            else
            {
                var viewModel = Application.Current.MainWindow.DataContext as PdfUtilViewModel;
                if (viewModel != null && _userCancel == null)
                {
                    viewModel.SendProgressCancelEvent();
                }
            }
        }

        private void Window_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }

    public class ProgressEventArgs
    {
        public int TotalItemsCount { get; set; }
        public int CurExtractItemIndex { get; set; }
        public ProgressStatus Status { get; set; }
        public string FileName { get; set; }
    }

    public enum ProgressStatus
    {
        None,
        InProgress,
        Completed,
        Cancel
    }

    public enum ProgressOperation
    {
        ExtractImage,
        ExtractPage,
        SaveAs,
        Watermark
    }
}
