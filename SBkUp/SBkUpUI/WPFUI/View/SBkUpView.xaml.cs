using Applets.Common;
using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SBkUpUI.WPFUI.View
{
    public enum MessageId
    {
        ID_REFRESH_UXNAG_BANNER = 13022,
    }

    /// <summary>
    /// Interaction logic for SBkUpView.xaml
    /// </summary>
    public partial class SBkUpView : BaseWindow
    {
        private bool _isShowGraceBanner;
        private SubscribePageView _subscribePage;
        private GracePeriodPageView _gracePeriodPage;
        private IntPtr _windowHandle = IntPtr.Zero;

        public static SBkUpView MainWindow
        {
            get; private set;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
#if !WZ_APPX
            if (msg == NativeMethods.WM_COMMAND)
            {
                int operation = wParam.ToInt32();
                switch (operation)
                {
                    case (int)MessageId.ID_REFRESH_UXNAG_BANNER:
                        {
                            var periodIndex = (int)lParam.LOWORD();
                            int trialDaysRemaining = lParam.HIWORD();
                            RefreshNagBanner(periodIndex, trialDaysRemaining);
                            break;
                        }
                }
            }
#endif
            return IntPtr.Zero;
        }

        public IntPtr WindowHandle
        {
            get
            {
                return _windowHandle;
            }
        }

        public bool IsClosing
        {
            get;
            private set;
        }

        public double LastPdfWindowTop
        {
            get;
            set;
        }

        public double LastPdfWindowLeft
        {
            get;
            set;
        }

        public SBkUpView(IntPtr parentWindowHandle)
        {
            InitializeComponent();
            IsClosing = false;
            LastPdfWindowLeft = 0;
            LastPdfWindowTop = 0;

            if (parentWindowHandle != IntPtr.Zero)
            {
                var interop = new WindowInteropHelper(this);
                interop.Owner = parentWindowHandle;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                MinimizeButton.IsEnabled = false;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (WzSBkUpUI.settings.MainWindowWidth != 0 && WzSBkUpUI.settings.MainWindowHeight != 0)
            {
                this.Height = WzSBkUpUI.settings.MainWindowHeight;
                this.Width = WzSBkUpUI.settings.MainWindowWidth;
                if (WindowStartupLocation != WindowStartupLocation.CenterOwner)
                {
                    this.Top = WzSBkUpUI.settings.MainWindowTop;
                    this.Left = WzSBkUpUI.settings.MainWindowLeft;
                }
            }

            MainWindow = this;
            DataContext = new ViewModel.SBkUpViewModel(this, EnableSbkupView);
            (DataContext as ViewModel.SBkUpViewModel).PropertyChanged += SBkUpViewModel_PropertyChanged;
            UseTheScrollViewerScrolling(JobListBox);
            UseTheScrollViewerScrolling(JobAndBackup_JobListBox);
            UseTheScrollViewerScrolling(JobAndBackup_BackupListBox);

            (DataContext as ViewModel.SBkUpViewModel).Dispatcher = this.Dispatcher;
            ShowActivated = false;
        }

        private void SBkUpViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = DataContext as ViewModel.SBkUpViewModel;
            if (e.PropertyName == nameof(model.Backups))
            {
                if (model.Backups.Count > 0)
                {
                    NoneSavedTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoneSavedTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        // ~~~
        // handlers to deselect items in Jobs List
        // they are added as workaround because LostFocus event is sent when items are selected
        private void JobListBox_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            JobListBox.SelectedIndex = -1;
        }

        private void SBkUpWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // don't mess with regular ListBox behavior
            if (JobListBox.IsMouseOver)
                return;

            // deselect Jobs List items only if mouse pos is below ribbon
            Point mousePos = this.PointToScreen(Mouse.GetPosition(this));
            Point RibbonBottomPos = FakeRibbonTabControl.PointToScreen(new Point(0, FakeRibbonTabControl.ActualHeight));

            if (mousePos.Y > RibbonBottomPos.Y)
                JobListBox.SelectedIndex = -1;
        }
        // ~~~

        private void JobListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (JobListBox.SelectedItems.Count == 1)
            {
                var displayName = Utils.Util.GetDisplayName((JobListBox.SelectedItems[0] as JobItem).Swjf.storeFolder);
                var text = string.Format(Properties.Resources.BOTTOM_TEXT, displayName);
                bottomTextBlock.Visibility = Visibility.Visible;
                bottomTextBlock.Text = Utils.Util.GetTrimmedString(text, bottomTextBlock.ActualWidth, bottomTextBlock, text.Length - displayName.Length);
                if (text != bottomTextBlock.Text)
                {
                    BottomToolTip_TextBlock.Text = displayName;
                    BottomToolTip.Visibility = Visibility.Visible;
                }
                else
                {
                    BottomToolTip.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BottomToolTip.Visibility = Visibility.Collapsed;
                bottomTextBlock.Visibility = Visibility.Collapsed;
            }

            (DataContext as ViewModel.SBkUpViewModel).SelectedItems.Clear();
            foreach (var item in JobListBox.SelectedItems)
            {
                (DataContext as ViewModel.SBkUpViewModel).SelectedItems.Add(item as JobItem);
            }

            if (!JobListBox.IsFocused)
            {
                JobListBox.Focus();
            }

            if (WzSBkUpUI.OpenMode == OpenMode.autoCreate && e.AddedItems.Count == 1 && e.RemovedItems.Count == 0)
            {
                WzSBkUpUI.OpenMode = OpenMode.Unknown;
                JobListBox.Dispatcher.BeginInvoke((Action)(() =>
                {
                    JobListBox.UpdateLayout();
                    JobListBox.ScrollIntoView(e.AddedItems[0]);
                }));
            }
        }

        private void FakeRibbonTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (backupTab.IsSelected)
            {
                jobView.Visibility = Visibility.Visible;
                bottomTextBlock.Visibility = Visibility.Visible;
                JobAndBackupView.Visibility = Visibility.Collapsed;
            }
            if (restoreTab.IsSelected)
            {
                jobView.Visibility = Visibility.Collapsed;
                bottomTextBlock.Visibility = Visibility.Collapsed;
                JobAndBackupView.Visibility = Visibility.Visible;
            }
        }

        private void JobAndBackup_JobListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var model = DataContext as ViewModel.SBkUpViewModel;
            model.Backups.Clear();
            if (JobAndBackup_JobListBox.SelectedItem != null)
            {
                model.SelectedJob = (JobAndBackup_JobListBox.SelectedItem as JobItem);

                foreach (var item in (JobAndBackup_JobListBox.SelectedItem as JobItem).Backups)
                {
                    model.Backups.Add(item);
                }
            }

            if (model.Backups.Count > 0)
            {
                NoneSavedTextBlock.Visibility = Visibility.Collapsed;
                JobAndBackup_BackupListBox.SelectedItem = model.Backups[0];
            }
            else
            {
                NoneSavedTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void bottomTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (bottomTextBlock.Visibility == Visibility.Visible && JobListBox.SelectedItems.Count == 1)
            {
                var displayName = Utils.Util.GetDisplayName((JobListBox.SelectedItems[0] as JobItem).Swjf.backupFolder);
                var text = string.Format(Properties.Resources.BOTTOM_TEXT, displayName);
                bottomTextBlock.Text = Utils.Util.GetTrimmedString(text, bottomTextBlock.ActualWidth, bottomTextBlock, text.Length - displayName.Length);
                if (text != bottomTextBlock.Text)
                {
                    BottomToolTip_TextBlock.Text = displayName;
                    BottomToolTip.Visibility = Visibility.Visible;
                }
                else
                {
                    BottomToolTip.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void UseTheScrollViewerScrolling(FrameworkElement fElement)
        {
            fElement.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                fElement.RaiseEvent(eventArg);
            };
        }

        private void SBkUpWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            if (_windowHandle != IntPtr.Zero)
            {
                NativeMethods.SetWindowLong(_windowHandle, NativeMethods.GWL_STYLE, NativeMethods.GetWindowLong(_windowHandle, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_MAXIMIZEBOX);

                if (WindowStartupLocation == WindowStartupLocation.CenterOwner)
                {
                    NativeMethods.SetWindowLong(_windowHandle, NativeMethods.GWL_STYLE, NativeMethods.GetWindowLong(_windowHandle, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_MINIMIZEBOX);
                }
            }

            AdjustLocation();
        }

        private void SBkUpWindow_Closed(object sender, EventArgs e)
        {
            IsClosing = true;

            var viewModel = DataContext as ViewModel.SBkUpViewModel;
            viewModel.WaitLoadWinzipSharedService();
            viewModel.DisposeJobManagement();

            WzSBkUpUI.settings.MainWindowTop = this.Top;
            WzSBkUpUI.settings.MainWindowLeft = this.Left;
            WzSBkUpUI.settings.MainWindowHeight = this.Height;
            WzSBkUpUI.settings.MainWindowWidth = this.Width;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SBkUpWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.Y > 0 && point.Y < FakeTitleBar.ActualHeight &&
                point.X < ActualWidth - MinimizeButton.ActualWidth - CloseButton.ActualWidth)
            {
                DragMove();
            }
        }

        private void AdjustLocation()
        {
            if (WindowStartupLocation != WindowStartupLocation.CenterOwner)
            {
                if (LastPdfWindowLeft != 0 && LastPdfWindowTop != 0)
                {
                    const double leftSpacing = 10;
                    const double topSpacing = 30;
                    Left = LastPdfWindowLeft + leftSpacing;
                    Top = LastPdfWindowTop + topSpacing;
                }
                else if (WzSBkUpUI.settings.MainWindowTop != -1)
                {
                    Left = WzSBkUpUI.settings.MainWindowLeft;
                    Top = WzSBkUpUI.settings.MainWindowTop;
                }
            }

            bool inScreen = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains((int)Left, (int)Top))
                {
                    inScreen = true;
                    break;
                }
            }

            if (!inScreen)
            {
                Left = SystemParameters.WorkArea.Left;
                Top = SystemParameters.WorkArea.Top;
            }
        }

        private void SBkUpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void SBkUpWindow_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        public void ModifybottomText()
        {
            var displayName = Utils.Util.GetDisplayName((JobListBox.SelectedItems[0] as JobItem).Swjf.storeFolder);
            var text = string.Format(Properties.Resources.BOTTOM_TEXT, displayName);
            bottomTextBlock.Text = Utils.Util.GetTrimmedString(text, bottomTextBlock.ActualWidth, bottomTextBlock, text.Length - displayName.Length);
            bottomTextBlock.Visibility = Visibility.Visible;
            if (text != bottomTextBlock.Text)
            {
                BottomToolTip_TextBlock.Text = displayName;
                BottomToolTip.Visibility = Visibility.Visible;
            }
            else
            {
                BottomToolTip.Visibility = Visibility.Collapsed;
            }
        }

        public void EnableSbkupView(bool enable)
        {
            SpecificButton.IsEnabled = enable;
            AllButton.IsEnabled = enable;
            OpenButton.IsEnabled = enable;
            ReplaceExistCheckBox.IsEnabled = enable;
            CreateButton.IsEnabled = enable;
            RunButton.IsEnabled = enable;
            EditButton.IsEnabled = enable;
            DeleteButton.IsEnabled = enable;
            JobAndBackup_BackupListBox.IsEnabled = enable;
            JobAndBackup_JobListBox.IsEnabled = enable;
            JobListBox.IsEnabled = enable;
            restoreTab.IsEnabled = enable;
            backupTab.IsEnabled = enable;

            this.Cursor = enable ? null : System.Windows.Input.Cursors.Wait;
        }

        private void SBkUpWindow_ContentRendered(object sender, EventArgs e)
        {
            this.MaxWidth = this.ActualWidth;
            this.MinWidth = this.ActualWidth;
        }

        private void SBkUpUtilViewWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    e.Handled = true;
                    break;
                default:
                    return;
            }
        }

        public void LoadNagBannerFrame(int nagIndex, int trialDaysRemaining, bool isAlreadyRegistered, string buyNowUrl)
        {
            if (isAlreadyRegistered)
            {
                HideUXNagBannerFrame();
            }
            else
            {
                _subscribePage = new SubscribePageView(this);
                _subscribePage.InitDataContext((TrialPeriodMode)nagIndex, trialDaysRemaining, buyNowUrl);
                this.NagBannerFrame.Navigate(_subscribePage);
            }
        }

        public void LoadGraceBannerFrame(int periodIndex, int daysRemaining, string userEmail)
        {
            _isShowGraceBanner = true;
            _gracePeriodPage = new GracePeriodPageView(this);
            _gracePeriodPage.InitDataContext((GracePeriodMode)periodIndex, daysRemaining, userEmail);
            this.NagBannerFrame.Navigate(_gracePeriodPage);
        }

        private void RefreshNagBanner(int periodIndex, int daysRemaining)
        {
            if (_isShowGraceBanner)
            {
                string userEmail = string.Empty;
                if (WinZipMethods.GetGracePeriodInfo(IntPtr.Zero, ref periodIndex, ref daysRemaining, ref userEmail))
                {
                    _gracePeriodPage?.SetGracePeriodMode((GracePeriodMode)periodIndex, daysRemaining, userEmail);
                }
            }
            else
            {
                _subscribePage?.SetTrialPeriodMode((TrialPeriodMode)periodIndex, daysRemaining);
            }
        }

        public void HideUXNagBannerFrame()
        {
            this.NagBannerFrame.Visibility = Visibility.Collapsed;
            _subscribePage = null;
            _gracePeriodPage = null;
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.IsSelected)
            {
                item.IsSelected = false;
                e.Handled = true;
            }
        }
    }
}
