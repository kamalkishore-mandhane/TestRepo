using Applets.Common;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SafeShare.WPFUI.View
{
    public enum MessageId
    {
        ID_REFRESH_UXNAG_BANNER = 13022,
    }

    /// <summary>
    /// Interaction logic for SafeShareView.xaml
    /// </summary>
    public partial class SafeShareView : BaseNavigationWindow
    {
        private IntPtr _windowHandle;
        private IntPtr _parentWndHandle = IntPtr.Zero;
        private bool _isClosing = false;
        private NamedPipeServerStream _pipeServer;
        private FrontPage _frontPage;
        private FileListPage _fileListPage;

        public SafeShareView()
        {
            App.InitApp();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.SourceInitialized += SafeShareView_SourceInitialized;
            Application.Current.MainWindow = this;
            _pipeServer = null;
            _windowHandle = IntPtr.Zero;
            SafeShareSettings.LoadSafeShareSettingsXML();
        }

        public void InitDataContext()
        {
            var viewModel = new SafeShareViewModel(this, AdjustPaneCursor);
            viewModel.Dispatcher = this.Dispatcher;
            DataContext = viewModel;
            FileOperation.MakeGlobalTempDir();

            _frontPage = new FrontPage();
            _frontPage.InitDataContext();
            NavigationService.Navigate(_frontPage);
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

        public IntPtr ParentWndHandle
        {
            get
            {
                return _parentWndHandle;
            }
            set
            {
                if (_parentWndHandle != value)
                {
                    _parentWndHandle = value;
                }
            }
        }

        public IntPtr WinzipSharedServiceHandle
        {
            get;
            set;
        }

        public IntPtr SafeHandle
        {
            get
            {
                if (IsVisible)
                {
                    return _windowHandle;
                }
                else
                {
                    return IntPtr.Zero;
                }
            }
        }

        public NamedPipeServerStream PipeServer
        {
            set
            {
                _pipeServer = value;
            }
        }

        public bool IsClosing
        {
            get
            {
                return _isClosing;
            }
        }

        private void SafeShareView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void SafeShareView_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            AdjustLocation();
        }

        public void AdjustPaneCursor(bool enable)
        {
            this.Cursor = enable ? null : Cursors.Wait;

            var page = NavigationService.Content as Page;
            if (page != null)
            {
                page.IsEnabled = enable;
            }
            else
            {
                if (_frontPage != null)
                {
                    _frontPage.IsEnabled = enable;
                }

                if (_fileListPage != null)
                {
                    _fileListPage.IsEnabled = enable;
                }
            }

            Mouse.UpdateCursor();
        }

        public void ExecuteCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CanExecuteCloseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecutePreviousPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            (e.Parameter as System.Windows.Controls.Page)?.NavigationService.GoBack();
        }

        private void CanExecutePreviousPageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SafeShareView_Closed(object sender, EventArgs e)
        {
            FileOperation.DeleteGlobalTempDir();
            _isClosing = true;
            var viewModel = DataContext as SafeShareViewModel;
            viewModel.WaitLoadWinzipSharedService();

            SafeShareView_Unloaded(null, null);
            viewModel.DisposeJobManagement();

            if (_pipeServer != null)
            {
                _pipeServer.Disconnect();
                _pipeServer.Dispose();
            }
            if (viewModel.CallFrom == ExeFrom.IMGUTIL || viewModel.CallFrom == ExeFrom.PDFUTIL)
            {
                NativeMethods.EnableWindow(ParentWndHandle, true);
                NativeMethods.SetForegroundWindow(ParentWndHandle);
            }
        }

        public void LoadGraceBannerFrame(int periodIndex, int daysRemaining, string userEmail)
        {
            _frontPage.LoadGraceBannerFrame(periodIndex, daysRemaining, userEmail);
        }

        public void LoadNagBannerFrame(int nagIndex, int trialDaysRemaining, bool isAlreadyRegistered, string buyNowUrl)
        {
            _frontPage.LoadNagBannerFrame(nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
        }

        private void SafeShareView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsWindows11OrGreater())
            {
                EnableBlur();
            }
        }

        private void SafeShareView_Unloaded(object sender, RoutedEventArgs e)
        {
            SafeShareSettings.Instance.WindowPosLeft = Left;
            SafeShareSettings.Instance.WindowPosTop = Top;
            SafeShareSettings.Instance.WindowsWidth = Width;
            SafeShareSettings.Instance.WindowsHeight = Height;
            SafeShareSettings.Instance.WindowsState = (int)WindowState;

            SafeShareSettings.SaveSafeShareSettingsXML();

            if (WinzipSharedServiceHandle != IntPtr.Zero)
            {
                WinzipMethods.DestroySession(WinzipSharedServiceHandle);
            }
        }

        private bool IsWindows11OrGreater()
        {
            var os = Environment.OSVersion;
            return (os.Platform == PlatformID.Win32NT) && (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor >= 2) && os.Version.Build >= 22000);
        }

        public void LaunchWithFile(string[] fileList)
        {
            _fileListPage = new FileListPage();
            _fileListPage.InitDataContext();
            (_fileListPage.DataContext as FileListPageViewModel).ExecuteDragFromExploreTaskCommand(fileList);
            this.Navigate(_fileListPage);
            AdjustPaneCursor(false);
        }

        public void FocusSelectFileButton()
        {
            _frontPage.SelectFilesButton.Focus();
        }

        private void SafeShareView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is SafeShareViewModel viewModel
                && viewModel.WinZipSessionCreated)
            {
                bool showDisplay = true;
                if ((this.Content is FileListPage fileListPage && fileListPage.fileListView.Items.Count == 0)
                    || this.Content is FrontPage || this.Content is ExperiencePage)
                {
                    showDisplay = false;
                }

                if (showDisplay && !SimpleMessageWindows.DisplayCloseAppWarningMessage())
                {
                    e.Cancel = true;
                }
            }
        }

        private void SafeShareView_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is SafeShareView safeShareView)
            {
                if (safeShareView.Content is BasePage page)
                {
                    page.Page_KeyDown(sender, e);
                }
            }
        }

        private void SafeShareView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Windows.Rect rect = new System.Windows.Rect(e.NewSize);
            RectangleGeometry gm = new RectangleGeometry(rect, 9, 9);
            ((UIElement)sender).Clip = gm;
        }

        private void AdjustLocation()
        {
            if (SafeShareSettings.Instance.WindowPosTop != -1)
            {
                Left = SafeShareSettings.Instance.WindowPosLeft;
                Top = SafeShareSettings.Instance.WindowPosTop;
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

        private void RefreshNagBanner(int periodIndex, int trialDaysRemaining)
        {
            _frontPage.RefreshNagBanner(periodIndex, trialDaysRemaining);
        }

        #region achieve an acrylic/translucent look

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19

            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        #endregion achieve an acrylic/translucent look
    }
}