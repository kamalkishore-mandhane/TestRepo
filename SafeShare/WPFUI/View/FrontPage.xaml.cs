using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for FrontPage.xaml
    /// </summary>
    public partial class FrontPage : BasePage
    {
        private bool _isShowGraceBanner;
        private SubscribePageView _subscribePage;
        private GracePeriodPageView _gracePeriodPage;

        private bool _isDragging;

        public FrontPage()
        {
            InitializeComponent();
        }

        public void InitDataContext()
        {
            var viewModel = new FrontPageViewModel(this, AdjustPaneCursor);
            DataContext = viewModel;
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    Notify(nameof(IsDragging));
                }
            }
        }

        private void SetWpfDragDropEffect(DragEventArgs e)
        {
            if (e.RoutedEvent == DragDrop.DragEnterEvent)
            {
                WpfDragDropHelper.DragEnter(e, this, this);
            }
            else if (e.RoutedEvent == DragDrop.DragOverEvent)
            {
                WpfDragDropHelper.DragOver(e, this);
            }
            else if (e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                WpfDragDropHelper.DragLeave();
            }
        }

        private void DragFileBorder_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.RoutedEvent == DragDrop.DragEnterEvent)
            {
                IsDragging = true;
            }
            else if (e.RoutedEvent == DragDrop.DragLeaveEvent)
            {
                IsDragging = false;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.ADD, string.Empty);
                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    SetWpfDragDropEffect(e);
                }
            }

            e.Handled = true;
        }

        private void DragFileBorder_Drop(object sender, DragEventArgs e)
        {
            IsDragging = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    var fileListPage = new FileListPage();
                    fileListPage.InitDataContext();
                    (fileListPage.DataContext as FileListPageViewModel).ExecuteDragFromExploreTaskCommand(files);
                    MainWindow.Navigate(fileListPage);
                    e.Handled = true;
                }
            }
        }

        private void FrontPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            ArrangeFrontPageAnimation();
            InitDataContext();
            NavigationCommandsManager.RemoveBrowseBackKeyGestures();
            SelectFilesButton.Focus();
            (DataContext as FrontPageViewModel)?.StartTimer();
        }

        private void FrontPageView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;

            NavigationCommandsManager.ResetBrowseBackKeyGestures();
            (DataContext as FrontPageViewModel)?.StopTimer();
        }

        public void AdjustPaneCursor(bool enable)
        {
            this.Cursor = enable ? null : System.Windows.Input.Cursors.Wait;

            SelectFilesButton.IsEnabled = enable;
            DragFileBorder.IsEnabled = enable;
            CloseButton.IsEnabled = enable;

            var frontPageViewModel = DataContext as FrontPageViewModel;
            if (enable && frontPageViewModel != null)
            {
                SelectFilesButton.Focus();
            }

            Mouse.UpdateCursor();
        }

        private void ArrangeFrontPageAnimation()
        {
            // do not animate front page at the first open
            if (WorkFlowManager.IsAppFirstOpen())
            {
                AnimationGrid.Style = null;
            }
            else if (AnimationGrid.Style == null)
            {
                AnimationGrid.Style = Application.Current.TryFindResource("AnimationGridStyle") as Style;
            }
        }

        public void LoadGraceBannerFrame(int periodIndex, int daysRemaining, string userEmail)
        {
            _isShowGraceBanner = true;
            _gracePeriodPage = new GracePeriodPageView(this);
            _gracePeriodPage.InitDataContext((GracePeriodMode)periodIndex, daysRemaining, userEmail);
            NagBannerFrame.Navigate(_gracePeriodPage);
            NagBannerFrame.Visibility = Visibility.Visible;
        }

        public void LoadNagBannerFrame(int nagIndex, int trialDaysRemaining, bool isAlreadyRegistered, string buyNowUrl)
        {
            if (isAlreadyRegistered)
            {
                HideUXNagBannerFrame();
            }
            else
            {
                _isShowGraceBanner = false;
                _subscribePage = new SubscribePageView(this);
                _subscribePage.InitDataContext((TrialPeriodMode)nagIndex, trialDaysRemaining, buyNowUrl);
                NagBannerFrame.Navigate(_subscribePage);
                NagBannerFrame.Visibility = Visibility.Visible;
            }
        }

        public void RefreshNagBanner(int periodIndex, int daysRemaining)
        {
            if (_isShowGraceBanner)
            {
                string userEmail = string.Empty;
                if (WinzipMethods.GetGracePeriodInfo(IntPtr.Zero, ref periodIndex, ref daysRemaining, ref userEmail))
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
    }
}