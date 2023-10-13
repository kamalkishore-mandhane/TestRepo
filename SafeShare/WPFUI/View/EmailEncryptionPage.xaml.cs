using Microsoft.Win32;
using SafeShare.Properties;
using SafeShare.Util;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for EmailEncryptionPage.xaml
    /// </summary>
    public partial class EmailEncryptionPage : BasePage
    {
        private const int _fileNameMaxLen = 251;
        private const int _tooltipAppearTime = 500;      // 500ms
        private const int _copiedTooltipExistTime = 3;   // 3s

        private FileListPage _fileListPage;
        private EmailEncryptionPageViewModel _viewModel;
        private DispatcherTimer _passwordTooltipTimer;

        public EmailEncryptionPage(FileListPage filePage)
        {
            _fileListPage = filePage;
            InitializeComponent();
            InitTooltipTimer();
        }

        public void InitDataContext()
        {
            _viewModel = new EmailEncryptionPageViewModel(this, _fileListPage);
            this.DataContext = _viewModel;
        }

        public void UpdateDataContext(FileListPage fileListPage)
        {
            _fileListPage = fileListPage;
            _viewModel.FileListPageView = _fileListPage;
        }

        public void NextPage()
        {
            _viewModel.ShowFileNameErrorTips = false;
            if (_viewModel.ZipFileName.Equals(string.Empty))
            {
                _viewModel.FileNameErrorTips = Properties.Resources.FILE_NAME_EMPTY;
                _viewModel.ShowFileNameErrorTips = true;
                SharedFileNameTextBox.TextBox.Focus();
                SharedFileNameTextBox.TextBox.Select(_viewModel.ZipFileName.Length, 0);
                return;
            }
            else if (_viewModel.ZipFileName.Length > _fileNameMaxLen)
            {
                _viewModel.ShowFileNameErrorTips = true;
                _viewModel.FileNameErrorTips = Properties.Resources.FILE_NAME_TOO_LONG;
                SharedFileNameTextBox.TextBox.Focus();
                SharedFileNameTextBox.TextBox.Select(_viewModel.ZipFileName.Length, 0);
                return;
            }
            else
            {
                _viewModel.ShowFileNameErrorTips = false;
            }

            if (!_viewModel.ZipFileName.EndsWith(_viewModel.ZipExt, true, null))
            {
                if (WinZipMethodHelper.ValidateZipFile(_viewModel.ZipFileName))
                {
                    _viewModel.ZipFileName = Path.ChangeExtension(_viewModel.ZipFileName, _viewModel.ZipExt);
                }
                else
                {
                    _viewModel.ZipFileName += _viewModel.ZipExt;
                }
            }

            Settings.Default.EmailAttachmentIsChecked = _viewModel.EmailAttachmentIsChecked;
            Settings.Default.EmailLinkIsChecked = _viewModel.EmailLinkIsChecked;
            Settings.Default.EncryptFile = _viewModel.EncryptFileIsChecked;
            Settings.Default.Save();

            WorkFlowManager.ShareOptionData.UseEmailAttachment = _viewModel.EmailAttachmentIsChecked;
            WorkFlowManager.ShareOptionData.UseEmailLink = _viewModel.EmailLinkIsChecked;
            WorkFlowManager.ShareOptionData.EncryptFile = _viewModel.EncryptFileIsChecked;

            WorkFlowManager.GoConversionPage(_fileListPage, this);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.GoBack();
        }

        private void EmailAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.StartManageEmail(this);
        }

        private void EmailServiceButton_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.StartManageCloud(this);
        }

        private void AddEmailHyperlink_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.StartManageEmail(this);
        }

        private void AddCloudHyperlink_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.StartManageCloud(this);
        }

        private void EmailEncryptionPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            // Other initalizations
            _viewModel.NotifyEmailAndCloudChanges();
            _viewModel.NotifyFilesizeAndCount();
            _viewModel.UpdateZipFileName();
            _viewModel.GenerateSuggestedPasswordIfEmpty();
            _viewModel.IsPasswordHide = true;

            ForceEncrypt();
            NextButton.Focus();
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.StartEncryptOption(this);
        }

        private void PasswordCopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    Clipboard.SetDataObject(_viewModel.EncryptPassword);
                }
                catch(Exception ex)
                {
                    SimpleMessageWindows.DisplayWarningMessage(ex.Message);
                    return;
                }

                if (PasswordTooltip.IsOpen == false)
                {
                    _viewModel.IsPasswordCopied = true;
                    CalculateTooltipOffset(PasswordTooltip);
                    PasswordTooltip.IsOpen = true;

                    // Setup a timer to close the popup in 3 seconds
                    DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(_copiedTooltipExistTime) };
                    timer.Tick += delegate (object obj, EventArgs arg)
                    {
                        timer.Stop();
                        if (PasswordTooltip.IsOpen)
                        {
                            PasswordTooltip.IsOpen = false;
                        }
                    };

                    timer.Start();
                }
            }
        }

        private void CalculateTooltipOffset(Popup popup)
        {
            var formattedText = new FormattedText(
                PasswordTooltipText.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(PasswordTooltipText.FontFamily, PasswordTooltipText.FontStyle, PasswordTooltipText.FontWeight,
                PasswordTooltipText.FontStretch), PasswordTooltipText.FontSize, PasswordTooltipText.Foreground);
            var textSize = new Size(formattedText.Width, formattedText.Height);
            var tooltipWidth = textSize.Width + 20; // 20 is all left + right padding size.

            popup.HorizontalOffset = -((tooltipWidth - PasswordCopyButton.ActualWidth) / 2); // move tooltip left to align with the PasswordCopyButton
        }

        private void InitTooltipTimer()
        {
            _passwordTooltipTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _passwordTooltipTimer.Interval = TimeSpan.FromMilliseconds(_tooltipAppearTime);
            _passwordTooltipTimer.Tick += delegate (object obj, EventArgs arg)
            {
                _passwordTooltipTimer.Stop();
                PasswordTooltip.IsOpen = true;
            };
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void EmailEncryptionPageView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;

            var fileListPageViewModel = _fileListPage.DataContext as FileListPageViewModel;
            if (fileListPageViewModel.ItemsFullPathList.Count > 0)
            {
                _viewModel.IsCustomFileName = _viewModel.ZipFileName != _viewModel.GetDefaultZipFileName();
                TrackHelper.TrackHelperInstance.NameChanged = _viewModel.IsCustomFileName;
            }
        }

        private void EmailAttachmentLimitTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            // Move tooltip to top center
            if (sender is TextBlock textBlock)
            {
                var toolTip = textBlock.ToolTip as ToolTip;
                toolTip.VerticalOffset = -toolTip.Height - EmailAttachmentLimitTextBlock.ActualHeight;
                toolTip.HorizontalOffset = (EmailAttachmentLimitTextBlock.ActualWidth - toolTip.Width) / 2;
            }
            base.OnMouseEnter(e);
        }

        private void ForceEncrypt()
        {
            if (WinzipMethods.IsAlwaysEncrypt(MainWindow.WindowHandle))
            {
                var viewModel = DataContext as EmailEncryptionPageViewModel;
                viewModel.EncryptFileIsChecked = true;
                viewModel.EncryptFileIsEnabled = false;
            }
        }

        private void PasswordHideButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPasswordHide = !_viewModel.IsPasswordHide;
        }
    }
}