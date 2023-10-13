using Applets.Common;
using Microsoft.Win32;
using DupFF.WPFUI.Controls;
using DupFF.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using static DupFF.WPFUI.Utils.WinZipMethods;
using DupFF.WPFUI.ViewModel;
using System.Windows.Threading;
using System.Security.Policy;

namespace DupFF.WPFUI.View
{
    public enum MessageId
    {
        ID_REFRESH_UXNAG_BANNER = 13022,
    }

    /// <summary>
    /// Interaction logic for DupFFView.xaml
    /// </summary>
    public partial class DupFFView : BaseWindow
    {
        private IntPtr _windowHandle = IntPtr.Zero;
        private BGTUICategory _viewType = BGTUICategory.Deduplicator;
        private bool _isShowGraceBanner;
        private SubscribePageView _subscribePage;
        private GracePeriodPageView _gracePeriodPage;

        public BGTUICategory ViewType
        {
            get
            {
                return _viewType;
            }
        }

        public static DupFFView MainWindow
        {
            get; private set;
        }

        public IntPtr WindowHandle
        {
            get
            {
                return _windowHandle;
            }
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

        public DupFFView(IntPtr parentWindowHandle, BGTUICategory viewType)
        {
            _viewType = viewType;

            InitializeComponent();
            LastPdfWindowLeft = 0;
            LastPdfWindowTop = 0;
            DataContext = new ViewModel.DupFFViewModel(this, EnableView);

            if (parentWindowHandle != IntPtr.Zero)
            {
                var interop = new WindowInteropHelper(this);
                interop.Owner = parentWindowHandle;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }


            if (Settings.Instance.MainWindowWidth != 0 && Settings.Instance.MainWindowHeight != 0)
            {
                this.Height = Settings.Instance.MainWindowHeight;
                this.Width = Settings.Instance.MainWindowWidth;
                if (WindowStartupLocation != WindowStartupLocation.CenterOwner)
                {
                    this.Top = Settings.Instance.MainWindowTop;
                    this.Left = Settings.Instance.MainWindowLeft;
                }
            }

            MainWindow = this;
            (DataContext as ViewModel.DupFFViewModel).Dispatcher = this.Dispatcher;
            (DataContext as ViewModel.DupFFViewModel).Items.CollectionChanged += Items_CollectionChanged;
            (DataContext as ViewModel.DupFFViewModel).ActionItems.CollectionChanged += ActionItems_CollectionChanged;
            (DataContext as ViewModel.DupFFViewModel).WinZipLoaded += (sender, e) =>
            {
                jobView.Visibility = Visibility.Visible;
            };
        }

        private void ActionItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<ActionItem> list)
            {
                noResultInfo.Visibility = list.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                resultsArray.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                ResultView.Visibility = resultsArray.Visibility;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<DisplayItem> list)
            {
                noBGToolInfo.Visibility = list.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                textArray.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                JobListBox_ScrollViewer.Visibility = textArray.Visibility;
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

        public void EnableView(bool enable)
        {
            AddButton.IsEnabled = enable;
            RunButton.IsEnabled = enable;
            DeleteButton.IsEnabled = enable;
            EditButton.IsEnabled = enable;
            UndoDeleteButton.IsEnabled = enable;
            TakeActionButton.IsEnabled = enable;
            DismissAllButton.IsEnabled = enable;
            DismissButton.IsEnabled = enable;

            ResultView.IsEnabled = enable;
            JobListBox.IsEnabled = enable;
            organizeTab.IsEnabled = enable;
            actionsTab.IsEnabled = enable;

            this.Cursor = enable ? null : System.Windows.Input.Cursors.Wait;
        }

        private void FakeRibbonTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((DataContext as ViewModel.DupFFViewModel).IsWinZipLoaded)
            {
                if (organizeTab.IsSelected)
                {
                    jobView.Visibility = Visibility.Visible;
                    ResultTabView.Visibility = Visibility.Collapsed;
                }
                if (actionsTab.IsSelected)
                {
                    jobView.Visibility = Visibility.Collapsed;
                    ResultTabView.Visibility = Visibility.Visible;
                }
            }
        }

        private void DupFFWindow_Closed(object sender, EventArgs e)
        {
            Settings.Instance.MainWindowTop = this.Top;
            Settings.Instance.MainWindowLeft = this.Left;
            Settings.Instance.MainWindowWidth = this.Width;
            Settings.Instance.MainWindowHeight = this.Height;
        }

