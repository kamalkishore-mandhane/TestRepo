using Applets.Common;
using ImgUtil.Util;
using ImgUtil.WPFUI.Controls;
using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ImgUtilView.xaml
    /// </summary>
    public partial class ImgUtilView : BaseWindow
    {
        private bool _isShowGraceBanner;
        private SubscribePageView _subscribePage;
        private GracePeriodPageView _gracePeriodPage;

        private double _scale;
        private IntPtr _windowHandle;
        private bool _isCropping;
        private bool _isViewLoaded;
        private Point? _lastMousePosition;
        private Point? _lastCenterPosition;

        private bool _isGrabing = false;
        private Cursor _handFreeCursor = null;
        private Cursor _handGrabCursor = null;
        private Point _grabMoveMousePoint;
        private double _grabMoveHorizontalOffset = 0;
        private double _grabMoveVerticleOffset = 0;

        private CroppingAdorner _curAdorner;
        private ImageService _imageService;
        private Dictionary<int, double> _zoomInOutDiction = new Dictionary<int, double>();

        public ImgUtilView()
        {
            App.InitApp();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Application.Current.MainWindow = this;
            _isViewLoaded = false;
            IsClosing = false;
            LastWindowRect = new Rect(0, 0, 0, 0);
            ShowActivated = false;
            ImgUtilSettings.LoadImgUtilSettingsXML();
        }

        public void InitDataContext()
        {
            _isCropping = false;
            _imageService = new ImageService(Dispatcher);
            var viewModel = new ImgUtilViewModel(this, _imageService, AdjustPaneCursor);
            DataContext = viewModel;
        }

        private const int WM_NCLBUTTONUP = 0x00A2;
        private const int HTCLOSE = 20;
        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCLBUTTONUP)
            {
                int ctlIdx = wParam.ToInt32();
                switch (ctlIdx)
                {
                    case HTCLOSE:
                        this.Close();
                        break;
                    case HTMINBUTTON:
                        if (!EnvironmentService.IsCalledByWinZip)
                            this.WindowState = WindowState.Minimized;
                        break;
                    case HTMAXBUTTON:
                        if (this.WindowState == WindowState.Maximized)
                            this.WindowState = WindowState.Normal;
                        else
                            this.WindowState = WindowState.Maximized;
                        break;
                    default:
                        break;
                }
            }
