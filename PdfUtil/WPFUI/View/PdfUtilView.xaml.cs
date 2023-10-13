using Applets.Common;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Forms;
using Microsoft.Win32;
using PdfUtil.Util;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Model;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PdfUtil.WPFUI.View
{
    public enum MessageId
    {
        ID_REFRESH_UXNAG_BANNER = 13022,
    }

    public class FakeGroup
    {
        private int _represent = -1;
        public List<UIElement> Children { get; set; } = new List<UIElement>();

        public int Represent
        {
            get { return _represent; }
            set
            {
                if (_represent != value % Children.Count)
                {
                    _represent = (value + Children.Count) % Children.Count;
                    for (var i = 0; i < Children.Count; i++)
                    {
                        Children[i].Focusable = _represent == i;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for PdfUtilView.xaml
    /// </summary>
    public partial class PdfUtilView : BaseWindow
    {
        private bool _isShowGraceBanner;
        private SubscribePageView _subscribePage;
        private GracePeriodPageView _gracePeriodPage;

        private const double offset = 0.1f;
        private const double triggerHeight = 15.0f;

        private bool _filesContextMenuIsOpen = false;
        private bool _dragFindControl = false;
        private bool _scrollToBottomAfterUpdate = false;
        private bool _isViewLoaded = false;
        private bool _isPageUpDownChanged = false;
        private bool _isClosing = false;
        private Point? _lastMousePosition;
        private Point? _lastCenterPosition;
        private Point? _lastDragPosition;
        private Point _startPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private ScrollViewer _scrollViewer;
        private ListViewSelector _dragSelector;
        private Caret _curDisplayCaret;
        private int _curCaretIndex = -1;
        private Dictionary<int, double> _zoomInOutDiction = new Dictionary<int, double>();

        private CancellationTokenSource _cts;

        private Point _selectStartPoint;
        private Point _selectEndPoint;
        private Point _findControlMouseDownPoint;
        private Point _mouseLeftDownOnGridSplitterPoint;

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button
        private IntPtr _windowHandle;

        private double lastLeftPaneWidth = 0;
        private double lastRightPaneWidth = 0;
        private System.Drawing.Color highlightColor = System.Drawing.Color.Transparent;

        private NamedPipeServerStream _pipeServer;

        private bool _dragInProgress;
        private bool _dragFromThumbnailList;

        private double _sizeToFitX = 0;
        private double _sizeToFitY = 0;

        private bool _initSignature = false;
        private bool _draggingSignature = false;
        private FrameworkElement _signatureElement;

        private Dictionary<int, Rect> _fieldRects = new Dictionary<int, Rect>();
        private Dictionary<int, Rectangle> _fieldRectangles = new Dictionary<int, Rectangle>();
        private int _currentFieldIndex = -1;
        private Rectangle _lastRectangle;
        private bool _enableForms = true;

        private bool _doNotScrollAfterCommentSelected;

        public PdfUtilView()
        {
            App.InitApp();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.SourceInitialized += PdfUtilView_SourceInitialized;
            Application.Current.MainWindow = this;
            _pipeServer = null;
            _windowHandle = IntPtr.Zero;
            LastPdfWindowLeft = 0;
            LastPdfWindowTop = 0;
            ShowActivated = false;
            PdfUtilSettings.LoadPDFUtilSettingsXML();
        }

        private double MeasureTextWidth(string text, double fontSize = 12, string fontFamily = "SegoeUI")
        {
            FormattedText formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily.ToString()),
            fontSize,
            Brushes.Black
            );
            return formattedText.WidthIncludingTrailingWhitespace;
        }

        public void InitDataContext()
        {
            IsCalledByWinZipFilePane = false;
            IsCalledByWinZipZipPane = false;
            _cts = new CancellationTokenSource();
            var viewModel = new PdfUtilViewModel(this, AdjustPaneCursor);
            viewModel.ThumbnailListView = this.ThumbnailListView;
            viewModel.BookmarkTreeView = this.BookMarkTree;
            viewModel.PreviewImage = this.PreviewImage;
            viewModel.Dispatcher = this.Dispatcher;

            viewModel.ScrollVerticalOffsetEvent += ViewModel_ScrollVerticalOffsetEvent;
            viewModel.PageUpdateFinishEvent += ViewModel_PageUpdateFinishEvent;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.PreviewImage.SizeChanged += PreviewImage_SizeChanged;
            viewModel.CurrentPdfDocumentChangedEventEvent += ViewModel_CurrentPdfDocumentChanged;
            DataContext = viewModel;

            FileOperation.MakeGlobalTempDir();
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
                        if (!IsCalledByWinZip)
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

        private void PreviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var model = DataContext as PdfUtilViewModel;
            if (model == null || model.CurPreviewIconItem == null)
            {
                return;
            }

            var page = model.CurPreviewIconItem.GetPage();
            if (page == null)
            {
                return;
            }

            if (_enableForms && (_currentFieldIndex < 0 || sender != null))
            {
                if (FormEditCanvas.Tag is Action action)
                {
                    action();
                    FormEditCanvas.Tag = null;
                }

                CleanUpForms();

                try
                {
                    DistinguishForms();
                }
                catch
                {
                    _enableForms = false;
                    CleanUpForms();
                }
            }

            if (model.SearchSet.ContainsKey(model.CurPreviewIconItem.GetPage()))
            {
                model.SearchSet.Remove(page);
            }

            if (findReplaceControl.Visibility == Visibility.Visible)
            {
                model.ExecuteFindCommand(findReplaceControl.FindWord, false, true, page);
            }

            if (e != null)
            {
                UpdateCommentsOnCanvas();
            }
        }

        private Rectangle HighlightFormRectangle(int index, Field field)
        {
            var border = new Rectangle();
            border.Focusable = true;
            border.Stroke = (Brush)FindResource("Brush.Form.Highlight.Border");
            border.Stroke.Opacity = 0.8;
            border.StrokeThickness = 2;
            border.Fill = (Brush)FindResource("Brush.Form.Highlight.Fill");
            border.Fill.Opacity = 0.4;
            border.GotFocus += (sender, e) =>
            {
                _lastRectangle = border;
                if (FormEditCanvas.Children.Count > 0)
                {
                    FormEditCanvas.Tag = null;
                    FormEditCanvas.Children.Clear();
                    FormEditCanvas.Visibility = Visibility.Collapsed;
                }
                border.Stroke = (Brush)FindResource("Brush.TabItem.Selected.Border");
            };
            border.LostFocus += (sender, e) =>
            {
                border.Stroke = (Brush)FindResource("Brush.Form.Highlight.Border");
            };
            border.KeyUp += (sender, e) =>
            {
                if (e.Key == Key.Enter
                    || e.Key == Key.Space && (field is CheckboxField || field is RadioButtonOptionField))
                {
                    _currentFieldIndex = index;
                    FieldAction(field);
                }
            };
            border.PreviewKeyDown += (sender, e) =>
            {
                if (border.Tag is FakeGroup group && group.Children.Count > 1)
                {
                    if (e.Key == Key.Down)
                    {
                        group.Represent += 1;
                        group.Children[group.Represent].Focus();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Up)
                    {
                        group.Represent -= 1;
                        group.Children[group.Represent].Focus();
                        e.Handled = true;
                    }
                }
            };

            return border;
        }

        private void DistinguishForms()
        {
            var model = DataContext as PdfUtilViewModel;
            if (model == null || model.CurPreviewIconItem == null)
            {
                return;
            }

            var page = model.CurPreviewIconItem.GetPage();
            if (page == null)
            {
                return;
            }

            var pageAsposeRect = page.GetPageRect(true);
            var pageRect = new Rect(pageAsposeRect.LLX, pageAsposeRect.LLY, pageAsposeRect.Width, pageAsposeRect.Height);
            double widthratio = model.PreviewPageSize.X / pageRect.Width;
            double heightRatio = model.PreviewPageSize.Y / pageRect.Height;
            var list = model.CurrentPdfDocument.Form.Fields.ToList();

            var tuples = new List<Tuple<Rect, Rectangle>>();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.PageIndex != model.PreviewPageNumber)
                {
                    continue;
                }

                var existPdfAction = false;
                var t = item.Actions.GetType();
                PropertyInfo[] PropertyList = t.GetProperties();
                foreach (PropertyInfo pi in PropertyList)
                {
                    if (pi.PropertyType.Name == nameof(PdfAction))
                    {
                        existPdfAction |= pi.GetValue(item.Actions, null) != null;
                    }
                    if (existPdfAction)
                    {
                        break;
                    }
                }

                if (existPdfAction ||
                    item.ReadOnly ||
                    item.Flags.HasFlag(Aspose.Pdf.Annotations.AnnotationFlags.Hidden) ||
                    item.Flags.HasFlag(Aspose.Pdf.Annotations.AnnotationFlags.ReadOnly) ||
                    item.Flags.HasFlag(Aspose.Pdf.Annotations.AnnotationFlags.NoView) ||
                    (!(item is TextBoxField) && !(item is CheckboxField) && !(item is ChoiceField) && !(item is RadioButtonOptionField)))
                {
                    continue;
                }

                var itemRect = item.GetRectangle(true);
                var point = new Point((itemRect.LLX - pageRect.Left) * widthratio, (pageRect.Bottom - itemRect.URY) * heightRatio);
                var rect = new Rect(point, new Size(itemRect.Width * widthratio, itemRect.Height * heightRatio));

                var border = HighlightFormRectangle(i, item);
                border.Width = rect.Width;
                border.Height = rect.Height;
                border.Tag = item;
                _fieldRects.Add(i, rect);
                _fieldRectangles.Add(i, border);

                tuples.Add(new Tuple<Rect, Rectangle>(rect, border));
                Canvas.SetLeft(border, rect.X);
                Canvas.SetTop(border, rect.Y);
            }
            tuples = tuples.OrderBy(n => (int)(n.Item1.Y)).ToList();
            int y = 0;
            for (int i = 0; i < tuples.Count; i++)
            {
                if (y == 0 || tuples[i].Item1.Y - y > 10)
                {
                    y = (int)tuples[i].Item1.Y;
                    continue;
                }
                var tempRect = tuples[i].Item1;
                tempRect.Y = y;
                tuples[i] = new Tuple<Rect, Rectangle>(tempRect, tuples[i].Item2);
            }
            tuples = tuples.OrderBy(n => (int)n.Item1.Y).ThenBy(n => (int)n.Item1.X).ToList();
            foreach (var tuple in tuples)
            {
                FormPersistenceCanvas.Children.Add(tuple.Item2);
            }
            foreach (var tuple in tuples)
            {
                var item = tuple.Item2.Tag;

                if (item is FakeGroup)
                {
                    continue;
                }

                if (item is RadioButtonOptionField rbof && rbof.Parent is RadioButtonField rbf)
                {
                    var groupList = tuples.FindAll(s => s.Item2.Tag is RadioButtonOptionField && (s.Item2.Tag as RadioButtonOptionField).Parent?.FullName == rbf.FullName);
                    var group = new FakeGroup();
                    foreach (var groupItem in groupList)
                    {
                        group.Children.Add(groupItem.Item2);
                        groupItem.Item2.Tag = group;
                    }
                    group.Represent = 0;
                }
                else
                {
                    tuple.Item2.Tag = null;
                }
            }
        }

        private void CleanUpForms()
        {
            _currentFieldIndex = -1;
            _fieldRects.Clear();
            _fieldRectangles.Clear();
            FormPersistenceCanvas.Children.Clear();
            FormEditCanvas.Children.Clear();
            FormEditCanvas.Tag = null;
            FormEditCanvas.Visibility = Visibility.Collapsed;
        }

        private void ViewModel_CurrentPdfDocumentChanged()
        {
            _enableForms = true;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = DataContext as PdfUtilViewModel;
            if (model.PreviewPageImage != null && e.PropertyName == nameof(model.PreviewPageImage) && model.CurPreviewIconItem != null)
            {
                PreviewImage_SizeChanged(null, null);
            }
        }

        public void CalLastWindowPostion()
        {
            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var appName = System.IO.Path.GetFileName(moduleFilePath);

            if (appName.Contains(".bin"))
            {
                assemblyName = appName;
            }

            var rawProcesses = Process.GetProcessesByName(assemblyName);
            var processes = FilterAccessDeniedProcesses(rawProcesses);
            if (processes.Length > 1)
            {
                var windowFinder = new MainWindowFinder();
                Array.Sort(processes, ProcessStartTimeCompare);

                int lastIndex = processes.Length - 2;
                for (int i = lastIndex; i >= 0; i--)
                {
                    int lastWindowProcessId = processes[i].Id;
                    var hwnd = windowFinder.FindMainWindow(lastWindowProcessId);
                    if (hwnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hwnd) && !NativeMethods.IsIconic(hwnd))
                    {
                        var rect = new NativeMethods.RECT();
                        NativeMethods.GetWindowRect(hwnd, out rect);
                        LastPdfWindowLeft = rect.left;
                        LastPdfWindowTop = rect.top;
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

        private int ProcessStartTimeCompare(Process firstProcess, Process secondProcess)
        {
            return firstProcess.StartTime.CompareTo(secondProcess.StartTime);
        }

        private void ViewModel_ScrollVerticalOffsetEvent(double offset)
        {
            PreViewScrollViewer.ScrollToVerticalOffset(offset);
        }

        public void ViewModel_PageUpdateFinishEvent()
        {
            if (!_isPageUpDownChanged)
            {
                return;
            }

            if (_scrollToBottomAfterUpdate)
            {
                PreViewScrollViewer.ScrollToBottom();
                _scrollToBottomAfterUpdate = false;
            }
            else
            {
                PreViewScrollViewer.ScrollToTop();
            }
            _isPageUpDownChanged = false;
        }

        public CancellationTokenSource Cts
        {
            get
            {
                return _cts;
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                return _windowHandle;
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

        public bool IsCalledByWinZip
        {
            get
            {
                return IsCalledByWinZipFilePane || IsCalledByWinZipZipPane;
            }
        }

        public bool IsCalledByWinZipFilePane
        {
            get;
            set;
        }

        public bool IsCalledByWinZipZipPane
        {
            get;
            set;
        }

        public bool SafeShareEnabled
        {
            get
            {
                return !RegeditOperation.GetIsEnterprise() || (RegeditOperation.GetIsEnterprise() && RegeditOperation.IsSafeShareEnabled());
            }
        }

        public int CurCaretIndex
        {
            get
            {
                return _curCaretIndex;
            }
            set
            {
                if (_curCaretIndex != value)
                {
                    _curCaretIndex = value;
                    HiddenCurDisplayCaret();
                }
            }
        }

        public bool IsPageUpdating
        {
            get;
            set;
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

        public bool IsClosing
        {
            get
            {
                return _isClosing;
            }
        }

        public void ShowFilesMenuButtonContextMenu()
        {
            if (!_filesContextMenuIsOpen)
            {
            }
        }

        private void DisableMinizeButton()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                NativeMethods.SetWindowLong(_windowHandle, GWL_STYLE, NativeMethods.GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
            }
        }

        private void PdfUtilView_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            if (IsCalledByWinZip)
            {
                DisableMinizeButton();
            }

            AdjustLocation();
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

        public void HideUXNagBannerFrame()
        {
            this.NagBannerFrame.Visibility = Visibility.Collapsed;
            _subscribePage = null;
            _gracePeriodPage = null;
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

        private void PdfUtilView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (PdfUtilSettings.Instance.IsTabsControlHidden || PdfUtilSettings.Instance.TabControlsWidth != 0)
            {
                if (PdfUtilSettings.Instance.IsTabsControlHidden)
                {
                    TabControlsStackGrid.Visibility = Visibility.Collapsed;
                    mainWindowsFirstColumn.MinWidth = 0;
                }

                mainWindowsFirstColumn.Width = new GridLength(PdfUtilSettings.Instance.TabControlsWidth);
            }

            // init comment pane width and comment sort combobox
            mainWindowsLastColumn.MinWidth = 0;
            mainWindowsLastColumn.Width = new GridLength(0);
            InitCommentSortComboBox();

            InitViewZoomInOutComboBox();
            SetViewPageTextBox(0);
            _adorner = null;
            _layer = null;
            _scrollViewer = GetFirstVisualChild<ScrollViewer>(ThumbnailListView);
            IconThumbnailManager.Init();

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.LoadRecentFilesXML();
                pdfUtilViewModel.LoadSignaturesFromLocal();
            }

            if (PdfUtilSettings.Instance.ThumbnailZoom >= thumbnailSlider.Minimum && PdfUtilSettings.Instance.ThumbnailZoom <= thumbnailSlider.Maximum)
            {
                pdfUtilViewModel.ThumbnailZoom = PdfUtilSettings.Instance.ThumbnailZoom;
            }

            // If user open PdfUtil inside a msix container by Right-Click menu, the newly opened PdfUtil window will not appear in the foreground,
            // nor can it be activated by Activate() or SetForegroundWindow(). For this problem, here is a trick:
            // simulate a key release event, and then call Activate(), the PdfUtil window will be brought to the foreground.

            // For restricts of Activate(), please check: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks

            NativeMethods.keybd_event(NativeMethods.VK_LMENU, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
            Activate();

            PdfStartupPane.Focus();

            _isViewLoaded = true;
        }

        private void PdfUtilView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            _adorner = null;
            _layer = null;
            PdfUtilSettings.Instance.WindowPosLeft = Left;
            PdfUtilSettings.Instance.WindowPosTop = Top;
            PdfUtilSettings.Instance.WindowsWidth = Width;
            PdfUtilSettings.Instance.WindowsHeight = Height;
            PdfUtilSettings.Instance.TabControlsWidth = mainWindowsFirstColumn.Width.Value;
            PdfUtilSettings.Instance.IsTabsControlHidden = mainWindowsFirstColumn.Width.Value == 0;
            PdfUtilSettings.Instance.ViewZoomInOutSelectedIndex = ViewZoomInOutComboBox.SelectedIndex;
            PdfUtilSettings.Instance.WindowsState = (int)WindowState;
            PdfUtilSettings.Instance.ThumbnailZoom = pdfUtilViewModel.ThumbnailZoom;

            PdfUtilSettings.SavePDFUtilSettingsXML();

            if (WinzipSharedServiceHandle != IntPtr.Zero)
            {
                WinzipMethods.DestroySession(WinzipSharedServiceHandle);
                WinzipSharedServiceHandle = IntPtr.Zero;
            }

            IconThumbnailManager.ShutDown();
            CancelLoadExecuteAction();
            FileOperation.DeleteGlobalTempDir();

            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.LoadRecentFilesXML();
                pdfUtilViewModel.SaveRecentFilesXML();
            }
        }

        private void PdfUtilView_Closed(object sender, EventArgs e)
        {
            _isClosing = true;
            TrackHelper.LogAddBlankPageEvent(TrackHelper.TrackHelperInstance.AddBlankPageCount);
            TrackHelper.TrackHelperInstance.AddBlankPageCount = 0;

            var viewModel = DataContext as PdfUtilViewModel;
            viewModel.WaitLoadWinzipSharedService();

            PdfUtilView_Unloaded(null, null);
            viewModel.DisposeJobManagement();

            if (IsCalledByWinZip && _pipeServer != null)
            {
                _pipeServer.Disconnect();
                _pipeServer.Dispose();

                var parent = new WindowInteropHelper(this).Owner;
                if (parent != IntPtr.Zero)
                {
                    NativeMethods.EnableWindow(parent, true);
                    NativeMethods.SetForegroundWindow(parent);
                }
            }
        }

        public bool ExecuteSaveToZip(string filePath, bool addPreviewPdf, bool removeDirty)
        {
            var viewModel = DataContext as PdfUtilViewModel;
            if (addPreviewPdf)
            {
                if (viewModel != null)
                {
                    var curPdfFileName = System.IO.Path.GetFileName(viewModel.CurrentPdfFileName);
                    if (viewModel.IsNewPDF && viewModel.IsDocChanged)
                    {
                        curPdfFileName = System.IO.Path.GetFileNameWithoutExtension(curPdfFileName);
                        curPdfFileName = new NameNewPdfDialog(this).RunDialog(curPdfFileName);
                        if (string.IsNullOrEmpty(curPdfFileName))
                        {
                            return false;
                        }
                    }

                    var newPdfFilePath = System.IO.Path.Combine(FileOperation.GlobalTempDir, curPdfFileName);
                    using (var stream = File.Create(newPdfFilePath))
                    {
                        lock (IconSouceImage.LockLoadPdfThumbnail)
                        {
                            viewModel.CurrentPdfDocument.Save(stream);
                            EDPHelper.SyncEnterpriseId(viewModel.CurrentPdfFileName, newPdfFilePath);
                        }
                    }

                    if (File.Exists(newPdfFilePath))
                    {
                        viewModel.CurrentPdfFileName = newPdfFilePath;

                        var tempFolderName = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                        var tempZipFileName = System.IO.Path.Combine(tempFolderName, curPdfFileName);
                        EDPHelper.FileCopy(newPdfFilePath, tempZipFileName);
                        filePath = tempFolderName + "*";
                    }
                }
            }

            bool ret;
            try
            {
                string parameter = _windowHandle.ToInt64().ToString();
                parameter = parameter + "\t" + filePath;
                var bytes = System.Text.Encoding.Unicode.GetBytes(parameter);

                _pipeServer.Flush();
                _pipeServer.Write(bytes, 0, bytes.Length);

                byte[] resByte = new byte[10];

                _pipeServer.Read(resByte, 0, 10);

                string resString = System.Text.Encoding.Unicode.GetString(resByte);
                ret = int.Parse(resString) == 0 ? false : true;
            }
            catch (ObjectDisposedException)
            {
                Close();
                return false;
            }
            catch
            {
                ret = false;
            }

            if (ret && removeDirty && (viewModel.IsNewPDF || viewModel.IsDocChanged))
            {
                viewModel.ResetDocChangedState();
            }

            return ret;
        }

        public bool CheckCurrentArchiveIsReadOnly()
        {
            bool ret;
            try
            {
                string parameter = "CheckCurrentArchiveIsReadOnly";
                parameter += "\t";
                var bytes = System.Text.Encoding.Unicode.GetBytes(parameter);

                _pipeServer.Flush();
                _pipeServer.Write(bytes, 0, bytes.Length);

                byte[] resByte = new byte[10];

                _pipeServer.Read(resByte, 0, 10);

                string resString = System.Text.Encoding.Unicode.GetString(resByte);
                ret = int.Parse(resString) == 0 ? false : true;
            }
            catch (ObjectDisposedException)
            {
                Close();
                ret = false;
            }
            catch
            {
                ret = true;
            }

            return ret;
        }

        private void PdfUtilViewWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isViewLoaded && PdfUtilSettings.Instance.WindowsState != -1 && PdfUtilSettings.Instance.WindowsState == (int)WindowState.Maximized)
            {
                WindowState = (WindowState)PdfUtilSettings.Instance.WindowsState;
            }
            else if (!_isViewLoaded && PdfUtilSettings.Instance.WindowsWidth != 0 && PdfUtilSettings.Instance.WindowsHeight != 0)
            {
                Width = PdfUtilSettings.Instance.WindowsWidth;
                Height = PdfUtilSettings.Instance.WindowsHeight;
            }

            if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
            {
                return;
            }

            SetSignatureBarMargin();

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.CurPreviewScrollVerticalOffset = PreViewScrollViewer.VerticalOffset;
            }
        }

        private void AdjustLocation()
        {
            if (LastPdfWindowLeft != 0 && LastPdfWindowTop != 0)
            {
                const double leftSpacing = 10;
                const double topSpacing = 30;
                Left = LastPdfWindowLeft + leftSpacing;
                Top = LastPdfWindowTop + topSpacing;
            }
            else if (PdfUtilSettings.Instance.WindowPosTop != -1)
            {
                Left = PdfUtilSettings.Instance.WindowPosLeft;
                Top = PdfUtilSettings.Instance.WindowPosTop;
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

        private void CancelLoadExecuteAction()
        {
            if (_cts.Token.CanBeCanceled && !_cts.Token.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        private void TabControls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && e.AddedItems.Count > 0)
            {
                var removedItem = e.RemovedItems[0] as TabItem;
                var addedItem = e.AddedItems[0] as TabItem;
                if (removedItem != null && addedItem != null)
                {
                    var removedItemHeader = removedItem.Header as string;
                    var addedItemHeader = addedItem.Header as string;
                    if (removedItemHeader.Equals("Bookmarks") && addedItemHeader.Equals("Thumbnails"))
                    {
                        var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                        if (pdfUtilViewModel != null)
                        {
                            pdfUtilViewModel.RootBookmarkTree.DeSelectOther(null, true);
                        }
                    }
                }
            }
        }

        private void FilesMenu_Closed(object sender, RoutedEventArgs e)
        {
            _filesContextMenuIsOpen = false;
        }

        private void FilesMenu_Initialized(object sender, EventArgs e)
        {

        }

        private void FilesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFilesMenuButtonContextMenu();
        }

        private void GridSplitter_DragStarted(object sender, DragStartedEventArgs e)
        {
            var splitter = sender as GridSplitter;
            if (splitter != null)
            {
                if (splitter.Tag.Equals("LeftSplitter"))
                {
                    if (TabControlsStackGrid.Visibility == Visibility.Collapsed)
                    {
                        splitter.CancelDrag();
                    }
                }
                else
                {
                    if (CommentControlsGrid.Visibility == Visibility.Collapsed)
                    {
                        splitter.CancelDrag();
                    }
                }
            }
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e)
        {
            var splitter = sender as GridSplitter;
            if (splitter != null)
            {
                if (splitter.Tag.Equals("LeftSplitter"))
                {
                    splitter.Cursor = TabControlsStackGrid.Visibility == Visibility.Collapsed ? Cursors.Arrow : Cursors.SizeWE;
                }
                else
                {
                    splitter.Cursor = CommentControlsGrid.Visibility == Visibility.Collapsed ? Cursors.Arrow : Cursors.SizeWE;
                }
            }
        }

        private void GridSplitter_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseLeftDownOnGridSplitterPoint = e.GetPosition(PdfUtilViewWindow);
        }

        private void GridSplitter_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var splitter = sender as GridSplitter;
            if (splitter != null)
            {
                if (splitter.Tag.Equals("LeftSplitter"))
                {
                    if (splitter.Cursor == Cursors.Arrow)
                    {
                        CollapseThumnailPane();
                    }
                    else if (splitter.Cursor == Cursors.SizeWE)
                    {
                        var pos = e.GetPosition(PdfUtilViewWindow);
                        if (pos == _mouseLeftDownOnGridSplitterPoint)
                        {
                            CollapseThumnailPane();
                        }
                    }
                }
                else
                {
                    var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                    if (pdfUtilViewModel != null)
                    {
                        if (splitter.Cursor == Cursors.Arrow)
                        {
                            pdfUtilViewModel.IsCommentPaneShow = !pdfUtilViewModel.IsCommentPaneShow;
                        }
                        else if (splitter.Cursor == Cursors.SizeWE)
                        {
                            var pos = e.GetPosition(PdfUtilViewWindow);
                            if (pos == _mouseLeftDownOnGridSplitterPoint)
                            {
                                pdfUtilViewModel.IsCommentPaneShow = !pdfUtilViewModel.IsCommentPaneShow;
                            }
                        }
                    }
                }
            }

            _mouseLeftDownOnGridSplitterPoint.X = 0;
            _mouseLeftDownOnGridSplitterPoint.Y = 0;
        }

        private void CollapseThumnailPane()
        {
            if (TabControlsStackGrid.Visibility == Visibility.Visible)
            {
                TabControlsStackGrid.Visibility = Visibility.Collapsed;
                lastLeftPaneWidth = mainWindowsFirstColumn.Width.Value;
                PdfUtilSettings.Instance.TabsControlLastLeftPaneWidth = mainWindowsFirstColumn.Width.Value;
                mainWindowsFirstColumn.MinWidth = 0;
                mainWindowsFirstColumn.Width = new GridLength(0);
            }
            else
            {
                TabControlsStackGrid.Visibility = Visibility.Visible;
                mainWindowsFirstColumn.MinWidth = 260;
                mainWindowsFirstColumn.Width = new GridLength(lastLeftPaneWidth != 0 ? lastLeftPaneWidth : PdfUtilSettings.Instance.TabsControlLastLeftPaneWidth);
            }
        }

        private void PreviewPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSignatureBarMargin();
        }

        private void PreViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!IsPageUpdating)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _lastMousePosition = Mouse.GetPosition(ImageViewerGrid);
                    int sizeToFitXIndex = 17;
                    if (ViewZoomInOutComboBox.SelectedIndex == sizeToFitXIndex)
                    {
                        var fitIndx = CalScaleForFitToSizeIndex();
                        if (e.Delta > 0)
                        {
                            ViewZoomInOutComboBox.SelectedIndex = --fitIndx;
                        }

                        if (e.Delta < 0)
                        {
                            ViewZoomInOutComboBox.SelectedIndex = fitIndx;
                        }
                    }

                    if (e.Delta > 0 && ViewZoomInOutComboBox.SelectedIndex < 15)
                    {
                        ViewZoomInOutComboBox.SelectedIndex++;
                    }

                    if (e.Delta < 0 && ViewZoomInOutComboBox.SelectedIndex > 0)
                    {
                        ViewZoomInOutComboBox.SelectedIndex--;
                    }

                    var centetPos = new Point(PreViewScrollViewer.ViewportWidth / 2.0, PreViewScrollViewer.ViewportHeight / 2.0);
                    _lastCenterPosition = PreViewScrollViewer.TranslatePoint(centetPos, ImageViewerGrid);
                    e.Handled = true;
                }
                else
                {
                    if (e.Delta > 0)
                    {
                        if (PreViewScrollViewer.VerticalOffset == 0)
                        {
                            _scrollToBottomAfterUpdate = true;
                            TurnToNextPage(false, true);
                        }
                    }
                    else
                    {
                        if (PreViewScrollViewer.VerticalOffset == PreViewScrollViewer.ScrollableHeight)
                        {
                            TurnToNextPage(true, true);
                        }
                    }
                }
            }
        }

        private bool TurnToNextPage(bool isPageDown, bool doMouseWheel = false)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.CurrentPdfDocument == null && !doMouseWheel)
                {
                    FlatMessageWindows.DisplayWarningMessage(WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                    return false;
                }

                _isPageUpDownChanged = true;
                var index = ThumbnailListView.Items.IndexOf(pdfUtilViewModel.CurPreviewIconItem);
                if (isPageDown)
                {
                    if (index >= 0 && index < pdfUtilViewModel.TotalPageCount - 1)
                    {
                        int nextIndex = index + 1;
                        var icon = ThumbnailListView.Items[nextIndex] as IconItem;
                        if (icon != null)
                        {
                            pdfUtilViewModel.CurPreviewIconItem.IsPreviewSelected = false;
                            icon.IsPreviewSelected = true;
                            pdfUtilViewModel.PreviewPageNeedUpdate(icon);
                            ThumbnailListView.ScrollIntoView(icon);
                            return true;
                        }
                    }
                }
                else
                {
                    if (index > 0 && index < pdfUtilViewModel.TotalPageCount)
                    {
                        int nextIndex = index - 1;
                        var icon = ThumbnailListView.Items[nextIndex] as IconItem;
                        if (icon != null)
                        {
                            pdfUtilViewModel.CurPreviewIconItem.IsPreviewSelected = false;
                            icon.IsPreviewSelected = true;
                            pdfUtilViewModel.PreviewPageNeedUpdate(icon);
                            ThumbnailListView.ScrollIntoView(icon);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void PreViewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePosition.HasValue)
                {
                    if (_lastCenterPosition.HasValue)
                    {
                        var centerOfViewport = new Point(PreViewScrollViewer.ViewportWidth / 2, PreViewScrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = PreViewScrollViewer.TranslatePoint(centerOfViewport, ImageViewerGrid);

                        targetBefore = _lastCenterPosition;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePosition;
                    targetNow = Mouse.GetPosition(ImageViewerGrid);

                    _lastMousePosition = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / ImageViewerGrid.ActualWidth;
                    double multiplicatorY = e.ExtentHeight / ImageViewerGrid.ActualHeight;

                    double newOffsetX = PreViewScrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = PreViewScrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    PreViewScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    PreViewScrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.CurPreviewScrollVerticalOffset = PreViewScrollViewer.VerticalOffset;
            }

            if (ViewZoomInOutComboBox.SelectedIndex == ViewZoomInOutComboBox.Items.Count - 1)
            {
                CalScaleForFitToSize(true);
            }
        }

        private void PreViewScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var pos = e.GetPosition(PreViewScrollViewer);
                if (pos.X <= PreViewScrollViewer.ViewportWidth && pos.Y < PreViewScrollViewer.ViewportHeight) //make sure we still can use the scrollbars
                {
                    _lastDragPosition = pos;
                    Mouse.Capture(PreViewScrollViewer);
                }
            }

            if (HighlightColorPopupWindow.IsOpen)
            {
                HighlightColorPopupWindow.IsOpen = false;
            }
        }

        private void PreViewScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_lastDragPosition != null)
            {
                _lastDragPosition = null;
            }

            PreViewScrollViewer.ReleaseMouseCapture();
            _dragFindControl = false;
        }

        private void PreViewScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && _lastDragPosition.HasValue)
            {
                var pos = e.GetPosition(PreViewScrollViewer);

                double dX = pos.X - _lastDragPosition.Value.X;
                double dY = pos.Y - _lastDragPosition.Value.Y;

                _lastDragPosition = pos;

                PreViewScrollViewer.ScrollToHorizontalOffset(PreViewScrollViewer.HorizontalOffset - dX);
                PreViewScrollViewer.ScrollToVerticalOffset(PreViewScrollViewer.VerticalOffset - dY);
            }

            if (_dragFindControl && e.LeftButton == MouseButtonState.Pressed)
            {
                Point curPoint = e.GetPosition(PreViewScrollViewer);
                double offset = _findControlMouseDownPoint.X - curPoint.X;
                const double minRightMargin = 18;
                double maxRightMargin = PreViewScrollViewer.ActualWidth - findReplaceControl.ActualWidth - minRightMargin;
                double newMargin = findReplaceControl.Margin.Right + offset;
                newMargin = Math.Min(maxRightMargin, Math.Max(minRightMargin, newMargin));
                findReplaceControl.Margin = new Thickness(0, 0, newMargin, 0);
                _findControlMouseDownPoint = curPoint;
            }
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.CurrentPdfDocument == null)
                {
                    FlatMessageWindows.DisplayWarningMessage(WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                    return;
                }

                if (ThumbnailListView.Items.Count == 0)
                {
                    FlatMessageWindows.DisplayWarningMessage(pdfUtilViewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_PDF_IS_EMPTY);
                    return;
                }

                LoadFindControl();
            }
        }

        private void AddHighlightBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var highlightButton = sender as ImageToggleButton;
                if (pdfUtilViewModel.CurrentPdfDocument == null)
                {
                    FlatMessageWindows.DisplayWarningMessage(WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                    highlightButton.IsChecked = false;
                    return;
                }

                if (ThumbnailListView.Items.Count == 0)
                {
                    FlatMessageWindows.DisplayWarningMessage(pdfUtilViewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_PDF_IS_EMPTY);
                    highlightButton.IsChecked = false;
                    return;
                }

                _selectStartPoint = new Point();
                _selectEndPoint = new Point();
                pdfUtilViewModel.ClearRectangleSelect();
                highlightColor = System.Drawing.Color.Transparent;

                if (highlightButton.IsChecked == true)
                {
                    PreviewScrollViewContent.Cursor = Cursors.Cross;
                }
                else
                {
                    PreviewScrollViewContent.Cursor = Cursors.Arrow;
                }
            }
        }

        private void AddHighlightBtn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                _selectStartPoint = new Point();
                _selectEndPoint = new Point();
                pdfUtilViewModel.ClearRectangleSelect();

                e.Handled = true;
            }
        }

        private void ViewZoomBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PageUpBtn_Click(object sender, RoutedEventArgs e)
        {
            TurnToNextPage(false);
        }

        private void PageDownBtn_Click(object sender, RoutedEventArgs e)
        {
            TurnToNextPage(true);
        }

        private void SetBackGroundBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteSetBackgroundColorCommand();
            }
        }

        private void AddBlankPageBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteAddBlankPageCommand(false);
            }
        }

        private void DeletePagesBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteDeletePagesCommand();
            }
        }

        private void DeleteBlankPagesBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.CanExecuteDeleteBlankPagesCommand())
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteDeleteBlankPagesCommand();
            }
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteOpenAdoutCommand();
            }
        }

        private void RotatePageBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteRotatePageCommand();
            }
        }

        private void WatermarkPageBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteWatermarkPagesCommand();
            }
        }

        private void AddBookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.BookmarksPaneContextMenuViewModel.CanExecuteAddBookmarkCommand())
            {
                pdfUtilViewModel.BookmarksPaneContextMenuViewModel.ExecuteAddBookmarkCommand();
            }
        }

        private void AddSubBookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.BookmarksPaneContextMenuViewModel.CanExecuteAddSubBookmarkCommand())
            {
                pdfUtilViewModel.BookmarksPaneContextMenuViewModel.ExecuteAddSubBookmarkCommand();
            }
        }

        private void RemoveBookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.BookmarksPaneContextMenuViewModel.CanExecuteRemoveBookmarkCommand())
            {
                pdfUtilViewModel.BookmarksPaneContextMenuViewModel.ExecuteRemoveBookmarkCommand();
            }
        }

        private void RenameBookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.BookmarksPaneContextMenuViewModel.CanExecuteRenameBookmarkCommand())
            {
                pdfUtilViewModel.BookmarksPaneContextMenuViewModel.ExecuteRenameBookmarkCommand();
            }
        }

        public void AdjustBookmarkTabCursor(bool enable)
        {
            bookmarkPanel.Cursor = enable ? Cursors.Arrow : Cursors.Wait;
        }

        private void ThumbnailListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && !pdfUtilViewModel.IsIconSourcesChanging)
            {
                if (ThumbnailListView.Items.Count == 0 || ThumbnailListView.SelectedItems.Count != 0)
                {
                    pdfUtilViewModel.PreviewPageNeedUpdate();
                }

                foreach (var removedIcon in e.RemovedItems)
                {
                    var icon = removedIcon as IconItem;
                    if (icon != null)
                    {
                        icon.IsPreviewSelected = false;
                    }
                }

                foreach (var selectedIcon in ThumbnailListView.SelectedItems)
                {
                    var icon = selectedIcon as IconItem;
                    if (icon != null)
                    {
                        icon.IsPreviewSelected = false;
                    }
                }

                if (ThumbnailListView.SelectedItems.Count != 0)
                {
                    var selectedIcon = (IconItem)ThumbnailListView.SelectedItems[ThumbnailListView.SelectedItems.Count - 1];
                    selectedIcon.IsPreviewSelected = true;

                    if (ThumbnailListView.ItemContainerGenerator.ContainerFromItem(selectedIcon) is ListViewItem item)
                    {
                        item.Focus();
                    }
                }
            }

            UpdateThumbnailToolBarState();
            pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.UpdateThumbnailContextMenuState();
        }

        private void UpdateThumbnailToolBarState()
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                MovePagesBtn.IsEnabled = !(pdfUtilViewModel.CurrentPdfDocument.Pages.Count < 2 || ThumbnailListView.SelectedItems.Count == pdfUtilViewModel.CurrentPdfDocument.Pages.Count);
            }

            var newlist = new List<int>();
            foreach (var item in ThumbnailListView.SelectedItems)
            {
                newlist.Add(ThumbnailListView.Items.IndexOf(item) + 1);
            }

            SelectPagesTextBox.Text = SelectPagesPraser.GenerareTextInTextbox(newlist);
        }

        private void PreviewScrollViewContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (e.ClickCount > 1 && pdfUtilViewModel.IsCommentOptionEnable && AddHighlightBtn.IsChecked == false
                    && !FileOperation.FileIsReadOnly(pdfUtilViewModel.CurrentPdfFileName)
                    && !(pdfUtilViewModel.LockPDFViewModel.IsSetPermissionPassword && (pdfUtilViewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                    || pdfUtilViewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission
                    || pdfUtilViewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission)))
                {
                    // Double-click on page to add a comment if the permissions allow, the file is not read-only, and it is not in highlight mode
                    pdfUtilViewModel.MouseButtonChoosedPoint = e.GetPosition(SelectRectCanvas);
                    _selectStartPoint = new Point();
                    _selectEndPoint = new Point();
                    pdfUtilViewModel.ClearRectangleSelect();

                    pdfUtilViewModel.IsCommentPaneShow = true;
                    pdfUtilViewModel.ExcutePrepareAddComment();
                }
                else
                {
                    if (AddHighlightBtn.IsChecked == true)
                    {
                        PreviewScrollViewContent.Cursor = Cursors.Cross;
                    }

                    var element = (UIElement)sender;
                    element.CaptureMouse();

                    _selectStartPoint = _selectEndPoint = e.GetPosition(SelectRectCanvas);
                    pdfUtilViewModel.IsRectSelecting = true;
                    pdfUtilViewModel.IsRectSelected = false;
                    pdfUtilViewModel.ResetSelectedRectangle();
                    pdfUtilViewModel.ResetMouseButtonChoosedPoint();
                    pdfUtilViewModel.SelectedCommentItem = null;
                }
            }
        }

        private void PreviewScrollViewContent_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox textBox && textBox.DataContext is CommentItem)
            {
                // If the user clicks the right button while adding a comment, send LostFocusEvent
                // to comment textbox to add or cancel this adding.
                textBox.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
            }
            else
            {
                // Set right click point on page.
                var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                if (pdfUtilViewModel != null)
                {
                    pdfUtilViewModel.MouseButtonChoosedPoint = e.GetPosition(SelectRectCanvas);
                }
            }
        }

        private void PreviewScrollViewContent_MouseMove(object sender, MouseEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.IsRectSelecting)
            {
                _selectEndPoint = e.GetPosition(this.SelectRectCanvas);

                var tempPoint = new Point(Math.Min(_selectEndPoint.X, _selectStartPoint.X), Math.Min(_selectEndPoint.Y, _selectStartPoint.Y));
                pdfUtilViewModel.SelectRectLeftTopPoint = new Point(tempPoint.X < 0 ? 0 : tempPoint.X, tempPoint.Y < 0 ? 0 : tempPoint.Y);
                tempPoint = new Point(Math.Max(_selectEndPoint.X, _selectStartPoint.X), Math.Max(_selectEndPoint.Y, _selectStartPoint.Y));
                pdfUtilViewModel.SelectRectRightBottomPoint =
                    new Point(tempPoint.X > PreviewImage.ActualWidth ? PreviewImage.ActualWidth : tempPoint.X, tempPoint.Y > PreviewImage.ActualHeight ? PreviewImage.ActualHeight : tempPoint.Y);
            }
            else if (_enableForms && _fieldRects.Count > 0)
            {
                if (FormEditCanvas.Children.Count == 0)
                {
                    var point = e.GetPosition(this.SelectRectCanvas);
                    foreach (var item in _fieldRects)
                    {
                        if (item.Value.Contains(point))
                        {
                            this.Cursor = Cursors.Hand;
                            _currentFieldIndex = item.Key;
                            return;
                        }
                    }
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void PreviewScrollViewContent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;

            if (pdfUtilViewModel != null && pdfUtilViewModel.IsRectSelecting)
            {
                var element = (UIElement)sender;
                element.ReleaseMouseCapture();

                _selectEndPoint = e.GetPosition(this.SelectRectCanvas);
                if (_selectEndPoint != _selectStartPoint)
                {
                    pdfUtilViewModel.IsRectSelected = true;
                    pdfUtilViewModel.IsRectSelecting = false;
                    if (AddHighlightBtn.IsChecked == true)
                    {
                        if (highlightColor == System.Drawing.Color.Transparent)
                        {
                            HighlightColorPopupWindow.IsOpen = true;
                            PreviewScrollViewContent.Cursor = Cursors.Arrow;
                        }
                        else
                        {
                            HighlightColorPopupWindow.IsOpen = true;
                            pdfUtilViewModel.PreviewPaneContextMenuViewModel.ChangeHighlight(highlightColor, pdfUtilViewModel.IsRectSelected);
                        }
                    }
                }
                else
                {
                    _selectStartPoint = new Point();
                    _selectEndPoint = new Point();
                    pdfUtilViewModel.ClearRectangleSelect();
                    pdfUtilViewModel.IsRectSelecting = false;
                }
            }

            if (_enableForms && FormEditCanvas.Children.Count > 0)
            {
                if (!FormEditCanvas.Children[0].IsMouseOver)
                {
                    if (FormEditCanvas.Tag is Action action)
                    {
                        action();
                    }
                    FormEditCanvas.Tag = null;
                    FormEditCanvas.Children.Clear();
                    FormEditCanvas.Visibility = Visibility.Collapsed;
                }
            }

            if (_enableForms && FormEditCanvas.Children.Count == 0 && _currentFieldIndex >= 0)
            {
                var field = pdfUtilViewModel.CurrentPdfDocument.Form.Fields[_currentFieldIndex];
                _lastRectangle = _fieldRectangles[_currentFieldIndex];
                if (_lastRectangle.IsMouseOver)
                {
                    FieldAction(field);
                }
            }
        }

        private void FieldAction(Field field)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (field is TextBoxField tb)
            {
                var rect = _fieldRects[_currentFieldIndex];
                var textbox = new TextBox();
                textbox.Text = tb.Value;
                textbox.CaretIndex = textbox.Text.Length;
                textbox.VerticalContentAlignment = VerticalAlignment.Center;

                FormEditCanvas.Children.Add(textbox);
                FormEditCanvas.Visibility = Visibility.Visible;

                var page = pdfUtilViewModel.CurPreviewIconItem;
                Action textboxEdited = () =>
                {
                    if (textbox.Tag is bool)
                    {
                        tb.Value = textbox.Text;
                        page.UpdateThumbnailImage();
                        if (page.Name == pdfUtilViewModel.CurPreviewIconItem.Name)
                        {
                            pdfUtilViewModel.PreviewPageNeedUpdate();
                        }
                    }
                    FormEditCanvas.Children.Clear();
                    FormEditCanvas.Visibility = Visibility.Collapsed;
                    if (_lastRectangle != null)
                    {
                        _lastRectangle.Focus();
                    }
                };

                textbox.KeyUp += (s, ke) =>
                {
                    if (ke.Key == Key.Enter)
                    {
                        textboxEdited();
                    }
                };

                textbox.TextChanged += (s, tce) =>
                {
                    if (textbox.Text != tb.Value)
                    {
                        tb.Value = textbox.Text;
                        textbox.Tag = true;
                        if (!pdfUtilViewModel.IsDocChanged)
                        {
                            pdfUtilViewModel.NotifyDocChanged();
                        }
                    }
                };
                FormEditCanvas.Tag = textboxEdited;
                switch (pdfUtilViewModel.CurPreviewIconItem.GetPage().Rotate)
                {
                    case Aspose.Pdf.Rotation.on90:
                    case Aspose.Pdf.Rotation.on270:
                        textbox.Width = rect.Height;
                        textbox.Height = rect.Width;
                        break;
                    default:
                        textbox.Width = rect.Width;
                        textbox.Height = rect.Height;
                        break;
                }
                SetCommonBehavior(textbox, pdfUtilViewModel.CurPreviewIconItem.GetPage(), rect);
            }
            else if (field is CheckboxField ckb)
            {
                ckb.Checked = !ckb.Checked;
                pdfUtilViewModel.CurPreviewPageEdited();
                if (_lastRectangle != null)
                {
                    _lastRectangle.Focus();
                }
            }
            else if (field is RadioButtonOptionField rbof && rbof.Parent is RadioButtonField rbf)
            {
                var options = rbf.Options.ToList();
                foreach (var option in options)
                {
                    if (option.Name == rbof.OptionName && !option.Selected)
                    {
                        rbf.Selected = option.Index;
                        pdfUtilViewModel.CurPreviewPageEdited();
                        break;
                    }
                }
                if (_lastRectangle != null)
                {
                    if (_lastRectangle.Tag is FakeGroup group && group.Children.Count > 1)
                    {
                        group.Represent = group.Children.IndexOf(_lastRectangle);
                    }
                    _lastRectangle.Focus();
                }
            }
            else if (field is ChoiceField cb)
            {
                if (cb.Options.Count > 1)
                {
                    var rect = _fieldRects[_currentFieldIndex];
                    var listbox = new ListBox();
                    listbox.SelectionMode = cb.MultiSelect ? SelectionMode.Multiple : SelectionMode.Single;
                    var options = cb.Options.ToList();
                    foreach (var option in options)
                    {
                        listbox.Items.Insert(option.Index - 1, option.Value);
                        if (option.Selected)
                        {
                            if (listbox.SelectionMode == SelectionMode.Multiple)
                            {
                                listbox.SelectedItems.Add(listbox.Items.GetItemAt(option.Index - 1));
                            }
                            else
                            {
                                listbox.SelectedIndex = option.Index - 1;
                            }
                        }
                    }
                    FormEditCanvas.Children.Add(listbox);
                    FormEditCanvas.Visibility = Visibility.Visible;

                    Action AchieveChange = () =>
                    {
                        var edited = false;
                        var listSelected = new List<int>();
                        foreach (var option in cb.Options)
                        {
                            var selectedNow = listbox.SelectedIndex == option.Index - 1;
                            if (listbox.SelectionMode == SelectionMode.Multiple)
                            {
                                selectedNow = listbox.SelectedItems.Contains(option.Value);
                            }
                            if (option.Selected != selectedNow)
                            {
                                edited = true;
                            }
                            if (selectedNow)
                            {
                                listSelected.Add(option.Index);
                            }
                        }

                        if (edited)
                        {
                            if (listbox.SelectionMode == SelectionMode.Multiple)
                            {
                                cb.SelectedItems = listSelected.ToArray();
                            }
                            else
                            {
                                cb.Selected = listSelected.Count > 0 ? listSelected[0] : -1;
                            }
                        }
                    };

                    var page = pdfUtilViewModel.CurPreviewIconItem;

                    Action listboxEdited = () =>
                    {
                        if (listbox.Tag is bool)
                        {
                            page.UpdateThumbnailImage();
                            if (page.Name == pdfUtilViewModel.CurPreviewIconItem.Name)
                            {
                                pdfUtilViewModel.PreviewPageNeedUpdate();
                            }
                        }
                        FormEditCanvas.Children.Clear();
                        FormEditCanvas.Visibility = Visibility.Collapsed;
                        if (_lastRectangle != null)
                        {
                            _lastRectangle.Focus();
                        }
                    };

                    listbox.KeyUp += (s, ke) =>
                    {
                        if (ke.Key == Key.Enter)
                        {
                            listboxEdited();
                        }
                    };

                    if (listbox.SelectionMode == SelectionMode.Single)
                    {
                        listbox.MouseLeftButtonUp += (s, e) =>
                        {
                            if (listbox.SelectedItem != null)
                            {
                                listboxEdited();
                            }
                        };
                    }

                    listbox.SelectionChanged += (s, sce) =>
                    {
                        AchieveChange();
                        listbox.Tag = true;
                        if (!pdfUtilViewModel.IsDocChanged)
                        {
                            pdfUtilViewModel.NotifyDocChanged();
                        }
                    };
                    FormEditCanvas.Tag = listboxEdited;
                    switch (pdfUtilViewModel.CurPreviewIconItem.GetPage().Rotate)
                    {
                        case Aspose.Pdf.Rotation.on90:
                        case Aspose.Pdf.Rotation.on270:
                            listbox.MaxHeight = FormEditCanvas.Width - rect.X;
                            break;
                        default:
                            listbox.MaxHeight = FormEditCanvas.Height - rect.Y;
                            break;
                    }
                    SetCommonBehavior(listbox, pdfUtilViewModel.CurPreviewIconItem.GetPage(), rect);
                }
            }
        }

        private void SetCommonBehavior(Control control, Aspose.Pdf.Page page, Rect rect)
        {
            control.LayoutTransform = new RotateTransform((int)page.Rotate * 90.0);
            Canvas.SetLeft(control, rect.X);
            Canvas.SetTop(control, rect.Y);
            control.PreviewKeyDown += (s, ke) =>
            {
                if (ke.Key == Key.Tab)
                {
                    if (FormEditCanvas.Tag is Action action)
                    {
                        action();
                        FormEditCanvas.Tag = null;
                    }

                    if (_lastRectangle != null)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => { _lastRectangle.Focus(); }));
                        ke.Handled = true;
                    }
                    ke.Handled = true;
                }
            };

            control.KeyDown += (s, ke) =>
            {
                if (ke.Key == Key.Escape)
                {
                    if (FormEditCanvas.Tag is Action action)
                    {
                        action();
                        FormEditCanvas.Tag = null;
                    }
                    ke.Handled = true;
                }
            };

            control.Focus();
        }

        private void PreviewImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ClearRectangleSelect();
                _selectStartPoint = new Point();
                _selectEndPoint = new Point();
                ExitHighlightMode();
            }
        }

        private void SelectRectCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExitHighlightMode();
        }

        public int IconCompare(IconItem left, IconItem right)
        {
            int leftIndex = ThumbnailListView.Items.IndexOf(left);
            int rightIndex = ThumbnailListView.Items.IndexOf(right);
            return leftIndex.CompareTo(rightIndex);
        }

        private void ThumbnailListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    BeginDrag(e);
                }
            }
        }

        private void ThumbnailListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void ThumbnailListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
            {
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight);
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - _scrollViewer.ViewportHeight);
                e.Handled = true;
            }
            else if (KeyboardUtil.IsAltKeyDown && (e.SystemKey == Key.Up || e.SystemKey == Key.Down || e.SystemKey == Key.Left || e.SystemKey == Key.Right))
            {
                if (ThumbnailListView.SelectedItems.Count == 1)
                {
                    var selectedItemArray = new IconItem[ThumbnailListView.SelectedItems.Count];
                    ThumbnailListView.SelectedItems.CopyTo(selectedItemArray, 0);
                    Array.Sort(selectedItemArray, IconCompare);
                    int startIndex = -1;
                    if (e.SystemKey == Key.Up || e.SystemKey == Key.Left)
                    {
                        var firstIndex = ThumbnailListView.Items.IndexOf(selectedItemArray[0]);
                        if (firstIndex > 0)
                        {
                            startIndex = firstIndex - 1;
                        }
                    }
                    else
                    {
                        var lastIndex = ThumbnailListView.Items.IndexOf(selectedItemArray[selectedItemArray.Length - 1]);
                        if (lastIndex < ThumbnailListView.Items.Count - 1)
                        {
                            startIndex = lastIndex + 2;
                        }
                    }

                    if (startIndex >= 0)
                    {
                        ReorderPagesAndUpdateListView(selectedItemArray, startIndex);
                        TrackHelper.LogPdfReorderEvent(true);
                    }
                }
                e.Handled = true;
            }
        }

        private void ThumbnailListView_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            _startPoint = e.GetPosition(null);

            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null || !listViewItem.IsSelected)
            {
                return;
            }

            e.Handled = true;
        }

        private void ThumbnailListView_PreviewStylusMove(object sender, StylusEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            var position = e.GetPosition(null);

            if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                BeginDrag(e);
            }
            e.Handled = true;
            this.Cursor = Cursors.Arrow;
        }

        private void ThumbnailListView_Loaded(object sender, RoutedEventArgs e)
        {
            _dragSelector = new ListViewSelector(ThumbnailListView);
        }

        private static T GetFirstVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            if (current != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);

                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    var childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }
            return null;
        }

        public void ReorderPagesAndUpdateListView(IconItem[] sourceIcon, int destIndex)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            var bindingData = pdfUtilViewModel.Icon.IconSources;
            IconItem startIcon = null;
            bool hasChange = false;

            foreach (var icon in sourceIcon)
            {
                if (icon == null)
                {
                    continue;
                }

                if (startIcon == null)
                {
                    startIcon = icon;
                }

                int sourceIndex = bindingData.IndexOf(icon);
                var moveForward = sourceIndex >= destIndex;
                var tempIndex = moveForward ? sourceIndex : sourceIndex + 1;
                if (tempIndex != destIndex)
                {
                    // update page order in CurrentPdfDocument, index start from 1
                    pdfUtilViewModel.CurrentPdfDocument.Pages.Insert(destIndex + 1, icon.GetPage());
                    pdfUtilViewModel.CurrentPdfDocument.Pages.Delete(moveForward ? sourceIndex + 2 : sourceIndex + 1);

                    // update pdfPage in icon
                    icon.SetPage(pdfUtilViewModel.CurrentPdfDocument.Pages[moveForward ? destIndex + 1 : destIndex]);

                    // update page order in ThumbnailListView, index start from 0
                    bindingData.Insert(destIndex, icon);
                    bindingData.RemoveAt(moveForward ? sourceIndex + 1 : sourceIndex);

                    SyncBookmarkWithReorderPages(pdfUtilViewModel.RootBookmarkTree, pdfUtilViewModel.RootBookmarkTree.BookmarkItems, sourceIndex + 1, moveForward ? destIndex + 1 : destIndex);
                    hasChange = true;
                }

                if (moveForward)
                {
                    destIndex++;
                }
            }

            if (hasChange)
            {
                pdfUtilViewModel.RefreshCommentCollectionView();
                pdfUtilViewModel.NotifyDocChanged();
                pdfUtilViewModel.RefreshIconName();

                ThumbnailListView.SelectedItem = startIcon;
                ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                ReSelectMovedIcon(ThumbnailListView.Items.IndexOf(startIcon), sourceIcon.Length);
            }
        }

        private bool SyncBookmarkWithReorderPages(RootBookmarkTree rootBookmarkTree, ObservableCollection<BookmarkTreeViewItem> bookmarkTreeViewItemList, int source, int dest)
        {
            if (bookmarkTreeViewItemList == null)
            {
                return false;
            }

            for (int bookmarkIndex = 0; bookmarkIndex < bookmarkTreeViewItemList.Count; ++bookmarkIndex)
            {
                var bookmarkTreeViewItem = bookmarkTreeViewItemList[bookmarkIndex];
                var itemPageIndex = bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex;
                if (bookmarkTreeViewItem == null)
                {
                    continue;
                }

                if (dest <= itemPageIndex && itemPageIndex <= source || source <= itemPageIndex && itemPageIndex <= dest)
                {
                    int destPage;
                    if (source == itemPageIndex)
                    {
                        destPage = dest;
                    }
                    else if (source < itemPageIndex && itemPageIndex <= dest)
                    {
                        destPage = itemPageIndex - 1;
                    }
                    else
                    {
                        destPage = itemPageIndex + 1;
                    }

                    bookmarkTreeViewItem.OutlineItem.Destination = rootBookmarkTree.GenerateBookmarkAction(destPage, bookmarkTreeViewItem.BookmarkLocationInfo.Left, bookmarkTreeViewItem.BookmarkLocationInfo.Top);

                    var newBookmarkLocationInfo = new BookmarkLocationInfo(bookmarkTreeViewItem.BookmarkLocationInfo);
                    newBookmarkLocationInfo.PageIndex = destPage;
                    bookmarkTreeViewItem.BookmarkLocationInfo = newBookmarkLocationInfo;
                }

                SyncBookmarkWithReorderPages(rootBookmarkTree, bookmarkTreeViewItem.BookmarkItems, source, dest);
            }

            return true;
        }

        private void ReSelectMovedIcon(int startIndex, int length)
        {
            var icons = (DataContext as PdfUtilViewModel).Icon.IconSources;
            if (icons == null)
            {
                return;
            }

            for (int i = startIndex; i < startIndex + length; i++)
            {
                var icon = icons[i] as IconItem;
                if (icon == null)
                {
                    continue;
                }

                var item = ThumbnailListView.ItemContainerGenerator.ContainerFromItem(icon) as ListViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void BeginDrag(RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.LockPDFViewModel.IsSetPermissionPassword)
                {
                    // file's permission is set, drag-drop in thumbnail should be disabled
                    return;
                }

                var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem == null || !listViewItem.IsSelected)
                {
                    return;
                }

                var data = new DataObject();

                // add icon array to data
                var selectedItemArray = new IconItem[ThumbnailListView.SelectedItems.Count];
                ThumbnailListView.SelectedItems.CopyTo(selectedItemArray, 0);
                data.SetData(DataFormats.Serializable, selectedItemArray);

                // add extracted file to data (the file is not exist at this point, it will be generated when drag leave the thumbnail view)
                string resultPath = pdfUtilViewModel.GetExtractTempFilePath();
                data.SetData(DataFormats.FileDrop, new string[] { resultPath });

                InitialiseAdorner(listViewItem);

                PreviewGrid.AllowDrop = false;
                _dragFromThumbnailList = true;
                _dragInProgress = false;

                var effect = DragDrop.DoDragDrop(ThumbnailListView, data, DragDropEffects.Copy | DragDropEffects.Move);

                PreviewGrid.AllowDrop = true;
                _dragFromThumbnailList = false;

                // clean extract temp folder, delete temp folder if doing reorder, while keep the temp folder if doing extract
                pdfUtilViewModel.CleanTempFolderAfterDragOperation(effect == DragDropEffects.None);

                if (_adorner != null)
                {
                    AdornerLayer.GetAdornerLayer(ThumbnailListView).Remove(_adorner);
                    _adorner = null;
                }
            }
        }

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            var brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0;
            _layer = AdornerLayer.GetAdornerLayer(ThumbnailListView as Visual);
            _layer.Add(_adorner);
        }

        private void ThumbnailListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!_dragFromThumbnailList)
            {
                e.Effects = DragDropEffects.None;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    if (files != null && files.Length > 0)
                    {
                        e.Effects = DragDropEffects.Copy;
                        WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.INSERT_DROPDESCRIPTION, string.Empty);
                        WpfDragDropHelper.DragEnter(e, this, this);
                    }
                }
            }
            else
            {
                _dragInProgress = true;
            }

            e.Handled = true;
        }

        private void ThumbnailListView_DragOver(object sender, DragEventArgs args)
        {
            if (!_dragFromThumbnailList)
            {
                args.Effects = DragDropEffects.None;

                if (args.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = args.Data.GetData(DataFormats.FileDrop) as string[];
                    if (files != null && files.Length > 0)
                    {
                        args.Effects = DragDropEffects.Copy;
                        WpfDragDropHelper.SetDropDescription(args.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.INSERT_DROPDESCRIPTION, string.Empty);
                        WpfDragDropHelper.DragOver(args, this);
                    }
                }
            }
            else
            {
                _dragInProgress = true;

                if (_adorner != null)
                {
                    if (ThumbnailListView.SelectedItems.Count == ThumbnailListView.Items.Count)
                    {
                        return;
                    }

                    double verticalPos = args.GetPosition(ThumbnailListView).Y;
                    if (verticalPos < triggerHeight)
                    {
                        _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - offset);
                    }
                    else if (verticalPos > ThumbnailListView.ActualHeight - triggerHeight)
                    {
                        _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + offset);
                    }

                    _adorner.OffsetLeft = args.GetPosition(ThumbnailListView).X;
                    _adorner.OffsetTop = args.GetPosition(ThumbnailListView).Y - _startPoint.Y;
                }
            }

            args.Handled = true;
        }

        private void ThumbnailListView_DragLeave(object sender, DragEventArgs e)
        {
            if (!_dragFromThumbnailList)
            {
                e.Effects = DragDropEffects.None;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    if (files != null && files.Length > 0)
                    {
                        WpfDragDropHelper.DragLeave();
                    }
                }
            }
            else
            {
                // DragLeave events will be fired even if we do drag inside the ThumbnailListView, and
                // it will be followed immediately by DragEnter events. So when we get a DragLeave,
                // we can't tell if the drag left the ThumbnailListView or not. 
                // To identify these two cases, we use a variable _dragInProgress and the following code.
                _dragInProgress = false;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!_dragInProgress)
                    {
                        var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                        if (pdfUtilViewModel != null)
                        {
                            string tempFile = pdfUtilViewModel.GetExtractTempFilePath();

                            if (!File.Exists(tempFile))
                            {
                                // generate the extracted file and set data
                                var extractPageList = new List<int>();
                                foreach (var item in ThumbnailListView.SelectedItems)
                                {
                                    var icon = item as IconItem;
                                    if (icon != null)
                                    {
                                        extractPageList.Add(pdfUtilViewModel.GetPageIndex(icon.GetPage()));
                                    }
                                }

                                extractPageList.Sort();

                                Mouse.SetCursor(Cursors.Wait);

                                var extractResultPath = string.Empty;
                                if (pdfUtilViewModel.ExtractTempFileByDragOperation(extractPageList, ref extractResultPath))
                                {
                                    e.Data.SetData(DataFormats.FileDrop, new string[] { extractResultPath });
                                }

                                Mouse.SetCursor(Cursors.Arrow);
                            }
                        }
                    }
                }));
            }

            e.Handled = true;
        }

        private void ThumbnailListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Serializable))
            {
                e.Effects = DragDropEffects.None;

                // drag from Thumbnail
                if (ThumbnailListView.SelectedItems.Count == ThumbnailListView.Items.Count)
                {
                    return;
                }

                var sourceIconArray = e.Data.GetData(DataFormats.Serializable) as IconItem[];
                if (sourceIconArray != null && sourceIconArray.Length > 0)
                {
                    var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

                    if (listViewItem != null)
                    {
                        var destIcon = ThumbnailListView.ItemContainerGenerator.ItemFromContainer(listViewItem) as IconItem;
                        var destIndex = ThumbnailListView.Items.IndexOf(destIcon);

                        var putBelow = destIcon.PutBelow;
                        Array.Sort(sourceIconArray, IconCompare);
                        ReorderPagesAndUpdateListView(sourceIconArray, putBelow ? destIndex + 1 : destIndex);
                        TrackHelper.LogPdfReorderEvent(true);
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // drag from Explorer
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);

                    var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                    if (pdfUtilViewModel != null)
                    {
                        if (pdfUtilViewModel.CurrentPdfDocument != null)
                        {
                            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
                            if (listViewItem != null)
                            {
                                var destIcon = ThumbnailListView.ItemContainerGenerator.ItemFromContainer(listViewItem) as IconItem;
                                var destIndex = ThumbnailListView.Items.IndexOf(destIcon);

                                var putBelow = destIcon.PutBelow;
                                CurCaretIndex = putBelow ? destIndex + 1 : destIndex;
                            }
                        }

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Activate();
                            pdfUtilViewModel.ExecuteDragFromExploreTaskCommand(files, true);
                        }));
                    }
                }
            }
            else
            {
                WpfDragDropHelper.Drop(e, this);
            }

            e.Handled = true;
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

        private void PreviewGrid_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    if (pdfUtilViewModel.CurrentPdfDocument != null && (files.Length > 1 || System.IO.Path.GetExtension(files[0]).ToLower() != PdfHelper.PdfExtension))
                    {
                        e.Effects = DragDropEffects.None;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Copy;
                        WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, Properties.Resources.OPEN_PICKER_TITLE, string.Empty);
                        SetWpfDragDropEffect(e);
                    }
                }
            }

            e.Handled = true;
        }

        private void PreviewGrid_Drop(object sender, DragEventArgs e)
        {
            // drag files to preview pane
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Activate();
                        pdfUtilViewModel.ExecuteDragFromExploreTaskCommand(files, false);
                    }));
                    e.Handled = true;
                }
            }
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

            if (PdfUtilSettings.Instance.ViewZoomInOutSelectedIndex != -1)
            {
                ViewZoomInOutComboBox.SelectedIndex = PdfUtilSettings.Instance.ViewZoomInOutSelectedIndex;
            }
            else
            {
                ViewZoomInOutComboBox.SelectedIndex = 17;
            }

            ViewZoomInOutComboBox.Width = (MeasureTextWidth(Properties.Resources.INFO_VIEW_BY_SIZE_TO_FIT) > MeasureTextWidth(Properties.Resources.INFO_VIEW_BY_ACTUAL_SIZE) ? MeasureTextWidth(Properties.Resources.INFO_VIEW_BY_SIZE_TO_FIT) : MeasureTextWidth(Properties.Resources.INFO_VIEW_BY_ACTUAL_SIZE)) + 50;
        }

        public void CalScaleForFitToSize(bool show)
        {
            if (ViewZoomInOutComboBox.SelectedIndex != ViewZoomInOutComboBox.Items.Count - 1)
            {
                return;
            }

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.PreviewPageImage != null)
            {
                // when calculate ratio for fit to size, it need a margin to prevent the scrollbar from being visible, we set the margin to 10.
                var WidthRatio = (PreViewScrollViewer.ActualWidth - 10) / pdfUtilViewModel.PreviewPageImage.Width;
                var heightRatio = (PreViewScrollViewer.ActualHeight - 10) / pdfUtilViewModel.PreviewPageImage.Height;

                double ratio = Math.Min(WidthRatio, heightRatio);
                if (show)
                {
                    scaleTransform.ScaleX = ratio;
                    scaleTransform.ScaleY = ratio;
                }
                _sizeToFitX = ratio;
                _sizeToFitY = ratio;
            }
        }

        public int CalScaleForFitToSizeIndex()
        {
            CalScaleForFitToSize(false);
            double[] sizeArr = { 0.1, 0.25, 0.75, 1, 1.1, 1.25, 1.5, 1.75, 2.0, 2.5, 3.0, 3.5, 4.0, 5.0, 6.0 };
            const double defaultScaleFactor = 1.147;
            int index = 0;
            foreach (var item in sizeArr)
            {
                var width = item * defaultScaleFactor;
                if (_sizeToFitX > width)
                {
                    index++;
                }
            }
            return index;
        }

        private void ViewZoomInOutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            double scale = 0;
            if (_zoomInOutDiction.TryGetValue(combobox.SelectedIndex, out scale) && scale != 0)
            {
                const double defaultScaleFactor = 1.147;
                scaleTransform.ScaleX = scale * defaultScaleFactor;
                scaleTransform.ScaleY = scale * defaultScaleFactor;
            }
            else
            {
                CalScaleForFitToSize(true);
            }

            var centetPos = new Point(PreViewScrollViewer.ViewportWidth / 2.0, PreViewScrollViewer.ViewportHeight / 2.0);
            PreViewScrollViewer.TranslatePoint(centetPos, ImageViewerGrid);
        }

        private void PreViewScrollView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // If the current e.Source is a child element of FormEditCanvas, do not open context menu.
            var canvas = VisualTreeHelperUtils.FindAncestor<Canvas>(e.Source as DependencyObject);
            if (canvas != null && canvas.Name.Equals("FormEditCanvas"))
            {
                e.Handled = true;
            }
            else
            {
                var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
                {
                    pdfUtilViewModel.PreviewPaneContextMenuViewModel.InitPreviewPaneMenuItem();
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void SelectRectCanvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (!pdfUtilViewModel.PreviewPaneContextMenuViewModel.InitPreviewPaneSelectedRectMenuItem())
                {
                    e.Handled = true;
                    pdfUtilViewModel.ClearRectangleSelect();
                    pdfUtilViewModel.ResetMouseButtonChoosedPoint();
                }
            }
        }

        private void ThumbnailStackPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
            {
                var point = new Point(0, 0);
                if (e.OriginalSource is ScrollViewer)
                {
                    point = ((ScrollViewer)e.OriginalSource).TranslatePoint(new Point(e.CursorLeft, e.CursorTop), ThumbnailListView);
                }
                else
                {
                    ListViewItem item = VisualTreeHelperUtils.FindAncestor<ListViewItem>(e.OriginalSource as FrameworkElement);
                    if (item != null)
                    {
                        point = (item).TranslatePoint(new Point(e.CursorLeft, e.CursorTop), ThumbnailListView);
                    }
                }
                pdfUtilViewModel.ThumbnailContextMenuDownPoint = point;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void ThumbnailListViewStackPanel_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.SelectPagesAfterContextMenuClosed();
            }
        }

        private void bookmarkPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && KeyboardUtil.IsCtrlKeyDown)
            {
                var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                SelectAllBoolmark(pdfUtilViewModel.RootBookmarkTree.BookmarkItems);
            }
        }

        private void SelectAllBoolmark(ObservableCollection<BookmarkTreeViewItem> BookmarkItems)
        {
            if (BookmarkItems.Count == 0)
            {
                return;
            }

            foreach (var bookmark in BookmarkItems)
            {
                bookmark.IsSelected = true;
                SelectAllBoolmark(bookmark.BookmarkItems);
            }
        }

        private void findReplaceControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragFindControl = true;
            _findControlMouseDownPoint = e.GetPosition(PreViewScrollViewer);
            PreViewScrollViewer.CaptureMouse();
        }

        private void OnThumbnailItemLoaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control != null)
            {
                var icon = control.DataContext as IconItem;
                if (icon != null && !icon.IsLoadThumbnailCompleted)
                {
                    IconThumbnailManager.AddRender(control);
                }

                if (icon != null)
                {
                    var iconIndex = ThumbnailListView.Items.IndexOf(icon);
                    if (iconIndex == ThumbnailListView.Items.Count - 1 && _curCaretIndex == -1)
                    {
                        var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>(control);
                        if (listViewItem != null)
                        {
                            MakeListViewItemCaretVisible(listViewItem, iconIndex);
                        }
                    }
                    else if (iconIndex == _curCaretIndex - 1)
                    {
                        var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>(control);
                        if (listViewItem != null)
                        {
                            MakeListViewItemCaretVisible(listViewItem, iconIndex);
                        }
                    }
                }
            }
        }

        public void RefreshCaretWhenIconDeleted()
        {
            if (ThumbnailListView.Items.Count == 0)
            {
                return;
            }

            IconItem icon = null;
            if (_curCaretIndex == -1)
            {
                icon = ThumbnailListView.Items[ThumbnailListView.Items.Count - 1] as IconItem;
            }
            else if (_curCaretIndex > 0)
            {
                icon = ThumbnailListView.Items[_curCaretIndex - 1] as IconItem;
            }

            if (icon != null)
            {
                var item = ThumbnailListView.ItemContainerGenerator.ContainerFromItem(icon) as ListViewItem;
                if (item != null)
                {
                    MakeListViewItemCaretVisible(item, _curCaretIndex - 1);
                }
            }
        }

        private void MakeListViewItemCaretVisible(ListViewItem item, int iconIndex)
        {
            var caret = VisualTreeHelperUtils.FindVisualChild<Caret>(item, o => o.Tag as string == "caret");
            if (caret != null)
            {
                HiddenCurDisplayCaret();
                _curDisplayCaret = caret;
                caret.Visibility = Visibility.Visible;
                if (iconIndex != ThumbnailListView.Items.Count - 1)
                {
                    CurCaretIndex = iconIndex + 1;
                }
            }
        }

        private void OnThumbnailItemUnloaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control != null)
            {
                IconThumbnailManager.RemoveRender(control);
            }
        }

        private void PdfUtilViewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel == null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.F:
                    if (KeyboardUtil.IsCtrlKeyDown && ThumbnailListView.Items.Count != 0)
                    {
                        LoadFindControl();
                        e.Handled = true;
                    }

                    break;
                case Key.N:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteCreateFromCommand();
                        e.Handled = true;
                    }

                    break;
                case Key.O:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteOpenCommand();
                        e.Handled = true;
                    }

                    break;
                case Key.S:
                    if (KeyboardUtil.IsCtrlKeyDown)
                    {
                        pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteSaveCommand();
                        e.Handled = true;
                    }

                    break;
                case Key.A:
                    if (KeyboardUtil.IsCtrlKeyDown && KeyboardUtil.IsShiftKeyDown)
                    {
                        pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteSaveAsCommand();
                        e.Handled = true;
                    }

                    break;
                case Key.Delete:
                    pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.ExecuteDeletePagesCommand();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (HighlightColorPopupWindow.IsOpen)
                    {
                        HighlightColorPopupWindow.IsOpen = false;
                        pdfUtilViewModel.ClearAllSelectionOnPage();
                        PreviewScrollViewContent.Cursor = Cursors.Cross;
                    }
                    else if (_signatureElement != null && _draggingSignature)
                    {
                        SignatureDragFinished(false);
                    }
                    else
                    {
                        pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteExitCommand();
                    }
                    e.Handled = true;
                    break;
                default:
                    return;
            }
        }

        private void LoadFindControl()
        {
            findReplaceControl.Visibility = Visibility.Visible;
            var textBox = VisualTreeHelperUtils.FindVisualChild<TextBox>(findReplaceControl, o => o.Name == "findBox");
            {
                if (textBox != null)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }

        private void PreViewScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void AdjustPaneCursor(bool enable)
        {
            this.Cursor = enable ? null : Cursors.Wait;

            ThumbnailListView.IsHitTestVisible = enable;
            PreViewScrollViewer.IsHitTestVisible = enable;
            RibbonStackPanel.IsEnabled = enable;
            TabControlsStackGrid.IsEnabled = enable;
            PreviewPanel.IsEnabled = enable;
            PdfStartupPane.IsEnabled = enable;

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (enable && pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.CurrentPdfDocument == null)
                {
                    PdfStartupPane.Focus();
                }
                else
                {
                    ThumbnailListView.Focus();
                }
            }

            Mouse.UpdateCursor();
        }

        private void HighlightColor_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var colorButton = sender as Button;
                System.Drawing.Color selectedColor = System.Drawing.Color.Transparent;
                if (!colorButton.Name.Equals("NoColorButton"))
                {
                    HighlightColorPiece.Fill = colorButton.Background;
                    var mediaColor = ((SolidColorBrush)colorButton.Background).Color;
                    if (colorButton.Tag != null)
                    {
                        mediaColor.A = (byte)(mediaColor.A * Convert.ToDouble(colorButton.Tag));
                    }
                    selectedColor = System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
                }
                else
                {
                    HighlightColorPiece.Fill = new SolidColorBrush(Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
                }

                bool isChangeLastHighlight = highlightColor != System.Drawing.Color.Transparent && AddHighlightBtn.IsChecked == true;

                highlightColor = selectedColor;
                HighlightColorPopupWindow.IsOpen = false;

                if (selectedColor == System.Drawing.Color.Transparent && AddHighlightBtn.IsChecked == false)
                {
                    pdfUtilViewModel.ClearAllSelectionOnPage();
                    return;
                }

                if (AddHighlightBtn.IsChecked == true)
                {
                    PreviewScrollViewContent.Cursor = Cursors.Cross;
                }

                pdfUtilViewModel.PreviewPaneContextMenuViewModel.ChangeHighlight(selectedColor, pdfUtilViewModel.IsRectSelected, isChangeLastHighlight);
            }
        }

        private void ExitHighlightMode()
        {
            if (AddHighlightBtn.IsChecked == true)
            {
                PreviewScrollViewContent.Cursor = Cursors.Arrow;
                AddHighlightBtn.IsChecked = false;
            }
        }

        public void SetViewPageTextBox(int pageCount)
        {
            CurPageTextBox.Text = pageCount.ToString();
            CurPageTextBox.IsEnabled = pageCount != 0;
            _currentFieldIndex = -1;
        }

        private void CurPageTextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (TextBox)sender;
            if (e.Source is TextBox)
            {
                element.SelectAll();
                e.Handled = true;
            }
        }

        private void CurPageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
            {
                var element = (TextBox)sender;
                if (e.Key == Key.Escape)
                {
                    SetViewPageTextBox(pdfUtilViewModel.PreviewPageNumber);
                    element.CaretIndex = element.Text.Length;
                }
                else if (e.Key == Key.Enter)
                {
                    int index = 0;
                    if (int.TryParse(element.Text, out index) && index > 0 && index <= pdfUtilViewModel.TotalPageCount)
                    {
                        ThumbnailListView.SelectedIndex = index - 1;
                        ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                    }
                    else
                    {
                        FlatMessageWindows.DisplayWarningMessage(new WindowInteropHelper(this).Handle, string.Format(Properties.Resources.WARNING_NO_PAGE_NUMBER, element.Text));
                        element.SelectAll();
                    }
                }
            }
        }

        private void CurPageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
            {
                SetViewPageTextBox(pdfUtilViewModel.PreviewPageNumber);
            }
        }

        private void PreViewScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && KeyboardUtil.IsCtrlKeyDown)
            {
                var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
                {
                    pdfUtilViewModel.DoCopyFromPdf();
                }
            }

            if (e.Key == Key.PageDown)
            {
                TurnToNextPage(true);
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                TurnToNextPage(false);
                e.Handled = true;
            }
        }

        public void SetWindowLoadingStatus(bool isLoading)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                if (LoadingGif.Source == null)
                {
                    var bits = WinzipMethods.Is32Bit() ? "32" : "64";
                    var source = $"pack://application:,,,/PdfUtil{bits};component/Resources/Loading.gif";

                    var image = new System.Windows.Media.Imaging.BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(source);
                    image.EndInit();
                    ImageBehavior.SetAnimatedSource(LoadingGif, image);
                }

                loadingGrid.Visibility = isLoading ? Visibility.Visible : Visibility.Hidden;
                PdfUtilViewWindow.IsEnabled = !isLoading;
            }));
        }

        private void ThumbnailListView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
            {
                return;
            }

            ThumbnailCommentCountClick((DependencyObject)e.OriginalSource);
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
            {
                return;
            }

            if (ThumbnailCommentCountClick((DependencyObject)e.OriginalSource))
            {
                e.Handled = true;
                return;
            }

            int curListViewItemIndex = ThumbnailListView.Items.IndexOf(listViewItem.DataContext);
            if (e.OriginalSource is Rectangle)
            {
                var rectangle = e.OriginalSource as Rectangle;
                if (rectangle != null)
                {
                    var grid = rectangle.Parent as Grid;
                    if (grid != null)
                    {
                        if (rectangle.Name == "firstCaretSupport")
                        {
                            Caret caret = null;
                            if (curListViewItemIndex == 0)
                            {
                                // click firstCaretSupport line at the top of the file
                                caret = VisualTreeHelperUtils.FindVisualChild<Caret>(grid, o => o.Tag as string == "firstCaret");
                            }
                            else
                            {
                                // click firstCaretSupport line in the middle of the file
                                var virtualizingWrapPanel = VisualTreeHelperUtils.FindAncestor<VirtualizingWrapPanel>(grid);
                                if (virtualizingWrapPanel != null)
                                {
                                    var preGrid = VisualTreeHelperUtils.FindVisualChild<Grid>(virtualizingWrapPanel, o => ((IconItem)o.DataContext).Name == curListViewItemIndex.ToString());
                                    if (preGrid != null)
                                    {
                                        caret = VisualTreeHelperUtils.FindVisualChild<Caret>(preGrid, o => o.Tag as string == "caret");
                                    }
                                }
                            }

                            if (caret != null)
                            {
                                ThumbnailListView.UnselectAll();
                                CurCaretIndex = curListViewItemIndex;
                                _curDisplayCaret = caret;
                                caret.Visibility = Visibility.Visible;
                            }
                        }
                        else if (rectangle.Name == "caretSupport")
                        {
                            // click caretSupport line
                            var caret = VisualTreeHelperUtils.FindVisualChild<Caret>(grid, o => o.Tag as string == "caret");
                            if (caret != null)
                            {
                                ThumbnailListView.UnselectAll();
                                CurCaretIndex = (curListViewItemIndex == ThumbnailListView.Items.Count - 1) ? -1 : curListViewItemIndex + 1;
                                _curDisplayCaret = caret;
                                caret.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }

                e.Handled = true;
            }
        }

        private void caretSupport_DragAndDrop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.RoutedEvent == DragDrop.DropEvent)
            {
                WpfDragDropHelper.Drop(e, this);
            }

            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
            {
                return;
            }

            int curListViewItemIndex = ThumbnailListView.Items.IndexOf(listViewItem.DataContext);

            var rectangle = e.OriginalSource as Rectangle;
            if (rectangle != null)
            {
                var grid = rectangle.Parent as Grid;
                if (grid != null)
                {
                    Rectangle listBoxItemIconViewLine = null;
                    Rectangle listBoxItemIconViewTopLine = null;
                    if (rectangle.Name == "firstCaretSupport")
                    {
                        // drag/drop in firstCaretSupport
                        if (curListViewItemIndex == 0)
                        {
                            // drag/drop in firstCaretSupport line at the top of the file
                            listBoxItemIconViewLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(grid, o => o.Name == "listBoxItemIconViewLine");
                            listBoxItemIconViewTopLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(grid, o => o.Name == "listBoxItemIconViewTopLine");
                            listBoxItemIconViewLine.Visibility = Visibility.Hidden;
                            listBoxItemIconViewTopLine.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // drag/drop in firstCaretSupport line in the middle of the file
                            var virtualizingWrapPanel = VisualTreeHelperUtils.FindAncestor<VirtualizingWrapPanel>(grid);
                            if (virtualizingWrapPanel != null)
                            {
                                var preGrid = VisualTreeHelperUtils.FindVisualChild<Grid>(virtualizingWrapPanel, o => ((IconItem)o.DataContext).Name == curListViewItemIndex.ToString());
                                if (preGrid != null)
                                {
                                    listBoxItemIconViewLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(preGrid, o => o.Name == "listBoxItemIconViewLine");
                                    listBoxItemIconViewTopLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(preGrid, o => o.Name == "listBoxItemIconViewTopLine");
                                    listBoxItemIconViewLine.Visibility = Visibility.Visible;
                                    listBoxItemIconViewTopLine.Visibility = Visibility.Hidden;
                                }
                            }
                        }

                        if (e.RoutedEvent == DragDrop.DropEvent)
                        {
                            // put the drag source file upon the current listViewItem
                            (listViewItem.DataContext as IconItem).PutBelow = false;
                        }
                    }
                    else if (rectangle.Name == "caretSupport")
                    {
                        // drag/drop to caretSupport line
                        listBoxItemIconViewLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(grid, o => o.Name == "listBoxItemIconViewLine");
                        listBoxItemIconViewTopLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(grid, o => o.Name == "listBoxItemIconViewTopLine");
                        listBoxItemIconViewLine.Visibility = Visibility.Visible;
                        listBoxItemIconViewTopLine.Visibility = Visibility.Hidden;

                        if (e.RoutedEvent == DragDrop.DropEvent)
                        {
                            // put the drag source file below the current listViewItem
                            (listViewItem.DataContext as IconItem).PutBelow = true;
                        }
                    }

                    if (listBoxItemIconViewLine != null && e.RoutedEvent != DragDrop.DragEnterEvent)
                    {
                        listBoxItemIconViewLine.Visibility = Visibility.Hidden;
                        listBoxItemIconViewTopLine.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void HiddenCurDisplayCaret()
        {
            if (_curDisplayCaret != null)
            {
                if (_curDisplayCaret != null)
                {
                    _curDisplayCaret.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ViewZoomInOutComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = true;
            }
        }

        private void PreViewScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HighlightColorPopupWindow.IsOpen)
            {
                HighlightColorPopupWindow.IsOpen = false;
            }
        }

        private void PdfUtilViewWindow_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HighlightColorPopupWindow.IsOpen && !(e.NewFocus is Button))
            {
                HighlightColorPopupWindow.IsOpen = false;
            }

            if (_initSignature && !(e.NewFocus is ComboBox))
            {
                ClearSignatureCanvas();
            }
        }

        private System.Drawing.Color GetButtonBackgroundColor(Button button)
        {
            if (button == null)
            {
                return System.Drawing.Color.Transparent;
            }

            var backgroundColor = ((SolidColorBrush)button.Background).Color;
            return System.Drawing.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
        }

        private void HighlightColorPopupWindow_Opened(object sender, EventArgs e)
        {
            var curSelectedBtn = VisualTreeHelperUtils.FindVisualChild<Button>(highlightPanel, o => GetButtonBackgroundColor(o) == highlightColor);
            if (curSelectedBtn != null)
            {
                curSelectedBtn.Focus();
            }
        }

        private void PdfUtilViewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.FakeRibbonTabViewModel.DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void PdfStartupPane_OpenFile(object sender, RoutedEventArgs e)
        {
            var recentFile = e.OriginalSource as RecentFile;
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && recentFile != null)
            {
                pdfUtilViewModel.Executor(() => pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteOpenRecentItemTask(recentFile.RecentOpenFileCloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
            }
        }

        private void PdfStartupPane_ChooseFile(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.FakeRibbonTabViewModel.ExecuteOpenCommand();
            }
        }

        private void PdfStartupPane_SetAsDefault(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
#if WZ_APPX
            var fileAssocWindow = new StartupPaneLib.FileAssociationWindow(this, StartupPaneLib.Applet.PdfExpress);
            fileAssocWindow.ShowDialog();
            if (fileAssocWindow.Associated)
            {
                pdfUtilViewModel.NotifyDefaultSet();
            }
#else
            if (pdfUtilViewModel != null)
            {
                IntegrationDialog.SetPdfUtilAsDefault();
                pdfUtilViewModel.FakeRibbonTabViewModel.StartAdminProgressAndChangeIntegration();
            }
#endif
        }

        private void PdfStartupPane_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    if (pdfUtilViewModel.CurrentPdfDocument != null && (files.Length > 1 || System.IO.Path.GetExtension(files[0]).ToLower() != PdfHelper.PdfExtension))
                    {
                        e.Effects = DragDropEffects.None;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Copy;
                        bool isOpenFile = files.Length == 1 && System.IO.Path.GetExtension(files[0]).ToLower() == PdfHelper.PdfExtension;
                        WpfDragDropHelper.SetDropDescription(e.Data, WpfDragDropHelper.DropImageType.Copy, isOpenFile ? Properties.Resources.OPEN_PICKER_TITLE : Properties.Resources.INSERT_DROPDESCRIPTION, string.Empty);
                        SetWpfDragDropEffect(e);
                    }
                }
            }

            e.Handled = true;
        }

        private void PdfStartupPane_Drop(object sender, DragEventArgs e)
        {
            // drop files to Startup Pane
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    WpfDragDropHelper.Drop(e, this);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Activate();
                        pdfUtilViewModel.ExecuteDragFromExploreTaskCommand(files, false);
                    }));
                    e.Handled = true;
                }
            }
        }

        private void SelectPagesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9,-]+");
            bool match = regex.IsMatch(e.Text);
            e.Handled = !match;
        }

        private void SelectPagesTextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (TextBox)sender;
            if (e.Source is TextBox)
            {
                element.SelectAll();
                e.Handled = true;
            }
        }

        private void SelectPagesTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
            {
                var element = (TextBox)sender;
                if (e.Key == Key.Escape)
                {
                    element.CaretIndex = element.Text.Length;
                }
                else if (e.Key == Key.Enter)
                {
                    var selectPages = SelectPagesPraser.PraseTextInTextbox(element.Text, ThumbnailListView.Items.Count);
                    if (selectPages != null && selectPages.Count > 0)
                    {
                        var selectedItems = new List<object>();
                        foreach (var item in ThumbnailListView.Items)
                        {
                            var icon = item as IconItem;
                            if (icon != null && selectPages.Contains(int.Parse(icon.Name)))
                            {
                                selectedItems.Add(item);
                            }
                        }

                        ThumbnailListView.SelectedItems.Clear();
                        ThumbnailListView.SetSelectedItems(selectedItems);
                        ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);

                        element.CaretIndex = element.Text.Length;
                    }
                    else
                    {
                        ThumbnailListView.UnselectAll();
                        element.Text = string.Empty;
                    }
                }
            }
        }

        private void MovePagesPopupWindow_Opened(object sender, EventArgs e)
        {
            var element = (Popup)sender;
            if (element != null)
            {
                var movePagesPanel = element.Child;
                if (movePagesPanel != null)
                {
                    var afterRadioButton = VisualTreeHelperUtils.FindVisualChild<RadioButton>(movePagesPanel, o => o.Name == "AfterRadioButton");
                    var numericUpDown = VisualTreeHelperUtils.FindVisualChild<NumericUpDown>(movePagesPanel, o => o.Name == "numericUpDown");

                    if (afterRadioButton != null && numericUpDown != null)
                    {
                        afterRadioButton.IsChecked = true;
                        numericUpDown.Value = 1;
                    }

                    movePagesPanel.Focus();
                }
            }
        }

        private void MovePagesPopupWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (Popup)sender;
            if (element != null)
            {
                element.Focus();
                e.Handled = true;
            }
        }

        private void MovePagesPopupWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var element = (Popup)sender;
                if (element != null)
                {
                    var movePagesPanel = element.Child;
                    if (movePagesPanel != null)
                    {
                        var moveButton = VisualTreeHelperUtils.FindVisualChild<Button>(movePagesPanel, o => o.Name == "MoveButton");
                        if (moveButton != null)
                        {
                            MoveButton_Click(moveButton, null);
                            e.Handled = true;
                        }
                    }
                }
            }
            else if (e.Key == Key.Escape)
            {
                var element = (Popup)sender;
                if (element != null)
                {
                    element.IsOpen = false;
                    e.Handled = true;
                }
            }
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && pdfUtilViewModel.CurrentPdfDocument != null)
            {
                var element = (Button)sender;
                if (element != null)
                {
                    var parent = VisualTreeHelperUtils.FindAncestor<Grid>(element);
                    if (parent != null)
                    {
                        var afterRadioButton = VisualTreeHelperUtils.FindVisualChild<RadioButton>(parent, o => o.Name == "AfterRadioButton");
                        var numericUpDown = VisualTreeHelperUtils.FindVisualChild<NumericUpDown>(parent, o => o.Name == "numericUpDown");

                        if (afterRadioButton != null && numericUpDown != null)
                        {
                            pdfUtilViewModel.ThumbnailPaneContextMenuViewModel.DoMovePages(numericUpDown.Value, afterRadioButton.IsChecked == true);
                            MovePagesPopupWindow.IsOpen = false;
                        }
                    }
                }
            }
        }

        private void sliderIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            var tick = Math.Min(0.44, thumbnailSlider.Maximum - pdfUtilViewModel.ThumbnailZoom);
            if (pdfUtilViewModel != null && pdfUtilViewModel.ThumbnailZoom + tick <= thumbnailSlider.Maximum)
            {
                pdfUtilViewModel.ThumbnailZoom += tick;
            }
        }

        private void sliderReductionButton_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            var tick = Math.Min(0.44, pdfUtilViewModel.ThumbnailZoom - thumbnailSlider.Minimum);
            if (pdfUtilViewModel != null && pdfUtilViewModel.ThumbnailZoom - tick >= thumbnailSlider.Minimum)
            {
                pdfUtilViewModel.ThumbnailZoom -= tick;
            }
        }

        private void thumbnailSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && ThumbnailListView.SelectedItems.Count != 0)
            {
                var vp = VisualTreeHelperUtils.FindVisualChild<VirtualizingPanel>(ThumbnailListView, o => o.Name == "virtualizingWrapPanel");
                if (vp != null)
                {
                    for (int index = 0; index < ThumbnailListView.Items.Count; index++)
                    {
                        if (ThumbnailListView.Items[index] is IconItem item && item.IsPreviewSelected)
                        {
                            vp.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                ThumbnailListView.UpdateLayout();
                                vp.BringIndexIntoViewPublic(index);
                            }));
                            return;
                        }
                    }
                }
            }
        }

        private void ThumbnailListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null && ThumbnailListView.SelectedItems.Count != 0)
            {
                var vp = VisualTreeHelperUtils.FindVisualChild<VirtualizingPanel>(ThumbnailListView, o => o.Name == "virtualizingWrapPanel");
                if (vp != null)
                {
                    for (int index = 0; index < ThumbnailListView.Items.Count; index++)
                    {
                        if (ThumbnailListView.Items[index] is IconItem item && item.IsPreviewSelected)
                        {
                            vp.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                ThumbnailListView.UpdateLayout();
                                vp.BringIndexIntoViewPublic(index);
                            }));
                            return;
                        }
                    }
                }
            }
        }

        #region Signature Bar & Canvas Action Region

        public void SetSignatureBarMargin()
        {
            // make Signature Tool Bar align left if the Preview Panel width is sufficient.
            double defaultMargin = ToolsStackPanel.Margin.Left;
            double signatureLeftMargin = defaultMargin;
            if (PreviewPanel.ActualWidth - ToolsStackPanel.ActualWidth - SignatureStackPanel.ActualWidth - 3 * defaultMargin > 0)
            {
                signatureLeftMargin += PreviewPanel.ActualWidth - ToolsStackPanel.ActualWidth - SignatureStackPanel.ActualWidth - 3 * defaultMargin;
            }
            SignatureStackPanel.Margin = new Thickness(signatureLeftMargin, 2, defaultMargin, 0);
        }

        public void ClearSignatureCanvas()
        {
            // clear the signature canvas, leave the signature addition status
            _initSignature = false;
            _draggingSignature = false;
            _signatureElement = null;

            Mouse.Capture(null);
            PreviewScrollViewContent.Cursor = Cursors.Arrow;
            SignatureAddCanvas.Children.Clear();
            SignatureAddCanvas.Background = null;
        }

        public void PrepareSignatureCanvas()
        {
            // prepare for signature addition
            SignatureAddCanvas.Children.Clear();
            SignatureAddCanvas.Background = new SolidColorBrush(Colors.Transparent);

            _initSignature = true;
        }

        private void SignatureAddCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_signatureElement != null && _initSignature)
            {
                _initSignature = false;

                // add the selected signature in canvas
                var startPoint = e.GetPosition(SignatureAddCanvas);
                if (startPoint.Y - _signatureElement.Height > 0 && startPoint.Y < PreviewImage.ActualHeight)
                {
                    Canvas.SetTop(_signatureElement, startPoint.Y - _signatureElement.Height);
                }
                else
                {
                    Canvas.SetTop(_signatureElement, 0);
                }

                if (startPoint.X > 0 && startPoint.X + _signatureElement.Width < PreviewImage.ActualWidth)
                {
                    Canvas.SetLeft(_signatureElement, startPoint.X);
                }
                else
                {
                    Canvas.SetLeft(_signatureElement, PreviewImage.ActualWidth - _signatureElement.Width);
                }
                SignatureAddCanvas.Children.Add(_signatureElement);

                // capture the mouse and set _draggingSignature true
                PreviewScrollViewContent.Cursor = Cursors.Cross;
                Mouse.Capture(SignatureAddCanvas);
                _draggingSignature = true;

                e.Handled = true;
            }
        }

        private void SignatureAddCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_signatureElement != null && _draggingSignature)
            {
                SignatureDragMoved();
                e.Handled = true;
            }
        }

        private void SignatureAddCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_signatureElement != null && _draggingSignature)
            {
                // add signature to the page
                SignatureDragFinished(true);
                e.Handled = true;
            }
        }

        private void SignatureAddCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_signatureElement != null && _draggingSignature)
            {
                // cancel signature addition operation
                SignatureDragFinished(false);
                // do not set e.Handled = true here for the event is still routing
            }
        }

        private void SignatureDragMoved()
        {
            // move signature in canvas
            var currentPosition = Mouse.GetPosition(SignatureAddCanvas);
            if (currentPosition.Y - _signatureElement.Height > 0 && currentPosition.Y < PreviewImage.ActualHeight)
            {
                Canvas.SetTop(_signatureElement, currentPosition.Y - _signatureElement.Height);
            }

            if (currentPosition.X > 0 && currentPosition.X + _signatureElement.Width < PreviewImage.ActualWidth)
            {
                Canvas.SetLeft(_signatureElement, currentPosition.X);
            }
        }

        private void SignatureDragFinished(bool doAdd)
        {
            Mouse.Capture(null);
            PreviewScrollViewContent.Cursor = Cursors.Arrow;
            _draggingSignature = false;

            if (doAdd)
            {
                var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                if (pdfUtilViewModel != null)
                {
                    var leftTopPoint = new Point(Canvas.GetLeft(_signatureElement), Canvas.GetTop(_signatureElement));
                    var rightBottomPoint = new Point(leftTopPoint.X + _signatureElement.Width, leftTopPoint.Y + _signatureElement.Height);

                    pdfUtilViewModel.ExecuteAddSignatureToPage(leftTopPoint, rightBottomPoint);
                }
            }

            ClearSignatureCanvas();
        }

        private void SignComboBox_AddSignature(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ExecuteAddSignatureToList();
            }
        }

        private void SignComboBox_DeleteSignature(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ExecuteDeleteSignatureFromList(e.OriginalSource as SignatureItem);
            }
        }

        private void SignComboBox_SignatureSelected(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (pdfUtilViewModel.SelectedSignature.IsInitialItem == false && pdfUtilViewModel.SelectedSignature.SignatureImage != null)
                {
                    _signatureElement = new Image
                    {
                        Width = pdfUtilViewModel.SelectedSignature.SignatureImage.Width,
                        Height = pdfUtilViewModel.SelectedSignature.SignatureImage.Height,
                        Source = pdfUtilViewModel.SelectedSignature.SignatureImage,
                    };
                    pdfUtilViewModel.AddSignature = pdfUtilViewModel.SelectedSignature;
                    PrepareSignatureCanvas();
                }
            }
        }

        private void MarkYBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                _signatureElement = new Rectangle
                {
                    Width = 28,
                    Height = 28,
                    Fill = Application.Current.TryFindResource("MarkYDrawingBrush") as DrawingBrush,
                };
                pdfUtilViewModel.AddSignature = pdfUtilViewModel.MarkYSignature;
                PrepareSignatureCanvas();
            }
        }

        private void MarkXBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                _signatureElement = new Rectangle
                {
                    Width = 28,
                    Height = 28,
                    Fill = Application.Current.TryFindResource("MarkXDrawingBrush") as DrawingBrush,
                };
                pdfUtilViewModel.AddSignature = pdfUtilViewModel.MarkXSignature;
                PrepareSignatureCanvas();
            }
        }

        #endregion

        #region Comment Pane & Canvas Action Region

        private void InitCommentSortComboBox()
        {
            CommentSortComboBox.Items.Add(Properties.Resources.COMMENT_SORT_PAGE);
            CommentSortComboBox.Items.Add(Properties.Resources.COMMENT_SORT_DATE);
            CommentSortComboBox.Items.Add(Properties.Resources.COMMENT_SORT_AUTHOR);
            CommentSortComboBox.SelectedIndex = 0;

            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                pdfUtilViewModel.ChangeCommentCollectView(CommentSortComboBox.SelectedItem as string);
            }
        }

        public void AdjustCommentPaneCursor(bool enable)
        {
            CommentControlsGrid.Cursor = enable ? Cursors.Arrow : Cursors.Wait;
        }

        public void ArrangeRightColumnWidth(bool isShow)
        {
            if (isShow)
            {
                mainWindowsLastColumn.MinWidth = 260;
                mainWindowsLastColumn.Width = new GridLength(lastRightPaneWidth);
            }
            else
            {
                lastRightPaneWidth = mainWindowsLastColumn.Width.Value;
                mainWindowsLastColumn.MinWidth = 0;
                mainWindowsLastColumn.Width = new GridLength(0);
            }
        }

        public void UpdateCommentsOnCanvas()
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                CommentCanvas.Children.Clear();

                var pageItem = pdfUtilViewModel.CurPreviewIconItem;
                if (pageItem != null && pdfUtilViewModel.PreviewPageSize.X != 0 && pdfUtilViewModel.PreviewPageSize.Y != 0)
                {
                    // draw the comment rectangle at the corresponding comment position on the page
                    foreach (var comment in pageItem.CommentItems)
                    {
                        Point ltPoint;
                        Point rbPoint;
                        Brush colorBrush;
                        if (comment.Annotation != null)
                        {
                            var rect = comment.Annotation.Rect;
                            ltPoint = pdfUtilViewModel.ConvertPagePointToPreviewPoint(new Point(rect.LLX, rect.URY), pageItem.GetPage());
                            rbPoint = pdfUtilViewModel.ConvertPagePointToPreviewPoint(new Point(rect.URX, rect.LLY), pageItem.GetPage());
                            colorBrush = PdfHelper.GetAnnotationBrush(comment.Annotation.Title);
                        }
                        else
                        {
                            var rect = comment.AddPosition;
                            ltPoint = rect.TopLeft;
                            rbPoint = rect.BottomRight;
                            colorBrush = PdfHelper.GetAnnotationBrush(Environment.UserName);
                        }

                        var commentRectangle = new CommentRectangle(comment);
                        var rectangle = VisualTreeHelperUtils.FindVisualChild<Rectangle>(commentRectangle.Content as Grid);
                        if (rectangle != null)
                        {
                            // make minor adjustments to make comment coverage display better
                            rectangle.Width = Math.Abs(rbPoint.X - ltPoint.X) + 2;
                            rectangle.Height = Math.Abs(rbPoint.Y - ltPoint.Y) + 2;

                            Canvas.SetTop(commentRectangle, Math.Min(ltPoint.Y, rbPoint.Y) - rectangle.Margin.Top - 1);
                            Canvas.SetLeft(commentRectangle, Math.Min(ltPoint.X, rbPoint.X) - rectangle.Margin.Top - 1);

                            CommentCanvas.Children.Add(commentRectangle);
                        }
                    }
                }
            }
        }

        private void CommentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var element = sender as PlaceholderTextBox;
            if (element != null)
            {
                if (e.Key == Key.Enter && !string.IsNullOrEmpty(element.Text))
                {
                    AddCommentOrCancelAdd(element, true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    AddCommentOrCancelAdd(element, false);
                    e.Handled = true;
                }
            }
        }

        private void CommentTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var element = sender as PlaceholderTextBox;
                if (element != null && element.Visibility == Visibility.Visible)
                {
                    if (!string.IsNullOrEmpty(element.Text))
                    {
                        AddCommentOrCancelAdd(element, true);
                        e.Handled = true;
                    }
                    else
                    {
                        AddCommentOrCancelAdd(element, false);
                        e.Handled = true;
                    }
                }
            }
        }

        private void AddCommentOrCancelAdd(PlaceholderTextBox textBox, bool isAdd)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>(textBox);
                if (listViewItem != null)
                {
                    var commentItem = listViewItem.DataContext as CommentItem;
                    if (commentItem != null)
                    {
                        if (isAdd)
                        {
                            pdfUtilViewModel.ExcuteAddComment(commentItem, textBox.Text);
                        }
                        else
                        {
                            pdfUtilViewModel.ExcuteCancelAddComment(commentItem);
                        }
                    }
                }
            }
        }

        private bool ThumbnailCommentCountClick(DependencyObject element)
        {
            if (element is Rectangle || element is TextBlock || element is StackPanel)
            {
                StackPanel stackPanel = VisualTreeHelperUtils.FindSelfOrAncestor<StackPanel>(element);
                if (stackPanel != null && stackPanel.Name == "CommentCountPanel")
                {
                    var pdfUtilViewModel = DataContext as PdfUtilViewModel;
                    if (pdfUtilViewModel != null)
                    {
                        pdfUtilViewModel.IsCommentPaneShow = true;

                        // In comment pane, if user selected sort by page, scroll to the corresponding page group
                        if (CommentSortComboBox.SelectedItem.Equals(Properties.Resources.COMMENT_SORT_PAGE))
                        {
                            var listViewItem = VisualTreeHelperUtils.FindAncestor<ListViewItem>(element);
                            if (listViewItem != null)
                            {
                                var iconItem = listViewItem.DataContext as IconItem;
                                var comment = iconItem.CommentItems.Count > 0 ? iconItem.CommentItems[0] : null;
                                if (comment != null)
                                {
                                    var scrollViewer = VisualTreeHelperUtils.FindVisualChild<ScrollViewer>(CommentList);
                                    var titleTextBlock = VisualTreeHelperUtils.FindVisualChild<TextBlock>(CommentList, o => o.Text == comment.PageNumberTitle);
                                    if (titleTextBlock != null && scrollViewer != null)
                                    {
                                        var groupGrid = VisualTreeHelperUtils.FindAncestor<Grid>(titleTextBlock);
                                        Point topLeft = groupGrid.TranslatePoint(new Point(0, 0), CommentList);
                                        double scrollPos = scrollViewer.VerticalOffset + topLeft.Y;

                                        scrollViewer.ScrollToVerticalOffset(scrollPos);
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private void CommentGroupGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var titleGrid = sender as Grid;
            if (titleGrid != null)
            {
                var groupGrid = VisualTreeHelperUtils.FindAncestor<Grid>(titleGrid);
                var scrollViewer = VisualTreeHelperUtils.FindAncestor<ScrollViewer>(titleGrid);
                if (groupGrid != null && scrollViewer != null)
                {
                    Point topLeft = groupGrid.TranslatePoint(new Point(0, 0), CommentList);
                    double scrollPos = scrollViewer.VerticalOffset + topLeft.Y;

                    scrollViewer.ScrollToVerticalOffset(scrollPos);
                }
            }
        }

        private void CommentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var commentList = sender as ListView;
                if (commentList != null)
                {
                    if (!pdfUtilViewModel.IsCommentOptionEnable)
                    {
                        // do not select comment item while commments is in loading status
                        if (commentList.SelectedIndex != -1)
                        {
                            commentList.SelectedIndex = -1;
                        }

                        _doNotScrollAfterCommentSelected = false;
                        return;
                    }

                    var pageItem = pdfUtilViewModel.CurPreviewIconItem; //current page
                    if (pageItem != null)
                    {
                        if (e.AddedItems != null && e.AddedItems.Count > 0)
                        {
                            var selectedItem = e.AddedItems[0] as CommentItem;
                            if (pageItem.GetPage().Number != selectedItem.PageNumber)
                            {
                                // if new selection is in the different page with current page, scroll to new page
                                if (pdfUtilViewModel.CurrentPdfDocument.Pages.Count < selectedItem.PageNumber || selectedItem.PageNumber == 0)
                                {
                                    _doNotScrollAfterCommentSelected = false;
                                    return;
                                }

                                pdfUtilViewModel.SelectThumbnailByPage(pdfUtilViewModel.CurrentPdfDocument.Pages[selectedItem.PageNumber]);
                            }

                            var newPageItem = pdfUtilViewModel.CurPreviewIconItem;
                            if (newPageItem != null && !_doNotScrollAfterCommentSelected && selectedItem.Annotation != null)
                            {
                                // scroll preview page to selected item
                                var showPoint = new Point(0, 0);
                                switch (newPageItem.GetPage().Rotate)
                                {
                                    case Aspose.Pdf.Rotation.None:
                                        showPoint = new Point(selectedItem.Annotation.Rect.LLX, selectedItem.Annotation.Rect.URY);
                                        break;
                                    case Aspose.Pdf.Rotation.on90:
                                        showPoint = new Point(selectedItem.Annotation.Rect.URX, selectedItem.Annotation.Rect.URY);
                                        break;
                                    case Aspose.Pdf.Rotation.on180:
                                        showPoint = new Point(selectedItem.Annotation.Rect.URX, selectedItem.Annotation.Rect.LLY);
                                        break;
                                    case Aspose.Pdf.Rotation.on270:
                                        showPoint = new Point(selectedItem.Annotation.Rect.LLX, selectedItem.Annotation.Rect.LLY);
                                        break;
                                }

                                var offset = pdfUtilViewModel.ConvertPagePointToPreviewPoint(showPoint, newPageItem.GetPage());
                                PreViewScrollViewer.ScrollToHorizontalOffset((offset.X - PdfHelper.ScrollLeaveDistance) > 0 ? offset.X - PdfHelper.ScrollLeaveDistance : 0);
                                PreViewScrollViewer.ScrollToVerticalOffset((offset.Y - PdfHelper.ScrollLeaveDistance) > 0 ? offset.Y - PdfHelper.ScrollLeaveDistance : 0);
                            }

                            _doNotScrollAfterCommentSelected = false;

                            // scroll comment pane to selected item
                            commentList.ScrollIntoView(selectedItem);
                        }
                    }
                    _doNotScrollAfterCommentSelected = false;
                }
            }
        }

        private void CommentCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (!pdfUtilViewModel.IsCommentOptionEnable)
                {
                    // do not select comment item while commments is in loading status
                    return;
                }

                var element = e.Source as CommentRectangle;
                if (element != null)
                {
                    var item = element.DataContext as CommentItem;
                    if (item != null)
                    {
                        if (pdfUtilViewModel.SelectedCommentItem == item)
                        {
                            e.Handled = true;
                            return;
                        }

                        if (Keyboard.FocusedElement is TextBox textBox && textBox.DataContext is CommentItem)
                        {
                            textBox.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
                        }

                        // do not scroll preview image if use select comment from preview image
                        _doNotScrollAfterCommentSelected = true;

                        pdfUtilViewModel.IsCommentPaneShow = true;
                        pdfUtilViewModel.SelectedCommentItem = item;

                        e.Handled = true;
                    }
                }
            }
        }

        private void SearchCommentTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var element = sender as PlaceholderTextBox;
            if (element != null && e.OriginalSource == element.ContentText)
            {
                if (string.IsNullOrEmpty(element.Text))
                {
                    ClearCommentSearch();
                }
                else
                {
                    SetCommentSearch();
                }
                e.Handled = true;
            }
        }

        private void SetCommentSearch()
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var view = (CollectionView)CollectionViewSource.GetDefaultView(CommentList.ItemsSource);
                if (view != null)
                {
                    view.Filter = CommentSearchFilter;

                    pdfUtilViewModel.IsInSearchingCommentMode = true;
                    pdfUtilViewModel.SearchingResultNumber = view.Count;
                }
            }
        }

        public void ClearCommentSearch()
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                var view = (CollectionView)CollectionViewSource.GetDefaultView(CommentList.ItemsSource);
                if (view != null)
                {
                    SearchCommentTextbox.Text = string.Empty;
                    view.Filter = null;

                    pdfUtilViewModel.IsInSearchingCommentMode = false;
                    pdfUtilViewModel.SearchingResultNumber = -1;
                }
            }
        }

        private bool CommentSearchFilter(object item)
        {
            var comment = item as CommentItem;
            var searchText = SearchCommentTextbox.Text;

            return string.IsNullOrEmpty(searchText) || comment != null &&
                (comment.User.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || comment.Content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void CommentSortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pdfUtilViewModel = DataContext as PdfUtilViewModel;
            if (pdfUtilViewModel != null)
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is string)
                {
                    pdfUtilViewModel.ChangeCommentCollectView(e.AddedItems[0] as string);
                }
            }
        }

        #endregion
    }
}