        private void DupFFWindow_SourceInitialized(object sender, EventArgs e)
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
                else if (Settings.Instance.MainWindowTop != -1)
                {
                    Left = Settings.Instance.MainWindowLeft;
                    Top = Settings.Instance.MainWindowTop;
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

        private void DupFFWindow_Loaded(object sender, RoutedEventArgs e)
        {
#if !WZ_APPX
            if (WinZipMethods.IsInGracePeriod(IntPtr.Zero))
            {
                // in grace period, load grace banner
                int gracePeriodIndex = 0;
                int graceDaysRemaining = 0;
                string userEmail = string.Empty;

                if (WinZipMethods.GetGracePeriodInfo(IntPtr.Zero, ref gracePeriodIndex, ref graceDaysRemaining, ref userEmail))
                {
                    _isShowGraceBanner = true;
                    _gracePeriodPage = new GracePeriodPageView(this);
                    _gracePeriodPage.InitDataContext((GracePeriodMode)gracePeriodIndex, graceDaysRemaining, userEmail);
                    NagBannerFrame.Navigate(_gracePeriodPage);
                }
            }
            else
            {
                // in normal trial period, load trial banner
                int nagIndex = 0;
                int trialDaysRemaining = 0;
                bool isAlreadyRegistered = false;
                string buyNowUrl = string.Empty;

                if (WinZipMethods.GetTrialPeriodInfo(IntPtr.Zero, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, ref buyNowUrl))
                {
                    _isShowGraceBanner = false;
                    _subscribePage = new SubscribePageView(this);
                    _subscribePage.InitDataContext((TrialPeriodMode)nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
                    NagBannerFrame.Navigate(_subscribePage);
                }
            }
#endif

            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void DupFFWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void JobListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ViewModel.DupFFViewModel).SelectedItems.Clear();
            foreach (var item in JobListBox.SelectedItems)
            {
                (DataContext as ViewModel.DupFFViewModel).SelectedItems.Add(item as DisplayItem);
            }
        }

        private void ResultView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ViewModel.DupFFViewModel).SelectedActionItems.Clear();
            foreach (var item in ResultView.SelectedItems)
            {
                (DataContext as ViewModel.DupFFViewModel).SelectedActionItems.Add(item as ActionItem);
            }
        }

        private void ItemStatusUI_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is ItemStatusUI status)
            {
                switch (status)
                {
                    case ItemStatusUI.Running:
                        textBlock.Text = Properties.Resources.BGT_BUSY;
                        break;
                    case ItemStatusUI.Deleted:
                        textBlock.Text = Properties.Resources.BGT_DEL_PENDING;
                        break;
                    case ItemStatusUI.ExtSched:
                        textBlock.Text = Properties.Resources.BGT_EXT_SCHED;
                        break;
                    case ItemStatusUI.Complete:
                        textBlock.Text = Properties.Resources.BGT_COMPLETE;
                        break;
                    case ItemStatusUI.ReadyToRun:
                        textBlock.Text = Properties.Resources.BGT_READY_TO_RUN;
                        break;
                    default:
                        textBlock.Text = string.Empty;
                        break;
                }
                if (status == ItemStatusUI.Complete)
                {
                    Run run1 = new Run(textBlock.Text + " - ");
                    Hyperlink hyperlink = new Hyperlink(new Run(Properties.Resources.SEE_RESULTS))
                    {
                        NavigateUri = new Uri("https://www.winzip.com/")
                    };
                    hyperlink.Style = this.FindResource("HyperlinkStyle") as Style;
                    hyperlink.TextDecorations = null;
                    hyperlink.RequestNavigate += new RequestNavigateEventHandler(Hyperlink_RequestNavigate);
                    textBlock.Inlines.Clear();
                    textBlock.Inlines.Add(run1);
                    textBlock.Inlines.Add(hyperlink);
                }

                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                var model = DataContext as ViewModel.DupFFViewModel;
                if (link.Tag is string guid)
                {
                    foreach (var item in model.ActionItems)
                    {
                        if (item.Guid == guid)
                        {
                            model.RibbonTabViewModel.ViewModelCommands.ExecuteTask(() =>
                            {
                                if (WinZipMethods.BGTTakeAction(DupFFView.MainWindow.WindowHandle, guid))
                                {
                                    var str = WinZipMethods.GetBGTRNInfos(DupFFView.MainWindow.WindowHandle, DupFFView.MainWindow.ViewType);
                                    model.RibbonTabViewModel.UpdateActionItem(str);
                                }
                            });
                            break;
                        }
                    }
                }
                else
                {
                    model.SelectedTabIndex = 1;
                }
            }
        }

        private void ButtonManageFinders_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ViewModel.DupFFViewModel;
            model.SelectedTabIndex = 0;
        }

        private void JobListBox_ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void DupFFWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

        private void DatePicker_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    var template = (sender as System.Windows.Controls.DatePicker).Template;
                    var popup = (System.Windows.Controls.Primitives.Popup)template.FindName("PART_Popup", sender as System.Windows.Controls.DatePicker);
                    if (popup != null && popup.Child != null && popup.Child.IsVisible)
                    {
                        e.Handled = true;
                    }
                    break;
                default:
                    return;
            }
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
    }
}