#if !WZ_APPX
            else if (msg == NativeMethods.WM_COMMAND)
            {
                int operation = wParam.ToInt32();
                switch (operation)
                {
                    case (int)NativeMethods.MessageId.ID_REFRESH_UXNAG_BANNER:
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


        private bool IsSizeToFit => ViewZoomInOutComboBox.SelectedIndex == ViewZoomInOutComboBox.Items.Count - 1;

        private Cursor HandFreeCursor => _handFreeCursor ?? (_handFreeCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/PanHandFree.cur", UriKind.RelativeOrAbsolute)).Stream));

        private Cursor HandGrabCursor => _handGrabCursor ?? (_handGrabCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/PanHandHold.cur", UriKind.RelativeOrAbsolute)).Stream));

        public IntPtr WindowHandle
        {
            get
            {
                return _windowHandle;
            }
        }

        public bool SafeShareEnabled
        {
            get
            {
                return !RegeditOperation.GetIsEnterprise() || (RegeditOperation.GetIsEnterprise() && RegeditOperation.IsSafeShareEnabled());
            }
        }

        public Rect LastWindowRect { get; set; }

        public bool IsMultipleWindow { get; set; }

        public bool IsClosing { get; private set; }

        private void RefreshNagBanner(int periodIndex, int daysRemaining)
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
            NagBannerFrame.Visibility = Visibility.Collapsed;
            _subscribePage = null;
            _gracePeriodPage = null;
        }

        #region Window Event

        private void ImgUtilView_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            if (EnvironmentService.IsCalledByWinZip)
            {
                DisableMinizeButton();
            }

            AdjustLocation();
        }

        private async void ImgUtilView_Loaded(object sender, RoutedEventArgs e)
        {
#if !WZ_APPX
            if (WinzipMethods.IsInGracePeriod(IntPtr.Zero))
            {
                // in grace period, load grace banner
                int gracePeriodIndex = 0;
                int graceDaysRemaining = 0;
                string userEmail = string.Empty;

                if (WinzipMethods.GetGracePeriodInfo(IntPtr.Zero, ref gracePeriodIndex, ref graceDaysRemaining, ref userEmail))
                {
                    _isShowGraceBanner = true;
                    _gracePeriodPage = new GracePeriodPageView(this);
                    _gracePeriodPage.InitDataContext((GracePeriodMode)gracePeriodIndex, graceDaysRemaining, userEmail);
                    NagBannerFrame.Navigate(_gracePeriodPage);
                    NagBannerFrame.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // in normal trial period, load trial banner
                int nagIndex = 0;
                int trialDaysRemaining = 0;
                bool isAlreadyRegistered = false;
                string buyNowUrl = string.Empty;

                if (WinzipMethods.GetTrialPeriodInfo(IntPtr.Zero, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, ref buyNowUrl))
                {
                    _isShowGraceBanner = false;
                    _subscribePage = new SubscribePageView(this);
                    _subscribePage.InitDataContext((TrialPeriodMode)nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
                    NagBannerFrame.Navigate(_subscribePage);
                    NagBannerFrame.Visibility = Visibility.Visible;
                }
            }
#endif

            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (DataContext is ImgUtilViewModel viewModel)
            {
                viewModel.LoadRecentFilesXML();
            }

            InitViewZoomInOutComboBox();

            // Wait for environment initialize
            AdjustPaneCursor(false);

            // Create InitWinzipLicense Task
            EnvironmentService.InitWinzipLicenseTask = new Task<bool>(() =>
            {
                return EnvironmentService.InitWinzipLicense(WindowHandle);
            });

            bool success = await EnvironmentService.InitEnvironmentTask;
            if (!success)
            {
                Close();
            }
            else
            {
                EnvironmentService.InitWinzipLicenseTask.Start();
                success = await EnvironmentService.InitWinzipLicenseTask;
                if (!success)
                {
                    Close();
                }
            }

            AdjustPaneCursor(true);

            // If user open ImgUtil inside a msix container by Right-Click menu, the newly opened ImgUtil window will not appear in the foreground,
            // nor can it be activated by Activate() or SetForegroundWindow(). For this problem, here is a trick:
            // simulate a key release event, and then call Activate(), the ImgUtil window will be brought to the foreground.

            // For restricts of Activate(), please check: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks

            NativeMethods.keybd_event(NativeMethods.VK_LMENU, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
            Activate();
            Focus();

            _isViewLoaded = true;
        }

        private void ImgUtilView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private async void ImgUtilView_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;

            if (DataContext is ImgUtilViewModel viewModel)
            {
                // Canceled before await
                e.Cancel = true;
                // Because HandleCurrentImageStatusAsync() has synchronous path, so await Task.Yield() here to return execution to caller
                await System.Threading.Tasks.Task.Yield();
                bool canContinue = await viewModel.HandleCurrentImageStatusAsync();

                if (!canContinue)
                {
                    Focus();
                }
                else
                {
                    _imageService.UnloadCurrentImage();

                    if (EnvironmentService.IsCalledByWinZipFilePane && viewModel.IsImageChanged && EnvironmentService.PipeServer != null)
                    {
                        string parameter = "imgchanged" + "\t";
                        if (viewModel.UploadedItem != null)
                        {
                            parameter += ImageHelper.CloudItemToString(viewModel.UploadedItem.Value);
                        }
                        else if (viewModel.CurrentOpenedItem != null)
                        {
                            parameter += ImageHelper.CloudItemToString(viewModel.CurrentOpenedItem.Value);
                        }
                        else
                        {
                            return;
                        }
                        var bytes = System.Text.Encoding.Unicode.GetBytes(parameter);
                        EnvironmentService.PipeServer.Flush();
                        EnvironmentService.PipeServer.Write(bytes, 0, bytes.Length);
                    }

                    Closing -= ImgUtilView_Closing;
                    Close();
                }
            }
        }

        private void ImgUtilView_Closed(object sender, EventArgs e)
        {
            ImgUtilSettings.Instance.WindowPosLeft = Left;
            ImgUtilSettings.Instance.WindowPosTop = Top;
            ImgUtilSettings.Instance.WindowsWidth = Width;
            ImgUtilSettings.Instance.WindowsHeight = Height;
            ImgUtilSettings.Instance.ViewZoomInOutSelectedIndex = ViewZoomInOutComboBox.SelectedIndex;
            ImgUtilSettings.Instance.WindowsState = (int)WindowState;
            ImgUtilSettings.SaveImgUtilSettingsXML();

            if (DataContext is ImgUtilViewModel imgUtilViewModel)
            {
                imgUtilViewModel.LoadRecentFilesXML();
                imgUtilViewModel.SaveRecentFilesXML();
            }

            if (EnvironmentService.IsCalledByWinZip)
            {
                var parent = new WindowInteropHelper(this).Owner;
                if (parent != IntPtr.Zero)
                {
                    NativeMethods.EnableWindow(parent, true);
                    NativeMethods.SetForegroundWindow(parent);
                }
            }
        }

        private void ImgUtilViewWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isViewLoaded && ImgUtilSettings.Instance.WindowsState != -1 && ImgUtilSettings.Instance.WindowsState != (int)WindowState.Minimized)
            {
                WindowState = (WindowState)ImgUtilSettings.Instance.WindowsState;
            }

            if (!_isViewLoaded)
            {
                if (IsMultipleWindow && LastWindowRect.Width != 0 && LastWindowRect.Height != 0)
                {
                    Width = LastWindowRect.Width;
                    Height = LastWindowRect.Height;
                }
                else if (ImgUtilSettings.Instance.WindowsWidth != 0 && ImgUtilSettings.Instance.WindowsHeight != 0)
                {
                    Width = ImgUtilSettings.Instance.WindowsWidth;
                    Height = ImgUtilSettings.Instance.WindowsHeight;
                }
            }

            if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
            {
                return;
            }

            if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
            {
                return;
            }

            if (IsSizeToFit)
            {
                CalScaleForFitToSize();
                if (_curAdorner != null)
                {
                    _curAdorner.UpdateAdorner(_scale);
                    scaleTransform.ScaleX = _curAdorner.CropScale;
                    scaleTransform.ScaleY = _curAdorner.CropScale;
                }
            }
        }

        private void ImgUtilViewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is ImgUtilViewModel imgUtilViewModel) || (_isCropping && e.Key != Key.Escape))
            {
                return;
            }

            switch (e.Key)
            {
                case Key.N:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        imgUtilViewModel.RibbonCommands.CreateFromCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.O:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        imgUtilViewModel.RibbonCommands.OpenCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.S:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        imgUtilViewModel.RibbonCommands.SaveCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.A:
                    if (KeyboardUtil.IsCtrlKeyDown && KeyboardUtil.IsShiftKeyDown)
                    {
                        imgUtilViewModel.RibbonCommands.SaveAsCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.C:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        imgUtilViewModel.RibbonCommands.CopyCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (_curAdorner != null)
                    {
                        RemoveCroppingAdorner();
                    }
                    else
                    {
                        imgUtilViewModel.RibbonCommands.ExitCommand.Execute(null);
                    }
                    e.Handled = true;
                    break;
                default:
                    return;
            }
        }

        private void PreviewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && !_isCropping)
            {
                _lastMousePosition = Mouse.GetPosition(PreviewImage);

                if (e.Delta > 0 && ViewZoomInOutComboBox.SelectedIndex < 15)
                {
                    ViewZoomInOutComboBox.SelectedIndex++;
                }

                if (e.Delta < 0 && ViewZoomInOutComboBox.SelectedIndex > 0)
                {
                    ViewZoomInOutComboBox.SelectedIndex--;
                }

                var centerPos = new Point(PreviewScrollViewer.ViewportWidth / 2.0, PreviewScrollViewer.ViewportHeight / 2.0);
                _lastCenterPosition = PreviewScrollViewer.TranslatePoint(centerPos, PreviewImage);
                e.Handled = true;
            }
        }

        private void PreviewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePosition.HasValue)
                {
                    if (_lastCenterPosition.HasValue)
                    {
                        var centerOfViewport = new Point(PreviewScrollViewer.ViewportWidth / 2, PreviewScrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = PreviewScrollViewer.TranslatePoint(centerOfViewport, PreviewImage);

                        targetBefore = _lastCenterPosition;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePosition;
                    targetNow = Mouse.GetPosition(PreviewImage);

                    _lastMousePosition = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / PreviewImage.ActualWidth;
                    double multiplicatorY = e.ExtentHeight / PreviewImage.ActualHeight;

                    double newOffsetX = PreviewScrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = PreviewScrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    PreviewScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    PreviewScrollViewer.ScrollToVerticalOffset(newOffsetY);
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

        private void PreviewScrollViewer_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.OPEN, string.Empty);
                    SetWpfDragDropEffect(e);
                }
            }

            e.Handled = true;
        }

        private void PreviewScrollViewer_Drop(object sender, DragEventArgs e)
        {
            // drag files to image preview pane
            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Activate();
                        imgUtilViewModel.MainViewCommands.DragFromExplorerCommand.Execute(files[0]);
                    }));
                    e.Handled = true;
                }
            }
        }

        private void PreviewImage_MouseEnter(object sender, MouseEventArgs e)
        {
            double outsideWidth = PreviewScrollViewer.ExtentWidth - PreviewScrollViewer.ViewportWidth;
            double outsideHeight = PreviewScrollViewer.ExtentHeight - PreviewScrollViewer.ViewportHeight;
            if (PreviewImage.IsMouseOver && !_isCropping && (outsideWidth > 0 || outsideHeight > 0) && !_isGrabing)
            {
                Cursor = HandFreeCursor;
            }
        }

        private void PreviewImage_MouseMove(object sender, MouseEventArgs e)
        {
            double outsideWidth = PreviewScrollViewer.ExtentWidth - PreviewScrollViewer.ViewportWidth;
            double outsideHeight = PreviewScrollViewer.ExtentHeight - PreviewScrollViewer.ViewportHeight;
            if (!_isCropping && (outsideWidth > 0 || outsideHeight > 0))
            {
                if (_isGrabing)
                {
                    if (outsideHeight > 0)
                    {
                        PreviewScrollViewer.ScrollToVerticalOffset(_grabMoveVerticleOffset + (_grabMoveMousePoint.Y - e.GetPosition(PreviewScrollViewer).Y));
                    }

                    if (outsideWidth > 0)
                    {
                        PreviewScrollViewer.ScrollToHorizontalOffset(_grabMoveHorizontalOffset + (_grabMoveMousePoint.X - e.GetPosition(PreviewScrollViewer).X));
                    }
                }
                else if (Cursor != HandFreeCursor)
                {
                    Cursor = HandFreeCursor;
                }
            }
            else if (Cursor == HandFreeCursor || Cursor == HandGrabCursor)
            {
                Cursor = null;
            }
        }

        private void PreviewImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Cursor == HandFreeCursor || Cursor == HandGrabCursor)
            {
                Cursor = null;
            }
            _isGrabing = false;
        }

        private void PreviewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double outsideWidth = PreviewScrollViewer.ExtentWidth - PreviewScrollViewer.ViewportWidth;
            double outsideHeight = PreviewScrollViewer.ExtentHeight - PreviewScrollViewer.ViewportHeight;
            if (!_isCropping && (outsideWidth > 0 || outsideHeight > 0) && !_isGrabing)
            {
                _grabMoveMousePoint = e.GetPosition(PreviewScrollViewer);

                if (outsideHeight > 0)
                {
                    _grabMoveVerticleOffset = PreviewScrollViewer.VerticalOffset;
                }

                if (outsideWidth > 0)
                {
                    _grabMoveHorizontalOffset = PreviewScrollViewer.HorizontalOffset;
                }

                Cursor = HandGrabCursor;
                _isGrabing = true;
            }
        }

        private void PreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isGrabing)
            {
                Cursor = HandFreeCursor;
            }
            _isGrabing = false;
        }

        private void RibbonPaneScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollView = sender as ScrollViewer;
            if (scrollView != null)
            {
                if (scrollView.HorizontalOffset >= scrollView.ScrollableWidth)
                {
                    var rightScrollButton = VisualTreeHelperUtils.FindVisualChild<RepeatButton>(RibbonPaneScrollViewer, o => o.Name == "RightScrollRepeatButton");
                    if (rightScrollButton != null)
                    {
                        rightScrollButton.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    var rightScrollButton = VisualTreeHelperUtils.FindVisualChild<RepeatButton>(RibbonPaneScrollViewer, o => o.Name == "RightScrollRepeatButton");
                    if (rightScrollButton != null && rightScrollButton.Visibility != Visibility.Visible)
                    {
                        rightScrollButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ViewZoomInOutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (_zoomInOutDiction.TryGetValue(combobox.SelectedIndex, out double scale) && scale != 0)
            {
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
                _scale = scale;
            }
            else
            {
                CalScaleForFitToSize();
            }

            string zoomRatio = ViewZoomInOutComboBox.Items[combobox.SelectedIndex] as string;
            if (!string.IsNullOrEmpty(zoomRatio))
            {
                TrackHelper.LogImgViewerEvent(zoomRatio);
            }
        }

        private void ImgStartupPane_OpenFile(object sender, RoutedEventArgs e)
        {
            var recentFile = e.OriginalSource as RecentFile;
            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null && recentFile != null)
            {
                imgUtilViewModel.RibbonCommands.OpenRecentCommand.Execute(recentFile);
            }
        }

        private void ImgStartupPane_ChooseFile(object sender, RoutedEventArgs e)
        {
            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null)
            {
                imgUtilViewModel.RibbonCommands.OpenCommand.Execute(null);
            }
        }

        private void ImgStartupPane_SetAsDefault(object sender, RoutedEventArgs e)
        {
            if (DataContext is ImgUtilViewModel imgUtilViewModel)
            {
                imgUtilViewModel.RibbonCommands.WindowsIntegrationCommand.Execute(null);
            }
        }

        private void ImgStartupPane_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.OPEN, string.Empty);
                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    SetWpfDragDropEffect(e);
                }
            }

            e.Handled = true;
        }

        private void ImgStartupPane_Drop(object sender, DragEventArgs e)
        {
            var imgUtilViewModel = DataContext as ImgUtilViewModel;
            if (imgUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Activate();
                        imgUtilViewModel.MainViewCommands.DragFromExplorerCommand.Execute(files[0]);
                    }));
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Private Helper Functions

        private void AdjustLocation()
        {
            if (IsMultipleWindow)
            {
                const double leftSpacing = 10;
                const double topSpacing = 30;
                Left = LastWindowRect.Left + leftSpacing;
                Top = LastWindowRect.Top + topSpacing;
            }
            else if (ImgUtilSettings.Instance.WindowPosLeft != -1 && ImgUtilSettings.Instance.WindowPosTop != -1)
            {
                Left = ImgUtilSettings.Instance.WindowPosLeft;
                Top = ImgUtilSettings.Instance.WindowPosTop;
            }

            bool inScreen = false;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
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

        private void DisableMinizeButton()
        {
            NativeMethods.SetWindowLong(_windowHandle, NativeMethods.GWL_STYLE, NativeMethods.GetWindowLong(_windowHandle, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_MINIMIZEBOX);
        }

        private void InitViewZoomInOutComboBox()
        {
            ViewZoomInOutComboBox.Items.Add("10%");
            ViewZoomInOutComboBox.Items.Add("25%");
            ViewZoomInOutComboBox.Items.Add("50%");
            ViewZoomInOutComboBox.Items.Add("75%");
            ViewZoomInOutComboBox.Items.Add("100%");
            ViewZoomInOutComboBox.Items.Add("110%");
            ViewZoomInOutComboBox.Items.Add("125%");
            ViewZoomInOutComboBox.Items.Add("150%");
            ViewZoomInOutComboBox.Items.Add("175%");
            ViewZoomInOutComboBox.Items.Add("200%");
            ViewZoomInOutComboBox.Items.Add("250%");
            ViewZoomInOutComboBox.Items.Add("300%");
            ViewZoomInOutComboBox.Items.Add("350%");
            ViewZoomInOutComboBox.Items.Add("400%");
            ViewZoomInOutComboBox.Items.Add("500%");
            ViewZoomInOutComboBox.Items.Add("600%");
            ViewZoomInOutComboBox.Items.Add(Properties.Resources.INFO_VIEW_BY_ACTUAL_SIZE);
            ViewZoomInOutComboBox.Items.Add(Properties.Resources.INFO_VIEW_BY_SIZE_TO_FIT);

            _zoomInOutDiction.Add(0, 0.1);
            _zoomInOutDiction.Add(1, 0.25);
            _zoomInOutDiction.Add(2, 0.5);
            _zoomInOutDiction.Add(3, 0.75);
            _zoomInOutDiction.Add(4, 1.0);
            _zoomInOutDiction.Add(5, 1.1);
            _zoomInOutDiction.Add(6, 1.25);
            _zoomInOutDiction.Add(7, 1.5);
            _zoomInOutDiction.Add(8, 1.75);
            _zoomInOutDiction.Add(9, 2.0);
            _zoomInOutDiction.Add(10, 2.5);
            _zoomInOutDiction.Add(11, 3.0);
            _zoomInOutDiction.Add(12, 3.5);
            _zoomInOutDiction.Add(13, 4.0);
            _zoomInOutDiction.Add(14, 5.0);
            _zoomInOutDiction.Add(15, 6.0);
            _zoomInOutDiction.Add(16, 1.0);

            if (ImgUtilSettings.Instance.ViewZoomInOutSelectedIndex != -1)
            {
                ViewZoomInOutComboBox.SelectedIndex = ImgUtilSettings.Instance.ViewZoomInOutSelectedIndex;
            }
            else
            {
                ViewZoomInOutComboBox.SelectedIndex = 17;
            }
            _zoomInOutDiction.TryGetValue(ViewZoomInOutComboBox.SelectedIndex, out _scale);
        }

        private void CalScaleForFitToSize()
        {
            if (!IsSizeToFit)
            {
                return;
            }

            if (DataContext is ImgUtilViewModel imgUtilViewModel && imgUtilViewModel.CurrentPreviewImage != null)
            {
                double widthRatio = (PreviewScrollViewer.ActualWidth - ImageBorder.Margin.Left - ImageBorder.Margin.Right)
                    / (imgUtilViewModel.CurrentPreviewImage.Width + ImageBorder.BorderThickness.Left * 2);
                double heightRatio = (PreviewScrollViewer.ActualHeight - ImageBorder.Margin.Top - ImageBorder.Margin.Bottom)
                    / (imgUtilViewModel.CurrentPreviewImage.Height + ImageBorder.BorderThickness.Top * 2);

                double ratio = Math.Min(widthRatio, heightRatio);

                if (!_isCropping)
                {
                    scaleTransform.ScaleX = ratio;
                    scaleTransform.ScaleY = ratio;
                }

                _scale = ratio;
            }
        }

        #endregion

        #region Public Functions

        public void AdjustPaneCursor(bool enable)
        {
            FakeRibbonTabControl.IsEnabled = enable;
            PreviewScrollViewer.IsEnabled = enable;
            ImgStartupPane.IsEnabled = enable;
            Cursor = enable ? null : Cursors.Wait;
            Mouse.UpdateCursor();
        }

        public void SetWindowLoadingStatus(bool isLoading)
        {
            if (loadingImage.Source == null)
            {
                var bits = WinzipMethods.Is32Bit() ? "32" : "64";
                var source = $"pack://application:,,,/ImgUtil{bits};component/Resources/Loading.gif";

                var image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(source);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(loadingImage, image);
            }

            loadingGrid.Visibility = isLoading ? Visibility.Visible : Visibility.Hidden;
            FakeRibbonTabControl.IsEnabled = !isLoading;
            PreviewScrollViewer.IsEnabled = !isLoading;
        }

        public void AddCroppingAdorner()
        {
            if (DataContext is ImgUtilViewModel viewModel)
            {
                // set image size to fit
                ViewZoomInOutComboBox.SelectedIndex = ViewZoomInOutComboBox.Items.Count - 1;

                FakeRibbonTabControl.IsEnabled = false;
                var layer = AdornerLayer.GetAdornerLayer(ImageBorder);
                var imageInfo = new ImageInfo(viewModel.CurrentPreviewImage.PixelHeight, viewModel.CurrentPreviewImage.PixelWidth, viewModel.CurrentPreviewImage.DpiX, viewModel.CurrentPreviewImage.DpiY);
                _curAdorner = new CroppingAdorner(ImageBorder, imageInfo, this, _scale, PreviewScrollViewer, () =>
                {
                    if (IsSizeToFit)
                    {
                        _curAdorner.UpdateAdorner(_scale);
                        scaleTransform.ScaleX = _curAdorner.CropScale;
                        scaleTransform.ScaleY = _curAdorner.CropScale;
                    }
                });

                layer.Add(_curAdorner);

                _isCropping = true;
            }
        }

        public void RemoveCroppingAdorner()
        {
            if (DataContext is ImgUtilViewModel viewModel)
            {
                var layer = AdornerLayer.GetAdornerLayer(ImageBorder);
                if (_curAdorner != null)
                {
                    _curAdorner.CleanHandlers();
                    layer.Remove(_curAdorner);
                    ImageBorder.Margin = new Thickness(5);
                    _curAdorner = null;
                }

                if (IsSizeToFit)
                {
                    scaleTransform.ScaleX = _scale;
                    scaleTransform.ScaleY = _scale;
                }

                FakeRibbonTabControl.IsEnabled = true;
                PreviewScrollViewer.IsEnabled = true;
                _isCropping = false;

                CalScaleForFitToSize();
            }
        }

        public void DoCrop()
        {
            if (DataContext is ImgUtilViewModel viewModel && _curAdorner != null)
            {
                viewModel.DoCrop(_curAdorner.GetCroppedArea());
            }
            RemoveCroppingAdorner();
        }

        public void CalLastWindowPostion()
        {
            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var appName = Path.GetFileName(moduleFilePath);

            if (appName.Contains(".bin"))
            {
                assemblyName = appName;
            }

            var rawProcesses = Process.GetProcessesByName(assemblyName);
            var processes = FilterAccessDeniedProcesses(rawProcesses);
            if (processes.Length > 1)
            {
                var windowFinder = new MainWindowFinder();
                Array.Sort(processes, (x, y) => x.StartTime.CompareTo(y.StartTime));

                int lastIndex = processes.Length - 2;
                for (int i = lastIndex; i >= 0; i--)
                {
                    int lastWindowProcessId = processes[i].Id;
                    var hwnd = windowFinder.FindMainWindow(lastWindowProcessId);
                    if (hwnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hwnd) && !NativeMethods.IsIconic(hwnd))
                    {
                        var rect = new NativeMethods.RECT();
                        NativeMethods.GetWindowRect(hwnd, out rect);
                        LastWindowRect = new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                        IsMultipleWindow = true;
                        break;
                    }
                }
            }
        }

        private Process[] FilterAccessDeniedProcesses(Process[] processes)
        {
            var filteredProcesses = new List<Process>();
            foreach (Process process in processes)
            {
                try
                {
                    var tryToGetStartTime = process.StartTime;
                    filteredProcesses.Add(process);
                }
                catch (Win32Exception)
                {
                    // catch Win32Exception means it is an abnormal process, just ignore it
                    continue;
                }
            }
            return filteredProcesses.ToArray();
        }

        public void ReCalculateSizeToFit()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => CalScaleForFitToSize()));
        }

        public void AwareCommandLineOperations()
        {
            Dispatcher.InvokeAsync(new Action(async () =>
            {
                await EnvironmentService.InitEnvironmentTask;
                await EnvironmentService.InitWinzipLicenseTask;
                await (DataContext as ImgUtilViewModel)?.DoCommandLineOperationAsync();
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        #endregion
    }
}
