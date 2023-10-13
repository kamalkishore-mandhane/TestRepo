using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Text;
using PdfUtil.Util;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Model;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;

using WindowsPoint = System.Windows.Point;
using WindowsControl = System.Windows.Controls.Control;
using FileIcon = PdfUtil.WPFUI.Utils.FileIcon;

namespace PdfUtil.WPFUI.ViewModel
{
    class PdfUtilViewModel : ViewModelBase
    {
        public delegate void ScrollVerticalOffsetEventHandler(double offset);
        public event ScrollVerticalOffsetEventHandler ScrollVerticalOffsetEvent;
        public delegate void PageUpdateFinishEventHandler();
        public event PageUpdateFinishEventHandler PageUpdateFinishEvent;
        public delegate void WaitProcessFinishEventHandler();
        public event WaitProcessFinishEventHandler WaitProcessFinishEvent;
        public delegate void ProgressCancelEventHander();
        public event ProgressCancelEventHander ProgressCancelEvent;
        public delegate void SearchingTextEventHandler(SearchingTextEventArgs e);
        public event SearchingTextEventHandler SearchingTextEvent;
        public delegate void CurrentPdfDocumentChangedEventHander();
        public event CurrentPdfDocumentChangedEventHander CurrentPdfDocumentChangedEventEvent;

        private PdfUtilView _pdfUtilView;

        private FakeRibbonTabViewModel _fakeRibbonTabViewModel;
        private ThumbnailPaneContextMenuViewModel _thumbnnailPaneContextMenuViewModel;
        private BookmarksPaneContextMenuViewModel _bookmarksPaneContextMenuViewModel;
        private PreviewPaneContextMenuViewModel _previewPaneContextMenuViewModel;
        private LockPDFViewModel _lockPDFViewModel;

        private Document _currentPdfDocument;
        private PdfLockStatus _currentPdfLockStatus;
        private BitmapImage _previewPageImage;
        private IconItem _curPreviewIconItem;
        private int _previewPageNumber;
        private System.Windows.Media.Brush _findBackgoundColor;
        private System.Windows.Media.Brush _findBackgoundBorderColor;
        private System.Windows.Media.Brush _findTextSelectedColor;
        private System.Windows.Media.Brush _findTextSelectedBorderColor;
        private string _curFindWord;
        private bool _firstTimeFound;
        private int _curPageFindWordIndex = -1;
        private int _curPageFindWordCount = -1;
        private Dictionary<Page, List<TextFragment>> _searchSet;
        private PdfFileInfo _currentPdfFileInfo;
        private PdfFilePasswordInfo _currentPdfFilePasswordInfo;

        private bool _isSelecting;
        private bool _isSelected;
        private bool _isIconSourcesChanging;
        private double _thumbnailZoom = 1;
        private double _curPreviewScrollVerticalOffset;
        private bool _isCommandExecuting;
        private bool _isBookmarkSelectionChanged = false;
        private bool _isBookmarkOptionEnable = true;
        private bool _isDocChanged = false;
        private bool _isRefreshPreviewByInitDoc = false;
        private bool? _isPdfSignedOrCertified = null;
        private bool _isNeedWaitProcessFinish = false;
        private ProgressStatus _curExtractStatus = ProgressStatus.None;
        private ObservableCollection<RecentFile> _recentList = new ObservableCollection<RecentFile>();
        private RecentFile _selectFileInRecentList;
        private WindowsPoint _selectRectLeftTopPoint;
        private WindowsPoint _selectRectRightBottomPoint;
        private WindowsPoint _mouseButtonChoosedPoint;
        private WindowsPoint _thumbnailContextMenuDownPoint;
        private DrawingCollection _drawingGroupChild;
        private DrawingImage _searchCanvasImage;

        private Task _delayLoadWinzipSharedService;
        private bool? _loadWinzipSharedServiceSuccess;
        private JobManagement _jobManagement;
        private static Mutex recentFileXmlMutex = new Mutex(false, "recentFileXmlMutex");
        private static Mutex createSessionMutex = new Mutex(false, "createSessionMutex");
        private static int PreloadingPageCount = 20;
        private CancellationTokenSource _loadingCts;

        private string _extractTempFolder;

        private bool _isSignatureBarShow;
        private ObservableCollection<SignatureItem> _signatureList = new ObservableCollection<SignatureItem>();
        private SignatureItem _selectedSignature;
        public readonly SignatureItem MarkXSignature;
        public readonly SignatureItem MarkYSignature;

        private bool _isCommentPaneShow;
        private bool _isCommentOptionEnable;
        private ObservableCollection<CommentItem> _commentItems = new ObservableCollection<CommentItem>();
        private CommentItem _selectedCommentItem;
        private int _searchingResultNumber = -1;

        public PdfUtilViewModel(PdfUtilView view, Action<bool> adjustPaneCursor) : base(view.WindowHandle, adjustPaneCursor)
        {
            _pdfUtilView = view;
            _findBackgoundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(90, 255, 238, 128));
            _findBackgoundBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 238, 128));
            _findTextSelectedColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(90, 198, 185, 99));
            _findTextSelectedBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 198, 185, 99));

            Icon = new IconSource();
            RootBookmarkTree = new RootBookmarkTree(this);
            ProgressCancelEvent += PdfUtilViewModel_ProgressCancelEvent;
            CurrentPdfDocumentChangedEventEvent += CurrentPdfDocumentChanged;
            CurrentOpenedItem = PdfHelper.InitWzCloudItem();

            _signatureList.Add(new SignatureItem());
            _selectedSignature = _signatureList[0];
            var imageBrushX = Application.Current.TryFindResource("MarkXDrawingBrush") as DrawingBrush;
            var imageBrushY = Application.Current.TryFindResource("MarkYDrawingBrush") as DrawingBrush;

            var bitmapX = PdfHelper.BitmapSourceFromBrush(imageBrushX, 28, 28);
            MarkXSignature = new SignatureItem(bitmapX);

            var bitmapY = PdfHelper.BitmapSourceFromBrush(imageBrushY, 28, 28);
            MarkYSignature = new SignatureItem(bitmapY);
        }

        public Func<Func<Task>, Func<Exception, int>, Task> Executor
        {
            get
            {
                return _executor;
            }
        }

        public PdfUtilView PdfUtilView
        {
            get
            {
                return _pdfUtilView;
            }
        }

        public Document CurrentPdfDocument
        {
            get
            {
                return _currentPdfDocument;
            }
            set
            {
                _currentPdfDocument = value;
                Notify(nameof(IsContextVisible));
                CurrentPdfDocumentChangedEventEvent();
            }
        }

        public int CurPageFindWordIndex
        {
            get
            {
                return _curPageFindWordIndex;
            }
            set
            {
                if (_curPageFindWordIndex != value)
                {
                    _curPageFindWordIndex = value;
                }
            }
        }

        public int CurPageFindWordCount
        {
            get
            {
                return _curPageFindWordCount;
            }
        }

        public Dictionary<Page, List<TextFragment>> SearchSet
        {
            get
            {
                if (_searchSet == null)
                {
                    _searchSet = new Dictionary<Page, List<TextFragment>>();
                }
                return _searchSet;
            }
        }

        // CurrentPdfFileInfo cannot get the right password information in some circumstances (for example: after lock or unlock a file)
        // so we add CurrentPdfFilePasswordInfo to store password-related information separately
        public PdfFilePasswordInfo CurrentPdfFilePasswordInfo
        {
            get
            {
                return _currentPdfFilePasswordInfo;
            }
            set
            {
                if (!_currentPdfFilePasswordInfo.Equals(value))
                {
                    _currentPdfFilePasswordInfo = value;
                }
            }
        }

        public PdfFileInfo CurrentPdfFileInfo
        {
            get
            {
                return _currentPdfFileInfo;
            }
            set
            {
                if (_currentPdfFileInfo != value)
                {
                    _currentPdfFileInfo = value;
                }
            }
        }

        public string CurrentPdfFileName
        {
            get;
            set;
        }

        public WzCloudItem4 CurrentOpenedItem
        {
            get;
            set;
        }

        public bool IsNewPDF
        {
            get;
            set;
        }

        public PdfLockStatus CurrentPdfLockStatus
        {
            get
            {
                return _currentPdfLockStatus;
            }
            set
            {
                if (!_currentPdfLockStatus.Equals(value))
                {
                    _currentPdfLockStatus = value;
                    LockPDFViewModel.ParserPasswordAndPermission(value);
                }
            }
        }

        public PdfLockStatus CurrentPdfSourceLockStatus
        {
            // After you finish "Lock PDF", the CurrentPdfLockStatus will be changed,
            // but the lock password will not take effect immediately until the user execute "Save" or "Save AS"
            // so before the user execute "Save" or "Save AS", we need to save the old LockStatus, because we might use it.
            get;
            set;
        }

        public bool IsCurrentPdfLockChanged
        {
            get;
            set;
        }

        public FakeRibbonTabViewModel FakeRibbonTabViewModel
        {
            get
            {
                if (_fakeRibbonTabViewModel == null)
                {
                    _fakeRibbonTabViewModel = new FakeRibbonTabViewModel(this);
                }

                return _fakeRibbonTabViewModel;
            }
        }

        public ThumbnailPaneContextMenuViewModel ThumbnailPaneContextMenuViewModel
        {
            get
            {
                if (_thumbnnailPaneContextMenuViewModel == null)
                {
                    _thumbnnailPaneContextMenuViewModel = new ThumbnailPaneContextMenuViewModel(this);
                }

                return _thumbnnailPaneContextMenuViewModel;
            }
        }

        public BookmarksPaneContextMenuViewModel BookmarksPaneContextMenuViewModel
        {
            get
            {
                if (_bookmarksPaneContextMenuViewModel == null)
                {
                    _bookmarksPaneContextMenuViewModel = new BookmarksPaneContextMenuViewModel(this);
                }

                return _bookmarksPaneContextMenuViewModel;
            }
        }

        public PreviewPaneContextMenuViewModel PreviewPaneContextMenuViewModel
        {
            get
            {
                if (_previewPaneContextMenuViewModel == null)
                {
                    _previewPaneContextMenuViewModel = new PreviewPaneContextMenuViewModel(this);
                }

                return _previewPaneContextMenuViewModel;
            }
        }

        public LockPDFViewModel LockPDFViewModel
        {
            get
            {
                if (_lockPDFViewModel == null)
                {
                    _lockPDFViewModel = new LockPDFViewModel();
                }

                return _lockPDFViewModel;
            }
            set
            {
                if (_lockPDFViewModel != value)
                {
                    _lockPDFViewModel = value;
                }
            }
        }

        public System.Windows.Controls.ListView ThumbnailListView
        {
            get;
            set;
        }

        public System.Windows.Controls.TreeView BookmarkTreeView
        {
            get;
            set;
        }

        public System.Windows.Controls.Image PreviewImage
        {
            get;
            set;
        }

        public RootBookmarkTree RootBookmarkTree
        {
            get;
            set;
        }

        public IconSource Icon
        {
            get;
            set;
        }

        public BitmapImage PreviewPageImage
        {
            get
            {
                return _previewPageImage;
            }
            set
            {
                if (_previewPageImage != value)
                {
                    _previewPageImage = value;
                    Notify(nameof(PreviewPageImage));
                }
            }
        }

        public IconItem CurPreviewIconItem
        {
            get
            {
                return _curPreviewIconItem;
            }
            set
            {
                if (_curPreviewIconItem != value)
                {
                    _curPreviewIconItem = value;
                }
                RefreshPreviewPageNumber();
            }
        }

        public double CurPreviewScrollVerticalOffset
        {
            get
            {
                return _curPreviewScrollVerticalOffset;
            }
            set
            {
                if (_curPreviewScrollVerticalOffset != value)
                {
                    _curPreviewScrollVerticalOffset = value;
                }
            }

        }

        public WindowsPoint PreviewPageSize
        {
            get
            {
                return new WindowsPoint(PreviewImage.ActualWidth, PreviewImage.ActualHeight);
            }
        }

        public bool IsRectSelecting
        {
            get
            {
                return _isSelecting;
            }
            set
            {
                if (_isSelecting != value)
                {
                    _isSelecting = value;
                    Notify(nameof(IsRectSelecting));
                }
            }
        }

        public bool IsRectSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    Notify(nameof(IsRectSelected));
                }
            }
        }

        public bool IsIconSourcesChanging
        {
            get
            {
                return _isIconSourcesChanging;
            }
            set
            {
                if (_isIconSourcesChanging != value)
                {
                    _isIconSourcesChanging = value;
                }
            }
        }

        public double ThumbnailZoom
        {
            get
            {
                return _thumbnailZoom;
            }
            set
            {
                if (_thumbnailZoom != value)
                {
                    _thumbnailZoom = value;
                    Notify(nameof(ThumbnailZoom));

                    HideCommentCountInThumbnail = _thumbnailZoom < 1;
                    Notify(nameof(HideCommentCountInThumbnail));
                }
            }
        }

        public bool HideCommentCountInThumbnail
        {
            get;
            set;
        }

        public bool IsPdfSignedOrCertified
        {
            get
            {
                if (_isPdfSignedOrCertified != null)
                {
                    return _isPdfSignedOrCertified == true;
                }

                try
                {
                    var signature = new PdfFileSignature(CurrentPdfDocument);
                    _isPdfSignedOrCertified = signature.IsCertified || signature.IsContainSignature();
                }
                catch
                {
                    // An error may be catched when there are invalid signature in pdf file, set _isPdfSignedOrCertified true;
                    _isPdfSignedOrCertified = true;
                }
                return _isPdfSignedOrCertified == true;
            }
        }

        public WindowsPoint SelectRectLeftTopPoint
        {
            get
            {
                return _selectRectLeftTopPoint;
            }
            set
            {
                if (_selectRectLeftTopPoint != value)
                {
                    _selectRectLeftTopPoint = value;
                    Notify(nameof(SelectRectLeftTopPoint));
                    Notify(nameof(SelectRectSize));
                }
            }
        }

        public WindowsPoint SelectRectRightBottomPoint
        {
            get
            {
                return _selectRectRightBottomPoint;
            }
            set
            {
                if (_selectRectRightBottomPoint != value)
                {
                    _selectRectRightBottomPoint = value;
                    Notify(nameof(SelectRectRightBottomPoint));
                    Notify(nameof(SelectRectSize));
                }
            }
        }

        public WindowsPoint SelectRectSize
        {
            get
            {
                return new WindowsPoint(SelectRectRightBottomPoint.X - SelectRectLeftTopPoint.X, SelectRectRightBottomPoint.Y - SelectRectLeftTopPoint.Y);
            }
        }

        public WindowsPoint MouseButtonChoosedPoint
        {
            get
            {
                return _mouseButtonChoosedPoint;
            }
            set
            {
                if (_mouseButtonChoosedPoint != value)
                {
                    _mouseButtonChoosedPoint = value;
                    Notify(nameof(MouseButtonChoosedPoint));
                }
            }
        }

        public WindowsPoint ThumbnailContextMenuDownPoint
        {
            get
            {
                return _thumbnailContextMenuDownPoint;
            }
            set
            {
                if (_thumbnailContextMenuDownPoint != value)
                {
                    _thumbnailContextMenuDownPoint = value;
                }
            }
        }

        public DrawingCollection DrawingGroupChild
        {
            get
            {
                if (_drawingGroupChild == null)
                {
                    _drawingGroupChild = new DrawingCollection();
                }

                return _drawingGroupChild;
            }
            set
            {
                if (_drawingGroupChild != value)
                {
                    _drawingGroupChild = value;
                    Notify(nameof(DrawingGroupChild));
                }
            }
        }

        public DrawingImage SearchCanvasImage
        {
            get
            {
                return _searchCanvasImage;
            }
            set
            {
                if (_searchCanvasImage != value)
                {
                    _searchCanvasImage = value;
                    Notify(nameof(SearchCanvasImage));
                }
            }
        }

        public string PDFTitle
        {
            get
            {
                string title = Path.GetFileName(CurrentPdfFileName);

                if (!string.IsNullOrEmpty(title))
                {
                    title = Properties.Resources.PDF_UTILITY_TITLE + " - " + title;
                }
                else
                {
                    title = Properties.Resources.PDF_UTILITY_TITLE + " - " + Properties.Resources.DEFAULT_PDF_TITLE_NAME;
                }

                if (IsDocChanged)
                {
                    title = title + " *";
                }

                return title;
            }
        }

        public int PreviewPageNumber
        {
            get
            {
                return _previewPageNumber;
            }
            set
            {
                if (_previewPageNumber != value)
                {
                    _previewPageNumber = value;
                    _pdfUtilView.SetViewPageTextBox(value);
                }
                Notify(nameof(IsPageUpEnable));
                Notify(nameof(IsPageDownEnable));
            }
        }

        public int TotalPageCount
        {
            get
            {
                if (CurrentPdfDocument != null && CurrentPdfDocument.Pages != null)
                {
                    return CurrentPdfDocument.Pages.Count;
                }
                return 0;
            }
        }

        public bool IsPageUpEnable
        {
            get
            {
                if (CurrentPdfDocument == null)
                {
                    return true;
                }

                if (CurrentPdfDocument != null && CurrentPdfDocument.Pages != null && PreviewPageNumber > 1)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsPageDownEnable
        {
            get
            {
                if (CurrentPdfDocument == null)
                {
                    return true;
                }

                if (CurrentPdfDocument != null && CurrentPdfDocument.Pages != null && PreviewPageNumber < CurrentPdfDocument.Pages.Count)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsBookmarkSelectionChanged
        {
            get
            {
                return _isBookmarkSelectionChanged;
            }
            set
            {
                if (_isBookmarkSelectionChanged != value)
                {
                    _isBookmarkSelectionChanged = value;
                }
            }
        }

        public bool IsBookmarkOptionEnable
        {
            get
            {
                return _isBookmarkOptionEnable;
            }
            set
            {
                if (_isBookmarkOptionEnable != value)
                {
                    _isBookmarkOptionEnable = value;
                    Notify(nameof(IsBookmarkOptionEnable));
                }
            }
        }

        public bool IsDocChanged
        {
            get
            {
                return _isDocChanged;
            }
        }

        public bool IsContextVisible
        {
            get
            {
                return CurrentPdfDocument != null;
            }
        }

        public bool IsNeedWaitProcessFinish
        {
            get
            {
                return _isNeedWaitProcessFinish;
            }
            set
            {
                if (_isNeedWaitProcessFinish != value)
                {
                    _isNeedWaitProcessFinish = value;
                }
            }
        }

        public ObservableCollection<RecentFile> RecentList
        {
            get
            {
                return _recentList;
            }
            set
            {
                if (_recentList != value)
                {
                    string recentFileTooltip = null;
                    if (SelectFileInRecentList != null)
                    {
                        recentFileTooltip = SelectFileInRecentList.RecentFileTooltip; // preserve the selected file info
                    }

                    _recentList = value;
                    Notify(nameof(RecentList));

                    if (recentFileTooltip != null)
                    {
                        var newItem = RecentList.FirstOrDefault(x => x.RecentFileTooltip == recentFileTooltip);
                        if (newItem != null)
                        {
                            SelectFileInRecentList = newItem; //reselected the file selected before
                        }
                    }
                }
            }
        }

        public RecentFile SelectFileInRecentList
        {
            get
            {
                return _selectFileInRecentList;
            }
            set
            {
                if (_selectFileInRecentList != value)
                {
                    _selectFileInRecentList = value;
                    Notify(nameof(SelectFileInRecentList));
                }
            }
        }

        public ProgressStatus CurExtractStatus
        {
            get
            {
                return _curExtractStatus;
            }
            set
            {
                if (_curExtractStatus != value)
                {
                    _curExtractStatus = value;
                }
            }
        }

        public CancellationTokenSource LoadingCts
        {
            get
            {
                return _loadingCts;
            }
        }

        public bool AlreadySetDefault
        {
            get
            {
                return IntegrationDialog.IsPdfUtilDefault();
            }
        }

        public bool IsSignatureBarShow
        {
            get => _isSignatureBarShow;
            set
            {
                if (_isSignatureBarShow != value)
                {
                    _isSignatureBarShow = value;
                    Notify(nameof(IsSignatureBarShow));
                    _pdfUtilView.SetSignatureBarMargin();
                }
            }
        }

        public ObservableCollection<SignatureItem> SignatureList
        {
            get => _signatureList;
            set
            {
                if (_signatureList != value)
                {
                    _signatureList = value;
                    Notify(nameof(SignatureList));
                }
            }
        }

        public SignatureItem SelectedSignature
        {
            get => _selectedSignature;
            set
            {
                if (_selectedSignature != value)
                {
                    _selectedSignature = value;
                    Notify(nameof(SelectedSignature));
                }
            }
        }

        public SignatureItem AddSignature
        {
            get;
            set;
        }

        public bool IsCommentPaneShow
        {
            get => _isCommentPaneShow;
            set
            {
                if (_isCommentPaneShow != value)
                {
                    _isCommentPaneShow = value;
                    _pdfUtilView.ArrangeRightColumnWidth(_isCommentPaneShow);
                    Notify(nameof(IsCommentPaneShow));
                    Notify(nameof(PageCommentTitle));
                }
            }
        }

        public bool IsCommentOptionEnable
        {
            get
            {
                return _isCommentOptionEnable;
            }
            set
            {
                if (_isCommentOptionEnable != value)
                {
                    _isCommentOptionEnable = value;
                    Notify(nameof(IsCommentOptionEnable));
                }
            }
        }

        public string PageCommentTitle
        {
            get
            {
                if (IsInSearchingCommentMode)
                {
                    return string.Format(Properties.Resources.COMMENT_PANE_TITLE, string.Format("{0}/{1}", SearchingResultNumber, CommentItems.Count));
                }
                else
                {
                    return string.Format(Properties.Resources.COMMENT_PANE_TITLE, CommentItems.Count);
                }
            }
        }

        public ObservableCollection<CommentItem> CommentItems
        {
            get => _commentItems;
            set
            {
                if (_commentItems != value)
                {
                    _commentItems = value;
                    Notify(nameof(CommentItems));
                }
            }
        }

        public CommentItem SelectedCommentItem
        {
            get => _selectedCommentItem;
            set
            {
                if (_selectedCommentItem != value)
                {
                    if (_selectedCommentItem != null)
                    {
                        _selectedCommentItem.IsSelected = false;
                    }

                    if (value != null)
                    {
                        value.IsSelected = true;
                    }

                    _selectedCommentItem = value;
                    Notify(nameof(SelectedCommentItem));
                }
            }
        }

        public bool IsInSearchingCommentMode
        {
            get;
            set;
        }

        public int SearchingResultNumber
        {
            get => _searchingResultNumber;
            set
            {
                // set SearchingResultNumber to -1 means that currently we are not in comment searching mode
                if (_searchingResultNumber != value)
                {
                    _searchingResultNumber = value;
                    Notify(nameof(PageCommentTitle));
                    Notify(nameof(IsSearchCommentNotFound));
                }
            }
        }

        public bool IsSearchCommentNotFound
        {
            get => IsInSearchingCommentMode ? SearchingResultNumber == 0 : false;
        }

        public void NotifyDefaultSet()
        {
            Notify(nameof(AlreadySetDefault));
        }

        public void LoadRecentFilesXML()
        {
            try
            {
                recentFileXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserRecentFilesPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(ObservableCollection<RecentFile>));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        RecentList = (ObservableCollection<RecentFile>)formatter.Deserialize(memoryStream);
                        foreach (var item in RecentList)
                        {
                            item.UpdateFileInfo();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally 
            {
                recentFileXmlMutex.ReleaseMutex();
            }
        }

        public void SaveRecentFilesXML()
        {
            try
            {
                recentFileXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserRecentFilesPath;
                var formatter = new XmlSerializer(typeof(ObservableCollection<RecentFile>));
                using (var stream = File.Create(path))
                {
                    formatter.Serialize(stream, RecentList);
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                recentFileXmlMutex.ReleaseMutex();
            }
        }

        public void RefreshPDFTitle()
        {
            Notify(nameof(PDFTitle));
        }

        public void ClearDocPdfFileSignature()
        {
            _isPdfSignedOrCertified = null;
        }

        public void SendProcessFinishedEvent()
        {
            WaitProcessFinishEvent();
        }

        public void ShowFilesMenuButtonContextMenu()
        {
            _pdfUtilView.ShowFilesMenuButtonContextMenu();
        }

        private void CurrentPdfDocumentChanged()
        {
            CurPreviewIconItem = null;
            PreviewPageImage = null;
            ClearDocPdfFileSignature();
            ResetDocChangedState();
            PdfUtilView.CurCaretIndex = -1; ;
        }

        public void ClearRectangleSelect()
        {
            IsRectSelecting = false;
            IsRectSelected = false;
            ResetSelectedRectangle();
        }

        public void ResetSelectedRectangle()
        {
            SelectRectLeftTopPoint = new WindowsPoint(0, 0);
            SelectRectRightBottomPoint = new WindowsPoint(0, 0);
        }

        public void ResetMouseButtonChoosedPoint()
        {
            MouseButtonChoosedPoint = new WindowsPoint(0, 0);
        }

        public WindowsPoint GetPageSize(Page page, bool considerRotation)
        {
            if (!considerRotation || page.Rotate == Aspose.Pdf.Rotation.None || page.Rotate == Aspose.Pdf.Rotation.on180)
            {
                return new WindowsPoint(page.MediaBox.Width, page.MediaBox.Height);
            }
            else
            {
                return new WindowsPoint(page.MediaBox.Height, page.MediaBox.Width);
            }
        }

        public WindowsPoint CalculateSelectedAreaOnPage(WindowsPoint point, Page page)
        {
            var pageSize = GetPageSize(page, true);
			
            if (page.Rotate == Aspose.Pdf.Rotation.on90) 
            {
                return new WindowsPoint(point.Y * pageSize.Y / PreviewPageSize.Y, point.X * pageSize.X / PreviewPageSize.X);
            }

            if (page.Rotate == Aspose.Pdf.Rotation.on180)
            {
                return new WindowsPoint((PreviewPageSize.X - point.X) * pageSize.X / PreviewPageSize.X, point.Y * pageSize.Y / PreviewPageSize.Y);
            }

            if (page.Rotate == Aspose.Pdf.Rotation.on270)
            {
                return new WindowsPoint((PreviewPageSize.Y - point.Y) * pageSize.Y/ PreviewPageSize.Y, (PreviewPageSize.X - point.X) * pageSize.X/ PreviewPageSize.X);
            }

            return new WindowsPoint(point.X * pageSize.X / PreviewPageSize.X, (PreviewPageSize.Y - point.Y) * pageSize.Y / PreviewPageSize.Y);
        }

        public WindowsPoint ConvertPagePointToPreviewPoint(WindowsPoint point, Page page)
        {
            var pageSize = GetPageSize(page, true);

            if (page.Rotate == Aspose.Pdf.Rotation.on90)
            {
                return new WindowsPoint(point.Y * PreviewPageSize.Y / pageSize.Y, point.X * PreviewPageSize.X / pageSize.X);
            }

            if (page.Rotate == Aspose.Pdf.Rotation.on180)
            {
                return new WindowsPoint(Math.Abs(pageSize.X - point.X) * PreviewPageSize.X / pageSize.X, point.Y * PreviewPageSize.Y / pageSize.Y);
            }

            if (page.Rotate == Aspose.Pdf.Rotation.on270)
            {
                return new WindowsPoint(Math.Abs(pageSize.X - point.Y) * PreviewPageSize.X / pageSize.X, Math.Abs(pageSize.Y - point.X) * PreviewPageSize.Y / pageSize.Y);
            }

            return new WindowsPoint(point.X * PreviewPageSize.X / pageSize.X, Math.Abs(pageSize.Y - point.Y) * PreviewPageSize.Y / pageSize.Y);
        }

        public double ConvertPageLengthToPreviewLength(double length, Page page)
        {
            var pageSize = GetPageSize(page, true);
            return length * PreviewPageSize.X / pageSize.X;
        }

        public WindowsPoint CalculateBookmarkDestinationOnPage(WindowsPoint point, Page page)
        {
            var pageSize = GetPageSize(page, false);
            return new WindowsPoint(point.X * pageSize.X / PreviewPageSize.X, point.Y * pageSize.Y / PreviewPageSize.Y);
        }

        public WindowsPoint ConvertBookmarkDestinationToPreviewPoint(WindowsPoint point, Page page)
        {
            var pageSize = GetPageSize(page, false);
            return new WindowsPoint(point.X * PreviewPageSize.X / pageSize.X, point.Y * PreviewPageSize.Y / pageSize.Y);
        }

        public void PreviewPageNeedUpdate(IconItem previewIconItem = null)
        {
            ClearAllSelectionOnPage();
            SetCurrentPreviewPage(previewIconItem);
        }

        public void ClearAllSelectionOnPage()
        {
            ClearRectangleSelect();
            ResetMouseButtonChoosedPoint();
        }

        public Task GetPageImage(Page page, CancellationToken cancellationToken, MemoryStream pageStream = null)
        {
            return Task.Factory.StartNew(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (page != null)
                {
                    try
                    {
                        Stream memory = null;
                        if (pageStream != null)
                        {
                            memory = new MemoryStream(pageStream.GetBuffer());
                        }
                        else
                        {
                            memory = page.ConvertToPNGMemoryStream();
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            memory.Dispose();
                            return;
                        }

                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = memory;
                            bitmap.EndInit();

                            memory.Close();
                            memory.Dispose();
                            PreviewPageImage = bitmap;
                        }));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        GC.Collect();
                    }
                }

                return;
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public void SelectThumbnailByCurSelectedBookmark()
        {
            IsBookmarkSelectionChanged = true;
            _curPreviewIconItem.IsPreviewSelected = false;
            CurPreviewIconItem = GetPreviewIconItemFromBookmark();
            if (_curPreviewIconItem != null)
            {
                if (!SelectThumbnailByPage(_curPreviewIconItem.GetPage()))
                {
                    SetCurrentPreviewPage(_curPreviewIconItem);
                }
            }
        }

        public void SetCurrentPreviewPage(IconItem needPreviewIconItem)
        {
            PdfUtilView.IsPageUpdating = true;
            CurPreviewIconItem = needPreviewIconItem != null ? needPreviewIconItem : GetCurSelectedIcon();

            if (_curPreviewIconItem != null)
            {
                if (_curPreviewIconItem != null && _curPreviewIconItem.PageStream != null)
                {
                    GetPageImage(_curPreviewIconItem.GetPage(), _pdfUtilView.Cts.Token, _curPreviewIconItem.PageStream).ContinueWith(task =>
                    {
                        PageUpdateFinishEvent?.Invoke();
                        if (_isRefreshPreviewByInitDoc)
                        {
                            PdfUtilView.CalScaleForFitToSize(true);
                            _isRefreshPreviewByInitDoc = false;
                        }

                        PdfUtilView.UpdateCommentsOnCanvas();
                        PdfUtilView.IsPageUpdating = false;
                        _curPreviewIconItem.IsPreviewSelected = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    GetPageImage(_curPreviewIconItem.GetPage(), _pdfUtilView.Cts.Token).ContinueWith(task =>
                    {
                        PageUpdateFinishEvent?.Invoke();
                        if (_isRefreshPreviewByInitDoc)
                        {
                            PdfUtilView.CalScaleForFitToSize(true);
                            _isRefreshPreviewByInitDoc = false;
                        }

                        PdfUtilView.UpdateCommentsOnCanvas();
                        PdfUtilView.IsPageUpdating = false;
                        _curPreviewIconItem.IsPreviewSelected = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            else
            {
                return;
            }

            if (IsBookmarkSelectionChanged)
            {
                IsBookmarkSelectionChanged = false;
                var offset = GetPreviewPageTopInfo();
                offset = ConvertBookmarkDestinationToPreviewPoint(new WindowsPoint(0, offset), CurPreviewIconItem.GetPage()).Y;
                if (offset != -1 && !double.IsNaN(offset))
                {
                    ScrollVerticalOffsetEvent(offset);
                }
            }
        }

        public int GetPageIndex(Page page)
        {
            return _currentPdfDocument.Pages.IndexOf(page);
        }

        public void RefreshWhenPageChanged()
        {
            NotifyTotalPageCount();
            RefreshIconName();
        }

        public void NotifyTotalPageCount()
        {
            Notify(nameof(TotalPageCount));
        }

        public void CancelLoadingExecuteAction()
        {
            if (_loadingCts != null && _loadingCts.Token.CanBeCanceled && !_loadingCts.Token.IsCancellationRequested)
            {
                _loadingCts.Cancel();
            }
        }

        public void RefreshIconName()
        {
            foreach (var icon in Icon.IconSources)
            {
                var iconItem = icon as IconItem;
                if (iconItem == null)
                {
                    continue;
                }

                iconItem.SetName((Icon.IconSources.IndexOf(icon) + 1).ToString());
            }
        }

        public void LoadThumbnailAndBookmark(PDFInfo pdfFileInfo, bool isNew, bool isChanged)
        {
            _loadingCts = new CancellationTokenSource();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                PdfUtilView.AdjustBookmarkTabCursor(false);
                IsBookmarkOptionEnable = false;
                PdfUtilView.AdjustCommentPaneCursor(false);
                IsCommentOptionEnable = false;
            }));

            Task.Factory.StartNewTCS(tcs =>
            {
                LoadThumbnailsAndCommentsInBeginning(pdfFileInfo, CurrentPdfDocument.Pages, _loadingCts.Token);
                RefreshPDFTitle();
                IsNewPDF = isNew;
                if (isChanged)
                {
                    NotifyDocChanged();
                }

                tcs.TrySetResult();
            }).ContinueWith(task =>
            {
                LoadRestThumbnailsAndComments(pdfFileInfo, CurrentPdfDocument.Pages, _loadingCts.Token);
                if (!_loadingCts.Token.IsCancellationRequested)
                {
                    LoadBookmarkSource(CurrentPdfDocument.Outlines, 0, _loadingCts.Token);
                }
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public void LoadThumbnailsAndCommentsInBeginning(PDFInfo pdfFileInfo, PageCollection pages, CancellationToken cancellationToken)
        {
            Icon.IconSources.Clear();
            var fileIcon = new FileIcon();
            var imageSource = fileIcon.GetImage(pdfFileInfo.filePath, 1024);
            foreach (var page in pages)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var index = pages.IndexOf(page);
                if (index > PreloadingPageCount)
                {
                    break;
                }

                string pageNumber = (Icon.IconSources.Count + 1).ToString();
                var width = CurrentPdfFileInfo.GetPageWidth(page.Number);
                var height = CurrentPdfFileInfo.GetPageHeight(page.Number);
                var iconItem = new IconItem(page, width, height, cancellationToken);
                iconItem.UpdateThumbnailEvent += IconItem_UpdateThumbnailEvent;
                iconItem.SetIcon(imageSource);
                iconItem.SetName(pageNumber);
                iconItem.CommentItems = LoadCommentSourceFromPage(page);

                Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
                {
                    Icon.IconSources.Add(iconItem);
                }));
            }
        }

        public void LoadRestThumbnailsAndComments(PDFInfo pdfFileInfo, PageCollection pages, CancellationToken cancellationToken)
        {
            if (pages.Count <= PreloadingPageCount)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    IsCommentOptionEnable = true;
                    PdfUtilView.AdjustCommentPaneCursor(true);
                }));

                return;
            }

            var fileIcon = new FileIcon();
            var imageSource = fileIcon.GetImage(pdfFileInfo.filePath, 256);

            var time = DateTime.Now;
            var tempui = new UIObjects();
            var pagec = Icon.IconSources.Count + 1;
            foreach (var page in pages)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var index = pages.IndexOf(page);
                if (index == 1)
                {
                    continue;
                }

                if (index > PreloadingPageCount)
                {
                    string pageNumber = (pagec++).ToString();
                    var width = CurrentPdfFileInfo.GetPageWidth(page.Number);
                    var height = CurrentPdfFileInfo.GetPageHeight(page.Number);
                    var iconItem = new IconItem(page, width, height, cancellationToken);
                    iconItem.UpdateThumbnailEvent += IconItem_UpdateThumbnailEvent;
                    iconItem.SetIcon(imageSource);
                    iconItem.SetName(pageNumber);

                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            iconItem.CommentItems = LoadCommentSourceFromPage(page);
                            Icon.IconSources.Add(iconItem);
                        }
                    }));
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                IsCommentOptionEnable = true;
                PdfUtilView.AdjustCommentPaneCursor(true);
            }));

            foreach (var icon in Icon.IconSources)
            {
                var iconItem = icon as IconItem;
                if (iconItem == null)
                {
                    continue;
                }

                IconThumbnailManager.AddBackgroundRender(iconItem);
            }
        }

        private void IconItem_UpdateThumbnailEvent(bool isCompleted)
        {
            SetThumbnailCursorBusy(!isCompleted, false);
        }

        public void SetThumbnailCursorBusy(bool isBusy, bool isFromCommand = true)
        {
            if (isBusy)
            {
                if (isFromCommand)
                {
                    _isCommandExecuting = true;
                }

                ThumbnailListView.Cursor = System.Windows.Input.Cursors.Wait;
            }
            else
            {
                if (isFromCommand)
                {
                    _isCommandExecuting = false;
                }

                if (_isCommandExecuting)
                {
                    return;
                }

                ThumbnailListView.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        public void LoadBookmarkSource(OutlineCollection outlines, int curBookmarkIndex, CancellationToken cancellationToken)
        {
            RootBookmarkTree.InitBookmarkItemsList(RootBookmarkTree, outlines, curBookmarkIndex, cancellationToken);
        }

        public IconItem InsertIconItem(Page page, int index, bool isAdd, bool isNeedSelectNewItem)
        {
            var fileIcon = new FileIcon();
            var imageSource = fileIcon.GetImage(Icon.PDFFileInfo.filePath, 1024);
            var width = CurrentPdfFileInfo.GetPageWidth(page.Number);
            var height = CurrentPdfFileInfo.GetPageHeight(page.Number);
            var iconItem = new IconItem(page, width, height, _pdfUtilView.Cts.Token);
            iconItem.UpdateThumbnailEvent += IconItem_UpdateThumbnailEvent;
            iconItem.SetIcon(imageSource);
            iconItem.CommentItems = LoadCommentSourceFromPage(page);

            if (isAdd)
            {
                Icon.IconSources.Add(iconItem);
            }
            else
            {
                Icon.IconSources.Insert(index, iconItem);
            }

            if (isNeedSelectNewItem)
            {
                ThumbnailListView.SelectedItem = iconItem;
                ThumbnailListView.ScrollIntoView(iconItem);
            }

            return iconItem;
        }

        public void InitOutlines(OutlineCollection outlines)
        {
            foreach (var outline in outlines)
            {
                _currentPdfDocument.Outlines.Add(outline);
            }
        }

        public void InitForTheFirstDocument()
        {
            if (_currentPdfDocument.Pages.Count == 0)
            {
                return;
            }

            _isRefreshPreviewByInitDoc = true;
            NotifyTotalPageCount();
            ThumbnailListView.SelectedIndex = 0;
            PreviewPageNeedUpdate();
        }

        public IconItem GetCurSelectedIcon()
        {
            var selectedThumbnailItems = ThumbnailListView.SelectedItems;
            if (selectedThumbnailItems.Count != 0)
            {
                var lastSelectedThumbnail = (IconItem)selectedThumbnailItems[selectedThumbnailItems.Count - 1];
                var index = ThumbnailListView.Items.IndexOf(lastSelectedThumbnail);
                return lastSelectedThumbnail;
            }
            return null;
        }

        private IconItem GetPreviewIconItemFromBookmark()
        {
            var selectedBookmark = BookmarkTreeView.SelectedItem as BookmarkTreeViewItem;
            if (selectedBookmark != null)
            {
                var pageIndex = selectedBookmark.BookmarkLocationInfo.PageIndex;
                if (pageIndex != 0)
                {
                    return ThumbnailListView.Items[pageIndex - 1] as IconItem;
                }
            }
            return null;
        }

        private IconItem GetIconItemFromPage(Page page)
        {
            if (CurrentPdfDocument.Pages.Count != ThumbnailListView.Items.Count)
            {
                return null;
            }

            int index = CurrentPdfDocument.Pages.IndexOf(page);
            if (index != 0)
            {
                return ThumbnailListView.Items[index - 1] as IconItem;
            }

            return null;
        }

        private double GetPreviewPageTopInfo()
        {
            var selectedBookmark = BookmarkTreeView.SelectedItem as BookmarkTreeViewItem;
            if (selectedBookmark != null)
            {
                return selectedBookmark.BookmarkLocationInfo.Top;
            }
            return -1;
        }

        public IconItem GetClosestAndBelowItem(ref bool isMouseDownInItemArea)
        {
            int itemIndex = 0;
            foreach (var icon in ThumbnailListView.Items)
            {
                var item = ThumbnailListView.ItemContainerGenerator.ContainerFromItem(icon) as System.Windows.Controls.ListViewItem;
                if (item != null)
                {
                    var border = VisualTreeHelperUtils.FindVisualChild<DragHilightBorder>(item, o => o.Name == "listBoxItemIconViewBorder");
                    if (border != null)
                    {
                        var rect = border.TransformToVisual(ThumbnailListView).TransformBounds(new Rect(0, 0, border.Width, border.Height));
                        if (ThumbnailContextMenuDownPoint.Y >= rect.Top && ThumbnailContextMenuDownPoint.Y <= rect.Top + rect.Height)
                        {
                            isMouseDownInItemArea = true;
                            return item.DataContext as IconItem;
                        }
                        else if (ThumbnailContextMenuDownPoint.Y < rect.Top)
                        {
                            return item.DataContext as IconItem;
                        }
                    }
                }
                itemIndex++;
            }

            return null;
        }

        private void RefreshPreviewPageNumber()
        {
            PreviewPageNumber = ThumbnailListView.Items.IndexOf(_curPreviewIconItem) + 1;
        }

        public void ResetSearchingData()
        {
            CurPageFindWordIndex = -1;
            _curFindWord = string.Empty;
            _firstTimeFound = true;
            SearchCanvasImage = new DrawingImage();
            SearchSet.Clear();
        }

        private List<TextFragment> GetSearchResult(Page page, string findWord)
        {
            if (SearchSet.ContainsKey(page))
            {
                return SearchSet[page];
            }

            var absorber = new TextFragmentAbsorber("(?i)" + Regex.Escape(findWord), new TextSearchOptions(true)); // "(?i)" for ignore case
            page.Accept(absorber);

            var tempTexts = new List<TextFragment>();
            foreach (var text in absorber.TextFragments)
            {
                if (text.Position.XIndent < 0 || text.Position.YIndent < 0)
                {
                    continue;
                }
                tempTexts.Add(text);
            }
            tempTexts.Sort((x, y) => {
                if ((int)x.Rectangle.LLY == (int)y.Rectangle.LLY)
                {
                    return x.Rectangle.LLX.CompareTo(y.Rectangle.LLX);
                }
                return y.Rectangle.LLY.CompareTo(x.Rectangle.LLY);
             });

            SearchSet.Add(page, tempTexts);

            if (_firstTimeFound && absorber.TextFragments.Count > 0)
            {
                _firstTimeFound = false;
                ExecuteFindCommand(findWord, false);
            }
            return tempTexts;
        }

        private bool FindPreviewPageContainFindWord(string findWord, bool isReverse = false, int curFindPageIndex = -1)
        {
            var curPageIndex = CurrentPdfDocument.Pages.IndexOf(_curPreviewIconItem.GetPage());
            var tempIndex = curPageIndex;
            do
            {
                tempIndex += isReverse ? 1 : -1;
                if (tempIndex < 1)
                {
                    tempIndex = CurrentPdfDocument.Pages.Count;
                }
                else if (tempIndex > CurrentPdfDocument.Pages.Count)
                {
                    tempIndex = 1;
                }

                var page = CurrentPdfDocument.Pages[tempIndex];
                var textFragments = GetSearchResult(page, findWord);

                if (textFragments.Count != 0)
                {
                    SelectThumbnailByPage(page);
                    return true;
                }

            } while (tempIndex != curPageIndex);

            return false;
        }

        public void ExecuteFindCommand(string findWord, bool isPrevious, bool isSelectedPageChanged = false, Page page = null)
        {
            if (CurrentPdfDocument == null)
            {
                FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                return;
            }

            if (string.IsNullOrEmpty(findWord))
            {
                return;
            }

            var curPage = page == null ? _curPreviewIconItem.GetPage() : page;
            if (findWord.Equals(_curFindWord, StringComparison.CurrentCultureIgnoreCase))
            {
                if (isSelectedPageChanged)
                {
                    CurPageFindWordIndex = -1;
                    ScrollVerticalOffsetEvent(0);
                    DoExecuteFindCurPreviewPage(findWord, curPage);
                }
                else
                {
                    if (isPrevious && _curPageFindWordIndex - 1 < 0)
                    {
                        if (!FindPreviewPageContainFindWord(findWord))
                        {
                            RefreshFindBtnState(false, false);
                            return;
                        }
                        curPage = _curPreviewIconItem.GetPage();
                        CurPageFindWordIndex = _curPageFindWordCount;
                    }
                    if (!isPrevious && _curPageFindWordIndex >= _curPageFindWordCount - 1)
                    {
                        if (!FindPreviewPageContainFindWord(findWord, true))
                        {
                            RefreshFindBtnState(false, false);
                            return;
                        }
                        curPage = _curPreviewIconItem.GetPage();
                        CurPageFindWordIndex = -1;
                    }

                    if (isPrevious)
                    {
                        CurPageFindWordIndex--;
                    }
                    else
                    {
                        CurPageFindWordIndex++;
                    }

                    DoExecuteFindCurPreviewPage(findWord, curPage, true);
                    RefreshFindBtnState(true, true);
                }
            }
            else
            {
                _curFindWord = findWord;
                _firstTimeFound = true;
                SearchSet.Clear();
                int curPageIndex = 1;
                Task.Factory.StartNew(() =>
                {
                    bool isCancel = false;
                    int totalCount = TotalPageCount;
                    int totalTextCount = 0;
                    foreach (var currentPage in CurrentPdfDocument.Pages)
                    {
                        if (findWord != _curFindWord)
                        {
                            break;
                        }

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {   
                            var textFragments = GetSearchResult(currentPage, findWord);
                            totalTextCount += textFragments.Count;
                            if (curPageIndex == totalCount)
                            {
                                SearchingTextEvent(new SearchingTextEventArgs() { Status = ProgressStatus.Completed, IsNotFound = totalTextCount == 0 });
                                if (totalTextCount == 1)
                                {
                                    RefreshFindBtnState(false, false);
                                }
                            }
                            else
                            {
                                var args = new SearchingTextEventArgs() { Status = ProgressStatus.InProgress, CurSearchingPage = curPageIndex, TotalPageCount = totalCount };
                                SearchingTextEvent(args);
                                if (args.Status == ProgressStatus.Cancel)
                                {
                                    isCancel = true;
                                }
                            }
                        }));

                        if (isCancel)
                        {
                            break;
                        }

                        curPageIndex++;
                    }

                });

                DoExecuteFindCurPreviewPage(findWord, curPage);
            }
        }

        private void DoExecuteFindCurPreviewPage(string findWord, Page page, bool doSelect = false)
        {
            if (page == null)
            {
                return;
            }

            if (_previewPageImage == null)
            {
                SearchCanvasImage = new DrawingImage();
                return;
            }

            double widthratio = PreviewPageSize.X / GetPageSize(page, true).X;
            double heightRatio = PreviewPageSize.Y / GetPageSize(page, true).Y;
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            var textFragments = GetSearchResult(page, findWord);
            _curPageFindWordCount = textFragments.Count;

            int curIndex = 0;
            foreach (var textFragment in textFragments)
            {
                var point = ConvertPagePointToPreviewPoint(new WindowsPoint(textFragment.Rectangle.LLX, textFragment.Rectangle.URY), page);
                var rect = new Rect(point, new System.Windows.Size(textFragment.Rectangle.Width * widthratio, textFragment.Rectangle.Height * heightRatio));

                if (doSelect && curIndex == _curPageFindWordIndex)
                {
                    DrawSearchRectangle(drawingContext, _findTextSelectedColor, _findTextSelectedBorderColor, in rect);

                    var offset = textFragment.Rectangle.LLY;
                    offset = ConvertPagePointToPreviewPoint(new WindowsPoint(0, offset), page).Y - PdfHelper.ScrollLeaveDistance;
                    if (offset != -1)
                    {
                        ScrollVerticalOffsetEvent(offset);
                    }
                }
                else
                {
                    DrawSearchRectangle(drawingContext, _findBackgoundColor, _findBackgoundBorderColor, in rect);
                }
                curIndex++;
            }

            drawingContext.Close();
            if (drawingVisual.Drawing != null)
            {
                var drawingGroup = new DrawingGroup();
                drawingGroup.Children.Add(drawingVisual.Drawing);
                drawingGroup.Children.Add(new GeometryDrawing(System.Windows.Media.Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, _previewPageImage.Width, _previewPageImage.Height))));
                SearchCanvasImage = new DrawingImage(drawingGroup);
            }
            else
            {
                SearchCanvasImage = new DrawingImage();
            }
        }

        private void DrawSearchRectangle(DrawingContext drawingContext, System.Windows.Media.Brush brush, System.Windows.Media.Brush borderBrush, in Rect rect)
        {
            drawingContext.DrawRectangle(brush, null, rect);
            var pen = new System.Windows.Media.Pen(borderBrush, 2);
            drawingContext.DrawLine(pen, rect.TopLeft, rect.TopRight);
            drawingContext.DrawLine(pen, rect.TopLeft, rect.BottomLeft);
            drawingContext.DrawLine(pen, rect.TopRight, rect.BottomRight);
            drawingContext.DrawLine(pen, rect.BottomLeft, rect.BottomRight);
        }

        private void RefreshFindBtnState(bool? isPreviousBtnEnable, bool? isNextBtnEnable)
        {
            if (PdfUtilView != null && PdfUtilView.findReplaceControl != null && PdfUtilView.findReplaceControl.Visibility == Visibility.Visible)
            {
                PdfUtilView.findReplaceControl.RefreshFindBtnState(isPreviousBtnEnable, isNextBtnEnable);
            }
        }

        protected override bool HandleException(Exception ex)
        {
            FileOperation.HandleFileException(ex, PdfUtilView.WindowHandle);
            return false;
        }

        public List<XImage> GetExtractImage(Page page)
        {
            // Images in header or footer should not be extracted.
            var imageList = new List<XImage>();
            var imageExcludeList = new List<XImage>();

            foreach (var artifact in page.Artifacts)
            {
                if ((artifact.Subtype == Artifact.ArtifactSubtype.Header || artifact.Subtype == Artifact.ArtifactSubtype.Footer) && artifact.Image != null)
                {
                    imageExcludeList.Add(artifact.Image);
                }
            }

            foreach (var image in page.Resources.Images)
            {
                if (!imageExcludeList.Exists(x => x.IsTheSameObject(image)))
                {
                    imageList.Add(image);
                }
            }

            return imageList;
        }

        public void ExecuteExtractImagesCommand(bool isFromPreview = false)
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                FakeRibbonTabViewModel.ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (!isFromPreview && ThumbnailListView.SelectedItems.Count == 0)
                        {
                            FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.SELECT_PAGE_TO_EXTRACT_IMAGE_WARNING);
                            return;
                        }

                        if (LockPDFViewModel.IsSetPermissionPassword && LockPDFViewModel.CurAllowChanges == AllowChanges.AnyExceptExtracting)
                        {
                            FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                            return;
                        }

                        var imageList = new List<XImage>();
                        if (!isFromPreview)
                        {
                            foreach (var item in ThumbnailListView.SelectedItems)
                            {
                                var icon = item as IconItem;
                                if (icon != null)
                                {
                                    imageList.AddRange(GetExtractImage(icon.GetPage()));
                                }
                            }
                        }
                        else
                        {
                            if (CurPreviewIconItem == null || CurPreviewIconItem.GetPage() == null)
                            {
                                return;
                            }

                            imageList = GetExtractImage(CurPreviewIconItem.GetPage());
                        }

                        if (imageList.Count == 0)
                        {
                            FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.NO_IMAGES_EXTRACT_WARNING);
                        }
                        else
                        {
                            var extractImageView = new ExtractImageView(_pdfUtilView);
                            if (extractImageView.ShowWindow())
                            {
                                _curExtractStatus = ProgressStatus.None;
                                if (extractImageView.CurDestOptions == ImageDestinationOptions.IndividualImageFiles)
                                {
                                    string title = Properties.Resources.SELECTFOLDER_PICKER_TITLE;
                                    string defaultBtn = Properties.Resources.SELECTFOLDER_PICKER_BUTTON;

                                    var defaultFolder = PdfHelper.GetSavePickerDefaultFolder();
                                    WzCloudItem4 selectedItem = PdfHelper.InitWzCloudItem();
                                    selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

                                    bool res = WinzipMethods.DestinationFolderSelection(PdfUtilView.WindowHandle, title, defaultBtn, defaultFolder, ref selectedItem, false, true, true, true);

                                    if (res)
                                    {
                                        PdfHelper.SetSavePickerDefaultFolder(selectedItem);
                                        var cloudFolder = selectedItem;

                                        if (PdfHelper.IsCloudItem(selectedItem.profile.Id))
                                        {
                                            string destFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                                            selectedItem = PdfHelper.InitCloudItemFromPath(destFolder);
                                        }

                                        string suffix = GetImageSuffix(extractImageView.CurImageFormat);
                                        if (suffix == null)
                                        {
                                            return;
                                        }

                                        int imageIndex = 1;
                                        var view = new ProgressView(PdfUtilView, ProgressOperation.ExtractImage);
                                        bool isCancel = false;

                                        var ProcessThread = new Thread(new ThreadStart(new Action(delegate
                                        {
                                            foreach (var image in imageList)
                                            {
                                                var destStream = GetImageSaveFileStream(selectedItem.itemId, Icon.FileName, ref imageIndex, suffix);
                                                if (destStream != null)
                                                {
                                                    if (SaveImageWithTransform(image, destStream, extractImageView.CurImageFormat))
                                                    {
                                                        EDPHelper.SyncEnterpriseId(CurrentPdfFileName, destStream.Name, 500);
                                                        destStream.Close();
                                                    }
                                                    else
                                                    {
                                                        destStream.Close();
                                                        if (File.Exists(destStream.Name))
                                                        {
                                                            File.Delete(destStream.Name);
                                                        }
                                                        imageIndex--;
                                                    }

                                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                    {
                                                        var args = new ProgressEventArgs() { CurExtractItemIndex = imageList.IndexOf(image), TotalItemsCount = imageList.Count, FileName = Path.GetFileName(Icon.FileName) };
                                                        view.InvokeProgressEvent(args);
                                                        if (_curExtractStatus == ProgressStatus.Cancel)
                                                        {
                                                            isCancel = true;
                                                        }
                                                    }));
                                                }
                                                else
                                                {
                                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                    {
                                                        FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                                                        isCancel = true;
                                                    }));
                                                }

                                                if (isCancel)
                                                {
                                                    break;
                                                }
                                            }

                                            if (_curExtractStatus != ProgressStatus.Cancel)
                                            {
                                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                {
                                                    view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                                                }));
                                            }

                                            if (!isCancel && PdfHelper.IsCloudItem(cloudFolder.profile.Id))
                                            {
                                                var allFiles = Directory.GetFiles(selectedItem.itemId, "*.*", SearchOption.AllDirectories);
                                                var filesList = new List<WzCloudItem4>();
                                                foreach (var filepath in allFiles)
                                                {
                                                    var item = PdfHelper.InitCloudItemFromPath(filepath);
                                                    filesList.Add(item);
                                                }

                                                int count = filesList.Count;
                                                isCancel = !WinzipMethods.UploadToCloud(PdfUtilView.WindowHandle, filesList.ToArray(), ref count, cloudFolder, false, false);
                                            }

                                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                            {
                                                if (!isCancel)
                                                {
                                                    TrackHelper.LogPdfExtractEvent(false, suffix);
                                                    FlatMessageWindows.DisplayInformationMessage(PdfUtilView.WindowHandle, Properties.Resources.IMAGE_EXTRACTED_SUCCESSFULLY);
                                                }
                                            }));
                                        })));

                                        ProcessThread.Start();
                                        view.ShowWindow();
                                    }
                                }
                                else
                                {
                                    if (_pdfUtilView.CheckCurrentArchiveIsReadOnly())
                                    {
                                        return;
                                    }

                                    string suffix = GetImageSuffix(extractImageView.CurImageFormat);
                                    if (suffix == null)
                                    {
                                        return;
                                    }

                                    int imageIndex = 1;
                                    var view = new ProgressView(PdfUtilView, ProgressOperation.ExtractImage);
                                    bool isCancel = false;

                                    var ProcessThread = new Thread(new ThreadStart(new Action(delegate
                                    {
                                        string destFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                                        foreach (var image in imageList)
                                        {
                                            var destStream = GetImageSaveFileStream(destFolder, Icon.FileName, ref imageIndex, suffix);
                                            if (destStream != null)
                                            {
                                                if (SaveImageWithTransform(image, destStream, extractImageView.CurImageFormat))
                                                {
                                                    EDPHelper.SyncEnterpriseId(CurrentPdfFileName, destStream.Name, 500);
                                                    destStream.Close();
                                                }
                                                else
                                                {
                                                    destStream.Close();
                                                    if (File.Exists(destStream.Name))
                                                    {
                                                        File.Delete(destStream.Name);
                                                    }
                                                    imageIndex--;
                                                }

                                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                {
                                                    var args = new ProgressEventArgs() { CurExtractItemIndex = imageList.IndexOf(image), TotalItemsCount = imageList.Count, FileName = Path.GetFileName(Icon.FileName) };
                                                    view.InvokeProgressEvent(args);
                                                    if (_curExtractStatus == ProgressStatus.Cancel)
                                                    {
                                                        isCancel = true;
                                                    }
                                                }));
                                            }
                                            else
                                            {
                                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                {
                                                    FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                                                    isCancel = true;
                                                }));
                                            }

                                            if (isCancel)
                                            {
                                                break;
                                            }
                                        }

                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                        {
                                            if (_curExtractStatus != ProgressStatus.Cancel)
                                            {
                                                view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                                            }

                                            if (!isCancel)
                                            {
                                                destFolder += "*";
                                                if (PdfUtilView.ExecuteSaveToZip(destFolder, false, false))
                                                {
                                                    TrackHelper.LogPdfExtractEvent(false, suffix);
                                                    FlatMessageWindows.DisplayInformationMessage(PdfUtilView.WindowHandle, Properties.Resources.IMAGE_EXTRACTED_SUCCESSFULLY);
                                                }
                                            }
                                        }));
                                    })));

                                    ProcessThread.Start();
                                    view.ShowWindow();
                                }
                            }
                        }
                    }
                    tcs.TrySetResult();
                });
            });
        }

        public string GetImageSuffix(ImageFormatEnum format)
        {
            switch (format)
            {
                case ImageFormatEnum.BMP:
                    return ".bmp";
                case ImageFormatEnum.GIF:
                    return ".gif";
                case ImageFormatEnum.JPG:
                    return ".jpg";
                case ImageFormatEnum.JP2:
                    return ".jp2";
                case ImageFormatEnum.PNG:
                    return ".png";
                case ImageFormatEnum.PSD:
                    return ".psd";
                case ImageFormatEnum.TIF:
                    return ".tif";
                case ImageFormatEnum.WEBP:
                    return ".webp";
                case ImageFormatEnum.SVG:
                    return ".svg";
                default:
                    return null;
            }
        }

        public FileStream GetImageSaveFileStream(string folderName, string fileName, ref int imageIndex, string suffix)
        {
            string filePath = Path.Combine(folderName, Path.GetFileNameWithoutExtension(fileName) + "-Image-" + imageIndex.ToString() + suffix);
            try
            {
                var stream = File.Open(filePath, FileMode.CreateNew);
                imageIndex++;
                return stream;

            }
            catch (IOException)
            {
                imageIndex++;
                return GetImageSaveFileStream(folderName, fileName, ref imageIndex, suffix);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        public void ExecuteExtractPagesCommand(bool isFromPreview = false)
        {
            Task.Factory.StartNewTCS(tcs =>
            {
                FakeRibbonTabViewModel.ExecuteOpenTask(false).ContinueWithTCSTaskInContext(tcs, task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (CurrentPdfDocument == null)
                        {
                            return;
                        }

                        if (!isFromPreview && ThumbnailListView.SelectedItems.Count == 0)
                        {
                            FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.SELECT_PAGE_TO_EXTRACT_WARNING);
                            return;
                        }

                        if (LockPDFViewModel.IsSetPermissionPassword && LockPDFViewModel.CurAllowChanges == AllowChanges.AnyExceptExtracting)
                        {
                            FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                            return;
                        }

                        var extractPageView = new ExtractPagesView(_pdfUtilView);
                        if (extractPageView.ShowWindow())
                        {
                            var extractPageList = new List<int>();
                            if (!isFromPreview)
                            {
                                foreach (var item in ThumbnailListView.SelectedItems)
                                {
                                    var icon = item as IconItem;
                                    if (icon != null)
                                    {
                                        extractPageList.Add(GetPageIndex(icon.GetPage()));
                                    }
                                }
                            }
                            else
                            {
                                if (CurPreviewIconItem == null || CurPreviewIconItem.GetPage() == null)
                                {
                                    return;
                                }

                                extractPageList.Add(GetPageIndex(CurPreviewIconItem.GetPage()));
                            }

                            if (extractPageList.Count == 0)
                            {
                                return;
                            }

                            extractPageList.Sort();

                            _curExtractStatus = ProgressStatus.None;
                            ConfirmFileReplaceDialog.NotNeedOverrideConfirm = false;

                            if (extractPageView.CurDestOptions == DestinationOptions.DocumentFile)
                            {
                                string title = Properties.Resources.SAVE_PICKER_TITLE;
                                string defaultBtn = Properties.Resources.SAVE_PICKER_BUTTON;
                                string filter = GetDocumentFilter(extractPageView.CurDocFormat);
                                if (string.IsNullOrEmpty(filter))
                                {
                                    return;
                                }

                                var defaultFolder = PdfHelper.GetSavePickerDefaultFolder();
                                string defaultFileName = string.Empty;
                                if (extractPageView.CurDocFormat == DocumentFormatEnum.PDF)
                                {
                                    defaultFileName = Properties.Resources.DEFAULT_PDF_TITLE_NAME;
                                }
                                else
                                {
                                    defaultFileName = Path.GetFileName(CurrentPdfFileName) + "." + extractPageView.CurDocFormat.ToString().ToLower();
                                }

                                WzCloudItem4 selectedItem = PdfHelper.InitWzCloudItem();
                                selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

                                bool ret = false;
                                string tempSelectedItemName = Path.GetFileName(CurrentPdfFileName);
                                if (extractPageView.CurDocFormat == DocumentFormatEnum.DOC || extractPageView.CurDocFormat == DocumentFormatEnum.DOCX
                                 || extractPageView.CurDocFormat == DocumentFormatEnum.PDF || extractPageList.Count == 1)
                                {
                                    ConfirmFileReplaceDialog.NotNeedOverrideConfirm = true;
                                    ret = WinzipMethods.SaveAsDialog(PdfUtilView.WindowHandle, title, defaultBtn, defaultFileName, filter, defaultFolder, ref selectedItem);
                                }
                                else
                                {
                                    ret = WinzipMethods.DestinationFolderSelection(PdfUtilView.WindowHandle, title, defaultBtn, defaultFolder, ref selectedItem, false, true, true, true);
                                    tempSelectedItemName += filter.Substring(filter.LastIndexOf('.'));
                                }

                                if (ret)
                                {
                                    PdfHelper.SetSavePickerDefaultFolder(selectedItem);

                                    string tempDestPdfPath = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), Path.GetFileNameWithoutExtension(Icon.FileName) + "-temp.pdf");
                                    string tempSourcePath = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), Path.GetFileNameWithoutExtension(Icon.FileName) + "-temp.pdf");

                                    if (!PrepareTempSourceFileForExtract(tempSourcePath))
                                    {
                                        Directory.Delete(Path.GetDirectoryName(tempSourcePath), true);
                                        return;
                                    }

                                    if (File.Exists(tempSourcePath))
                                    {
                                        var pdfEditor = new PdfFileEditor();
                                        bool disableCancel = extractPageView.CurDocFormat == DocumentFormatEnum.DOC || extractPageView.CurDocFormat == DocumentFormatEnum.DOCX || extractPageView.CurDocFormat == DocumentFormatEnum.PDF;
                                        var view = new ProgressView(PdfUtilView, ProgressOperation.ExtractPage, disableCancel);

                                        var ProcessThread = new Thread(new ThreadStart(new Action(delegate
                                        {
                                            bool extractSuccess = pdfEditor.Extract(tempSourcePath, extractPageList.ToArray(), tempDestPdfPath);
                                            if (extractSuccess)
                                            {
                                                if (PdfHelper.IsCloudItem(selectedItem.profile.Id))
                                                {
                                                    string tempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                                                    string tempItemPath = Path.Combine(tempFolder, selectedItem.isFolder ? tempSelectedItemName : selectedItem.name);
                                                    extractSuccess = SaveExtractedDocument(view, extractPageView.CurDocFormat, tempDestPdfPath, tempItemPath, extractPageView.CurDestOptions, selectedItem);
                                                }
                                                else
                                                {
                                                    string itemPath = selectedItem.isFolder ? Path.Combine(selectedItem.itemId, tempSelectedItemName) : Path.Combine(selectedItem.parentId, selectedItem.name);
                                                    extractSuccess = SaveExtractedDocument(view, extractPageView.CurDocFormat, tempDestPdfPath, itemPath, extractPageView.CurDestOptions, selectedItem);
                                                }
                                            }

                                            if (!extractSuccess)
                                            {
                                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                                {
                                                    view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                                                }));
                                            }
                                            else
                                            {
                                                TrackHelper.LogPdfExtractEvent(true, defaultFileName);
                                            }

                                            Directory.Delete(Path.GetDirectoryName(tempDestPdfPath), true);
                                            if (CurrentPdfSourceLockStatus.isSetOpenPassword)
                                            {
                                                Directory.Delete(Path.GetDirectoryName(tempSourcePath), true);
                                            }
                                        })));

                                        ProcessThread.Start();
                                        view.ShowWindow();
                                    }
                                }
                            }
                            else
                            {
                                if (!FlatMessageWindows.DisplayWarningConfirmationMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_OVERWRITE_ZIP_FILE, false))
                                {
                                    return;
                                }

                                string destFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                                string tempSourcePath = Path.Combine(destFolder, Path.GetFileNameWithoutExtension(Icon.FileName) + "-temp.pdf");
                                string tempDestPath = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), Path.GetFileNameWithoutExtension(Icon.FileName) + "-temp.pdf");

                                if (!PrepareTempSourceFileForExtract(tempSourcePath))
                                {
                                    Directory.Delete(Path.GetDirectoryName(tempSourcePath), true);
                                    return;
                                }

                                if (File.Exists(tempSourcePath))
                                {
                                    string defaultFileName;
                                    if (extractPageView.CurDocFormat == DocumentFormatEnum.PDF)
                                    {
                                        defaultFileName = Path.GetFileNameWithoutExtension(CurrentPdfFileName) + "." + extractPageView.CurDocFormat.ToString().ToLower();
                                    }
                                    else
                                    {
                                        defaultFileName = Path.GetFileName(CurrentPdfFileName) + "." + extractPageView.CurDocFormat.ToString().ToLower();
                                    }

                                    WzCloudItem4 selectedItem = PdfHelper.InitWzCloudItem();
                                    selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

                                    string itemPath = Path.Combine(destFolder, defaultFileName);
                                    var pdfEditor = new PdfFileEditor();
                                    bool disableCancel = extractPageView.CurDocFormat == DocumentFormatEnum.DOC || extractPageView.CurDocFormat == DocumentFormatEnum.DOCX || extractPageView.CurDocFormat == DocumentFormatEnum.PDF;
                                    var view = new ProgressView(PdfUtilView, ProgressOperation.ExtractPage, disableCancel);

                                    var ProcessThread = new Thread(new ThreadStart(new Action(delegate
                                    {
                                        bool extractSuccess = pdfEditor.Extract(tempSourcePath, extractPageList.ToArray(), tempDestPath);
                                        if (extractSuccess)
                                        {
                                            extractSuccess = SaveExtractedDocument(view, extractPageView.CurDocFormat, tempDestPath, itemPath, extractPageView.CurDestOptions, selectedItem);
                                        }

                                        if (!extractSuccess)
                                        {
                                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                            {
                                                view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                                            }));
                                        }
                                        else
                                        {
                                            TrackHelper.LogPdfExtractEvent(true, defaultFileName);
                                        }

                                        File.Delete(tempSourcePath);
                                    })));

                                    ProcessThread.Start();
                                    view.ShowWindow();
                                }
                            }
                        }
                    }
                    tcs.TrySetResult();
                });
            });

        }

        public bool PrepareTempSourceFileForExtract(string tempSourcePath)
        {
            if (CurrentPdfSourceLockStatus.isSetOpenPassword)
            {
                // PdfFileEditor cannot extract files with open password, so we decrypt and save it to a temp file and do extract
                var password = string.IsNullOrEmpty(CurrentPdfSourceLockStatus.openPassword) ?
                    CurrentPdfSourceLockStatus.permissionPassword : CurrentPdfSourceLockStatus.openPassword;
                var tempDoc = new Document(CurrentPdfFileName, password);
                if (tempDoc == null)
                {
                    return false;
                }

                tempDoc.Decrypt();
                tempDoc.Save(tempSourcePath);
            }
            else
            {
                using (var stream = File.Create(tempSourcePath))
                {
                    lock (IconSouceImage.LockLoadPdfThumbnail)
                    {
                        CurrentPdfDocument.Save(stream);
                    }
                }
            }

            return true;
        }

        public string GetExtractTempFilePath()
        {
            if (string.IsNullOrEmpty(_extractTempFolder))
            {
                _extractTempFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
            }

            return Path.Combine(_extractTempFolder, Properties.Resources.DEFAULT_PDF_TITLE_NAME);
        }

        public bool ExtractTempFileByDragOperation(List<int> extractPageList, ref string resultPath)
        {
            string tempDestPath = Path.Combine(_extractTempFolder, Properties.Resources.DEFAULT_PDF_TITLE_NAME);
            string tempSourcePath = Path.Combine(_extractTempFolder, Icon.FileName);

            if (!PrepareTempSourceFileForExtract(tempSourcePath))
            {
                Directory.Delete(Path.GetDirectoryName(tempSourcePath), true);
                return false;
            }

            var pdfEditor = new PdfFileEditor();
            bool extractSuccess = pdfEditor.Extract(tempSourcePath, extractPageList.ToArray(), tempDestPath);
            if (extractSuccess)
            {
                EDPHelper.SyncEnterpriseId(CurrentPdfFileName, tempDestPath, 500);
                resultPath = tempDestPath;
            }

            return extractSuccess;
        }

        public void CleanTempFolderAfterDragOperation(bool doDelete)
        {
            if (!string.IsNullOrEmpty(_extractTempFolder))
            {
                if (Directory.Exists(_extractTempFolder) && doDelete)
                {
                    try
                    {
                        Directory.Delete(_extractTempFolder, true);
                    }
                    catch
                    {
                        // catch the exception when delete directory
                    }
                }

                _extractTempFolder = string.Empty;
            }
        }

        private bool SaveExtractedDocument(ProgressView view, DocumentFormatEnum documentFormat, string srcFile, string destFileName, DestinationOptions destOption, WzCloudItem4 selectedItem)
        {
            var document = new Document(srcFile);
            if (document == null)
            {
                return false;
            }

            switch (documentFormat)
            {
                case DocumentFormatEnum.PDF:
                    DoExtractPageToDoc(view, document, destFileName, SaveFormat.None, destOption, selectedItem);
                    break;
                case DocumentFormatEnum.DOC:
                    DoExtractPageToDoc(view, document, destFileName, SaveFormat.Doc, destOption, selectedItem);
                    break;
                case DocumentFormatEnum.DOCX:
                    DoExtractPageToDoc(view, document, destFileName, SaveFormat.DocX, destOption, selectedItem);
                    break;
                case DocumentFormatEnum.BMP:
                case DocumentFormatEnum.JPG:
                case DocumentFormatEnum.PNG:
                case DocumentFormatEnum.TIF:
                    DoExtractPageToImage(view, document, documentFormat, destFileName, destOption, selectedItem);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private void DoExtractPageToDoc(ProgressView view, Document document, string destFileName, SaveFormat format, DestinationOptions destOption, WzCloudItem4 selectedItem)
        {
            var ProcessThread = new Thread(new ThreadStart(new Action(delegate
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    view.InvokeProgressEvent(new ProgressEventArgs() { TotalItemsCount = 1, FileName = Path.GetFileName(CurrentPdfFileName) });
                }));

                bool catchError = false;
                try
                {
                    document.Save(destFileName, format);
                }
                catch (UnauthorizedAccessException)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                    }));
                    catchError = true;
                    _curExtractStatus = ProgressStatus.Cancel;
                }
                catch
                {
                    catchError = true;
                    _curExtractStatus = ProgressStatus.Cancel;
                }

                if (catchError || _curExtractStatus != ProgressStatus.Cancel)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                    }));
                }

                ExtractPagePostProcess(document, destOption, destFileName, selectedItem);
            })));

            ProcessThread.Start();
        }

        private void DoExtractPageToImage(ProgressView view, Document document, DocumentFormatEnum documentFormat, string destFileName, DestinationOptions destOption, WzCloudItem4 selectedItem)
        {
            var ProcessThread = new Thread(new ThreadStart(new Action(delegate
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    var args = new ProgressEventArgs() { CurExtractItemIndex = 0, TotalItemsCount = document.Pages.Count, FileName = Path.GetFileName(CurrentPdfFileName) };
                    view.InvokeProgressEvent(args);
                }));

                string extension = Path.GetExtension(destFileName);
                int curIndex = 1;
                bool isCancel = false;
                bool catchError = false;
                try
                {
                    Func<OverrideConfirmResult.Choice, string, string> handleConfirmResult = (choice, tempFileName)  =>
                    {
                        switch (choice)
                        {
                            case OverrideConfirmResult.Choice.Override:
                                break;
                            case OverrideConfirmResult.Choice.Rename:
                                tempFileName = OverrideConfirmResult.RenameFileName(tempFileName, selectedItem.profile.Id);
                                break;
                            default:
                                break;
                        }

                        return tempFileName;
                    };

                    bool isMultiple = document.Pages.Count > 1 ? true : false;
                    var confirmChoice = OverrideConfirmResult.Choice.None;
                    bool applyToAll = false;
                    foreach (var page in document.Pages)
                    {
                        if (isCancel)
                        {
                            break;
                        }

                        string tempFileName = destFileName;
                        if (document.Pages.Count > 1)
                        {
                            // If the document has more than one page, add the page number to the image name
                            tempFileName = tempFileName.Insert(destFileName.IndexOf(extension), "-" + curIndex.ToString());
                        }

                        if (File.Exists(tempFileName))
                        {
                            if (applyToAll)
                            {
                                if (confirmChoice == OverrideConfirmResult.Choice.Skip)
                                {
                                    continue;
                                }

                                tempFileName = handleConfirmResult(confirmChoice, tempFileName);
                            }
                            else
                            {
                                var confirmRes = ConfirmFileReplaceDialog.Show(PdfUtilView.WindowHandle, Path.GetFileName(destFileName), tempFileName, isMultiple);
                                confirmChoice = confirmRes.choice;
                                applyToAll = confirmRes.applyToAll;

                                if (confirmChoice == OverrideConfirmResult.Choice.Cancel)
                                {
                                    catchError = true;
                                    _curExtractStatus = ProgressStatus.Cancel;
                                    break;
                                }

                                if (confirmChoice == OverrideConfirmResult.Choice.Skip)
                                {
                                    continue;
                                }

                                tempFileName = handleConfirmResult(confirmChoice, tempFileName);
                            }
                        }

                        using (var stream = new FileStream(tempFileName, FileMode.Create))
                        {
                            int pageWidth = (int)page.PageInfo.Width;
                            int pageHeight = (int)page.PageInfo.Height;
                            Device imageDevice;
                            switch (documentFormat)
                            {
                                case DocumentFormatEnum.BMP:
                                    imageDevice = new BmpDevice(pageWidth, pageHeight);
                                    break;
                                case DocumentFormatEnum.PNG:
                                    imageDevice = new PngDevice(pageWidth, pageHeight);
                                    break;
                                case DocumentFormatEnum.TIF:
                                    imageDevice = new TiffDevice(pageWidth, pageHeight);
                                    break;
                                case DocumentFormatEnum.JPG:
                                default:
                                    imageDevice = new JpegDevice(pageWidth, pageHeight);
                                    break;
                            }

                            if (documentFormat == DocumentFormatEnum.TIF)
                            {
                                (imageDevice as TiffDevice).Process(document, curIndex, curIndex, stream);
                            }
                            else
                            {
                                (imageDevice as ImageDevice).Process(page, stream);
                            }
                            stream.Close();

                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                view.InvokeProgressEvent(new ProgressEventArgs() { CurExtractItemIndex = curIndex++, TotalItemsCount = document.Pages.Count });

                                if (_curExtractStatus == ProgressStatus.Cancel)
                                {
                                    isCancel = true;
                                }
                            }));
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.WARNING_SAVE_LOCATION_NOT_SUPPORTED);
                    }));
                    catchError = true;
                    _curExtractStatus = ProgressStatus.Cancel;
                }
                catch
                {
                    catchError = true;
                    _curExtractStatus = ProgressStatus.Cancel;
                }

                if (catchError || _curExtractStatus != ProgressStatus.Cancel)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        view.InvokeProgressEvent(new ProgressEventArgs() { Status = ProgressStatus.Completed });
                    }));
                }

                ExtractPagePostProcess(document, destOption, destFileName, selectedItem);
            })));

            ProcessThread.Start();
        }

        private void ExtractPagePostProcess(Document doc, DestinationOptions destOption, string destFileName, WzCloudItem4 selectedItem)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (destOption != DestinationOptions.AddToCurrentZipFile && PdfHelper.IsCloudItem(selectedItem.profile.Id))
                {
                    string destFolder = Path.GetDirectoryName(destFileName);
                    var dir = new DirectoryInfo(destFolder);
                    var fileList = new List<string>();
                    RecursiveDir(dir, ref fileList);

                    var uploadFilesList = new List<WzCloudItem4>();
                    foreach (var filepath in fileList)
                    {
                        EDPHelper.SyncEnterpriseId(CurrentPdfFileName, filepath);
                        var item = PdfHelper.InitCloudItemFromPath(filepath);
                        uploadFilesList.Add(item);
                    }

                    int count = uploadFilesList.Count;
                    bool res = WinzipMethods.UploadToCloud(PdfUtilView.WindowHandle, uploadFilesList.ToArray(), ref count, selectedItem, false, false);

                    if (!res)
                    {
                        _curExtractStatus = ProgressStatus.Cancel;
                    }

                    if (Directory.Exists(destFolder))
                    {
                        Directory.Delete(destFolder, true);
                    }
                }

                if (_curExtractStatus != ProgressStatus.Cancel)
                {
                    EDPHelper.SyncEnterpriseId(CurrentPdfFileName, destFileName, 500);
                    if (destOption != DestinationOptions.AddToCurrentZipFile)
                    {
                        FlatMessageWindows.DisplayInformationMessage(PdfUtilView.WindowHandle, Properties.Resources.PAGES_EXTRACTED_SUCCESSFULLY);
                    }
                }

                doc.Dispose();

                if (destOption == DestinationOptions.AddToCurrentZipFile)
                {
                    string destFolder = Path.GetDirectoryName(destFileName);
                    destFolder += "*";
                    if (PdfUtilView.ExecuteSaveToZip(destFolder, false, false))
                    {
                        FlatMessageWindows.DisplayInformationMessage(PdfUtilView.WindowHandle, Properties.Resources.PAGES_EXTRACTED_SUCCESSFULLY);
                    }
                }
            }));
        }

        private void PdfUtilViewModel_ProgressCancelEvent()
        {
            _curExtractStatus = ProgressStatus.Cancel;
        }

        public void SendProgressCancelEvent()
        {
            ProgressCancelEvent();
        }

        private string GetDocumentFilter(DocumentFormatEnum documentFormat)
        {
            switch (documentFormat)
            {
                case DocumentFormatEnum.PDF:
                    return "Documents|*.pdf";
                case DocumentFormatEnum.DOC:
                    return "Documents|*.doc";
                case DocumentFormatEnum.DOCX:
                    return "Documents|*.docx";
                case DocumentFormatEnum.BMP:
                    return "Documents|*.bmp";
                case DocumentFormatEnum.JPG:
                    return "Documents|*.jpg";
                case DocumentFormatEnum.PNG:
                    return "Documents|*.png";
                case DocumentFormatEnum.TIF:
                    return "Documents|*.tif";
                default:
                    return null;
            }
        }

        public bool SaveImageWithTransform(XImage sourceImage, FileStream destStream, ImageFormatEnum destFormat, bool retry = false)
        {
            if (sourceImage == null)
            {
                return false;
            }

            string tempName = string.Empty;
            try
            {
                // Aspose.Image cannot load when saving to Stream, however it works when saving to file, strange.... :-)
                tempName = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), $"{DateTime.Now.Ticks}.png");
                using (var tempStream = File.Open(tempName, FileMode.Create))
                {
                    sourceImage.Save(tempStream, System.Drawing.Imaging.ImageFormat.Png);
                }

                using (var image = Aspose.Imaging.Image.Load(tempName))
                {
                    Aspose.Imaging.ImageOptionsBase option = null;
                    switch (destFormat)
                    {
                        case ImageFormatEnum.BMP:
                            option = new Aspose.Imaging.ImageOptions.BmpOptions();
                            break;
                        case ImageFormatEnum.GIF:
                            option = new Aspose.Imaging.ImageOptions.GifOptions();
                            break;
                        case ImageFormatEnum.JPG:
                            option = new Aspose.Imaging.ImageOptions.JpegOptions();
                            break;
                        case ImageFormatEnum.JP2:
                            option = new Aspose.Imaging.ImageOptions.Jpeg2000Options();
                            break;
                        case ImageFormatEnum.PNG:
                            option = new Aspose.Imaging.ImageOptions.PngOptions();
                            break;
                        case ImageFormatEnum.PSD:
                            option = new Aspose.Imaging.ImageOptions.PsdOptions();
                            break;
                        case ImageFormatEnum.TIF:
                            option = new Aspose.Imaging.ImageOptions.TiffOptions(Aspose.Imaging.FileFormats.Tiff.Enums.TiffExpectedFormat.TiffJpegRgb);
                            break;
                        case ImageFormatEnum.WEBP:
                            option = new Aspose.Imaging.ImageOptions.WebPOptions();
                            break;
                        case ImageFormatEnum.SVG:
                            option = new Aspose.Imaging.ImageOptions.SvgOptions
                            {
                                VectorRasterizationOptions = new Aspose.Imaging.ImageOptions.SvgRasterizationOptions()
                            };
                            option.VectorRasterizationOptions.PageWidth = image.Width;
                            option.VectorRasterizationOptions.PageHeight = image.Height;
                            break;
                        default:
                            return false;
                    }
                    image.Save(destStream, option);
                }

                return true;
            }
            catch
            {
                // sometime Aspose.Image throw exceptions like image type not support or index out of range, try again, if it still fails, skip this image
                if (!retry)
                {
                    return SaveImageWithTransform(sourceImage, destStream, destFormat, true);
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (Directory.Exists(Path.GetDirectoryName(tempName)))
                {
                    Directory.Delete(Path.GetDirectoryName(tempName), true);
                }
            }
        }

        public bool SelectThumbnailByPage(Page page)
        {
            var icon = GetIconItemFromPage(page);
            if (icon != null && ThumbnailListView.SelectedItem != icon)
            {
                ThumbnailListView.SelectedItem = icon;
                ThumbnailListView.ScrollIntoView(icon);
                return true;
            }

            return false;
        }

        public Task ExecuteImportFilesTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                if (!string.IsNullOrEmpty(CurrentPdfFileName) && FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
                {
                    tcs.TrySetResult();
                    return;
                }

                if (LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission))
                {
                    FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                    tcs.TrySetResult();
                    return;
                }

                PdfUtilView.SetWindowLoadingStatus(true);
                var resultFilePath = FakeRibbonTabViewModel.ImportFilesByConvert(true);
                if (!string.IsNullOrEmpty(resultFilePath) && Path.GetExtension(resultFilePath.ToLower()) == PdfHelper.PdfExtension)
                {
                    var ret = CurrentPdfDocument == null ? OpenNewPdfAfterConverted(resultFilePath) : InsertToExistPdfAfterConverted(resultFilePath);
                }

                PdfUtilView.SetWindowLoadingStatus(false);
                PdfUtilView.Focus();
                tcs.TrySetResult();
            });
        }

        public Task ExecuteImportFromCameraTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                if (!string.IsNullOrEmpty(CurrentPdfFileName) && FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
                {
                    tcs.TrySetResult();
                    return;
                }

                if (LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission))
                {
                    FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                    tcs.TrySetResult();
                    return;
                }

                string destFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                bool ret = WinzipMethods.ImportFromCamera(PdfUtilView.WindowHandle, destFolder);

                if (ret)
                {
                    var dir = new DirectoryInfo(destFolder);
                    var fileList = new List<string>();
                    RecursiveDir(dir, ref fileList);
                    if (OpenImportSelectedFiles(fileList, false) == Result.Ok)
                    {
                        NotifyDocChanged();

                        foreach (var file in fileList)
                        {
                            TrackHelper.LogInsertFromCameraEvent(file);
                        }
                    }
                }

                tcs.TrySetResult();
            });
        }

        public Task<bool> ExecuteImportFromScannerTask(bool fromContextMenu = false, string folderPath = null)
        {
            return Task<bool>.Factory.StartNewTCS(tcs =>
            {
                if (!string.IsNullOrEmpty(CurrentPdfFileName) && FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
                {
                    tcs.TrySetResult(false);
                    return;
                }

                if (LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission))
                {
                    FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                    tcs.TrySetResult(false);
                    return;
                }

                string destFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
                bool ret = WinzipMethods.ImportFromScanner(PdfUtilView.WindowHandle, destFolder);

                if (ret)
                {
                    var dir = new DirectoryInfo(destFolder);
                    var fileList = new List<string>();
                    RecursiveDir(dir, ref fileList);

                    if (fromContextMenu)
                    {
                        var filePath = fileList[0];
                        var destPath = Path.Combine(folderPath, Path.GetFileName(filePath));
                        bool moveFail = false;
                        try
                        {
                            File.Move(filePath, destPath);
                        }
                        catch (Exception)
                        {
                            moveFail = true;
                        }

                        var dialogResult = System.Windows.Forms.DialogResult.Yes;

                        if (!moveFail)
                        {
                            fileList.Clear();
                            fileList.Add(destPath);

                            TASKDIALOG_BUTTON[] buttons = new[]
                            {
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.Yes,
                                    text = Properties.Resources.SCAN_OPEN_IN_EXPRESS_TITLE + "\n" + Properties.Resources.SCAN_OPEN_IN_EXPRESS_DESC
                                },
                                new TASKDIALOG_BUTTON()
                                {
                                    id = (int)System.Windows.Forms.DialogResult.No,
                                    text = Properties.Resources.SCAN_CLOSE_TITLE + "\n" + Properties.Resources.SCAN_CLOSE_DESC
                                }
                            };

                            int taskDialogWidth = 180;
                            var taskDialog = new TaskDialog(Properties.Resources.PDF_UTILITY_TITLE, string.Format(Properties.Resources.SCAN_SAVE_TO_PATH_MAIN, destPath),
                                Properties.Resources.SCAN_SAVE_TO_PATH_CONTENT, null, buttons, Properties.Resources.PdfUtil, taskDialogWidth);
                            dialogResult = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle).dialogResult;
                        }

                        if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            PdfUtilView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                if (OpenImportSelectedFiles(fileList, !moveFail) == Result.Ok)
                                {
                                    if (moveFail)
                                    {
                                        NotifyDocChanged();
                                    }
                                    else
                                    {
                                        ResetDocChangedState();
                                    }

                                    foreach (var file in fileList)
                                    {
                                        TrackHelper.LogInsertFromScannerEvent(file);
                                    }
                                }
                            }));

                            tcs.TrySetResult(true);
                            return;
                        }
                        else
                        {
                            PdfUtilView.Close();
                        }
                    }
                    else
                    {
                        if (OpenImportSelectedFiles(fileList, false) == Result.Ok)
                        {
                            NotifyDocChanged();

                            foreach (var file in fileList)
                            {
                                TrackHelper.LogInsertFromScannerEvent(file);
                            }
                        }
                    }
                }

                tcs.TrySetResult(false);
            });
        }

        public void ExecuteDragFromExploreTaskCommand(string[] files, bool dragToThumbnailPane)
        {
            Executor(() => ExecuteDragFromExploreTask(files, dragToThumbnailPane), RetryStrategy.Create(false, 0)).IgnoreExceptions();
        }

        public Task ExecuteDragFromExploreTask(string[] files, bool dragToThumbnailPane)
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                if (dragToThumbnailPane && !string.IsNullOrEmpty(CurrentPdfFileName) && FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
                {
                    tcs.TrySetResult();
                    return;
                }

                if (dragToThumbnailPane && LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission
                    || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission))
                {
                    FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                    tcs.TrySetResult();
                    return;
                }

                if (!dragToThumbnailPane && CurrentPdfDocument != null)
                {
                    if (files.Length == 1 && Path.GetExtension(files[0]).ToLower() == PdfHelper.PdfExtension)
                    {
                        bool isCancel = false;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (FakeRibbonTabViewModel.DoSaveChangesCheck() == SaveChangesCheckEnum.Cancel)
                            {
                                isCancel = true;
                            }
                        }));

                        if (isCancel)
                        {
                            tcs.TrySetCanceled();
                            return;
                        }
                    }
                    else
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                }

                PdfUtilView.SetWindowLoadingStatus(true);

                bool continueOperation = true;
                var supportFileList = PdfHelper.FilterFilesSupportConvertToPdf(files);

                if (supportFileList.Length < files.Length)
                {
                    // some selected file(s) do not support convert to pdf, show user the unsupported file(s)
                    var unSupportFileList = files.Where(file => !supportFileList.Contains(file)).ToArray();
                    var dialog = new FileNotSupportWarningDialog(unSupportFileList);
                    continueOperation = dialog.ShowWindow() && supportFileList.Length > 0;
                }

                if (continueOperation)
                {
                    var selectedItemsList = new List<WzCloudItem4>();
                    foreach (var file in supportFileList)
                    {
                        var fileItem = PdfHelper.InitCloudItemFromPath(file);
                        selectedItemsList.Add(fileItem);
                    }

                    FileOperation.FilterUnreadableFiles(selectedItemsList, PdfUtilView.WindowHandle);

                    if (selectedItemsList.Count > 0)
                    {
                        if (selectedItemsList.Count == 1 && Path.GetExtension(selectedItemsList[0].path).ToLower() == PdfHelper.PdfExtension
                            && (CurrentPdfDocument == null || !dragToThumbnailPane))
                        {
                            // open selected pdf file
                            var file = selectedItemsList[0];

                            if (FakeRibbonTabViewModel.InitDocumentByOpen(file, false, false) == Result.Ok)
                            {
                                TrackHelper.LogFileOpenEvent(CurrentPdfFileName);
                                FakeRibbonTabViewModel.RefreshRecentList(file);
                            }
                        }
                        else
                        {
                            // convert selected file(s)
                            var resultFilePath = FakeRibbonTabViewModel.ConvertSelectedFilesToPdf(selectedItemsList.ToArray());
                            if (!string.IsNullOrEmpty(resultFilePath) && Path.GetExtension(resultFilePath.ToLower()) == PdfHelper.PdfExtension)
                            {
                                var ret = CurrentPdfDocument == null ? OpenNewPdfAfterConverted(resultFilePath) : InsertToExistPdfAfterConverted(resultFilePath);
                                if (ret)
                                {
                                    foreach (var file in supportFileList)
                                    {
                                        TrackHelper.LogFileImportEvent(file);
                                        if (CurrentPdfDocument != null)
                                        {
                                            TrackHelper.LogInsertFromFileEvent(file);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                PdfUtilView.SetWindowLoadingStatus(false);
                tcs.TrySetResult();

            }).ContinueWith(task =>
            {
                PdfUtilView.Focus();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public bool OpenNewPdfAfterConverted(string resultFilePath)
        {
            var result = Result.Ok;
            string newName = Path.Combine(Path.GetDirectoryName(resultFilePath), Properties.Resources.DEFAULT_PDF_TITLE_NAME);

            if (!newName.Equals(resultFilePath))
            {
                EDPHelper.FileCopy(resultFilePath, newName, true);
            }

            var doc = FakeRibbonTabViewModel.OpenDocument(newName, resultFilePath, ref result, true, false);
            if (doc == null || result != Result.Ok)
            {
                return false;
            }

            FakeRibbonTabViewModel.ClearDocument(doc, newName);
            InitForTheFirstDocument();

            var pdfInfo = new PdfFileInfo(doc);
            CurrentPdfFileInfo = pdfInfo;
            CurrentOpenedItem = PdfHelper.InitWzCloudItem();
            CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
            {
                HasOpenPassword = pdfInfo.HasOpenPassword,
                HasEditPassword = pdfInfo.HasEditPassword,
                PasswordType = pdfInfo.PasswordType
            };

            var pdfFileInfo = new PDFInfo
            {
                filePath = newName,
                fileName = Path.GetFileName(newName)
            };

            Icon.SetPDFFileInfo(pdfFileInfo);
            LoadThumbnailAndBookmark(pdfFileInfo, true, true);

            return true;
        }

        public bool InsertToExistPdfAfterConverted(string resultFilePath)
        {
            var result = Result.Ok;
            var doc = FakeRibbonTabViewModel.OpenDocument(resultFilePath, null, ref result, false, false);
            if (doc == null || result != Result.Ok)
            {
                return false;
            }

            if (EDPAPIHelper.IsProcessProtectedByEDP())
            {
                var sourceEnterpriseId = EDPAPIHelper.GetEnterpriseId(resultFilePath);
                if (!string.IsNullOrEmpty(sourceEnterpriseId))
                {
                    using (var enter = new EDPAutoRestoreTempEnterpriseID(sourceEnterpriseId))
                    {
                        EDPAPIHelper.ProtectNewItem(CurrentPdfFileName);
                    }
                }
            }

            Page lastInsertPage = _curPreviewIconItem?.GetPage();
            if (PdfUtilView.CurCaretIndex == -1)
            {
                foreach (var page in doc.Pages)
                {
                    var newAddedPage = CurrentPdfDocument.Pages.Add(page);
                    if (newAddedPage == null)
                    {
                        continue;
                    }
                    InsertIconItem(newAddedPage, -1, true, true);
                    lastInsertPage = newAddedPage;
                }
            }
            else
            {
                int pageInsertedIndex = PdfUtilView.CurCaretIndex + 1;
                var newAddedPagesIndexList = new List<int>();
                foreach (var page in doc.Pages)
                {
                    var newAddedPage = CurrentPdfDocument.Pages.Insert(pageInsertedIndex, page);
                    if (newAddedPage == null)
                    {
                        continue;
                    }
                    InsertIconItem(newAddedPage, pageInsertedIndex - 1, false, true);
                    lastInsertPage = newAddedPage;
                    pageInsertedIndex++;
                    newAddedPagesIndexList.Add(PdfUtilView.CurCaretIndex + 1);
                }

                PdfUtilView.CurCaretIndex = pageInsertedIndex - 1;
                RootBookmarkTree.SyncBookmarkWithAddedPages(newAddedPagesIndexList);
            }

            //the bookmarks of new Pdf won't add in current pdf document;
            if (lastInsertPage != null)
            {
                SelectThumbnailByPage(lastInsertPage);
            }

            RefreshCommentCollectionView();
            RefreshWhenPageChanged();
            NotifyDocChanged();

            return true;
        }

        private void RecursiveDir(DirectoryInfo dirInfo, ref List<string> fileList)
        {
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                fileList.Add(fileInfo.FullName);
            }

            if (dirInfo.GetDirectories().Length == 0)
            {
                return;
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                RecursiveDir(dir, ref fileList);
            }
        }

        private Result OpenImportSelectedFiles(List<string> fileList, bool openSavedFile)
        {
            var spidList = new List<WzSvcProviderIDs>();

            if (fileList.Count > 1)
            {
                spidList.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
            }

            foreach (var file in fileList)
            {
                if (Path.GetExtension(file.ToLower()) != PdfHelper.PdfExtension)
                {
                    spidList.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                    break;
                }
            }

            string selectedFile = fileList[0];
            if (spidList.Count > 0)
            {
                var resultFiles = new List<WinzipMethods.ConvertFileResultPath>();

                bool ret = WinzipMethods.ConvertFile(PdfUtilView.WindowHandle, spidList.ToArray(), spidList.Count, fileList.ToArray(), fileList.Count, null, 0, null, resultFiles, true, true, true);

                if (ret && resultFiles.Count > 0)
                {
                    if (spidList.Contains(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM))
                    {
                        TrackHelper.LogPdfMergeEvent(fileList.Count);
                    }

                    selectedFile = resultFiles[0].path;
                }
                else
                {
                    return Result.Error;
                }
            }

            if (Path.GetExtension(selectedFile.ToLower()) != PdfHelper.PdfExtension)
            {
                return Result.Error;
            }

            if (!openSavedFile)
            {
                string resultPath = selectedFile;
                try
                {
                    if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
                    {
                        var newPdfPath = Path.Combine(FileOperation.CreateTempFolder(FileOperation.GlobalTempDir), Properties.Resources.DEFAULT_PDF_TITLE_NAME);

                        if (File.Exists(newPdfPath) && selectedFile != newPdfPath)
                        {
                            FakeRibbonTabViewModel.ClearDocument(null, string.Empty, true);
                            File.Delete(newPdfPath);
                        }

                        EDPHelper.FileCopy(selectedFile, newPdfPath, true);
                        selectedFile = newPdfPath;
                    }
                }
                catch (Exception)
                {
                    selectedFile = resultPath;
                }
            }

            var result = Result.Ok;
            if (CurrentPdfDocument == null)
            {
                var doc = FakeRibbonTabViewModel.OpenDocument(selectedFile, null, ref result, true, false);
                if (doc == null || result != Result.Ok)
                {
                    return result;
                }

                FakeRibbonTabViewModel.ClearDocument(doc, selectedFile);
                InitForTheFirstDocument();

                var pdfInfo = new PdfFileInfo(doc);
                CurrentPdfFileInfo = pdfInfo;
                CurrentPdfFilePasswordInfo = new PdfFilePasswordInfo
                {
                    HasOpenPassword = pdfInfo.HasOpenPassword,
                    HasEditPassword = pdfInfo.HasEditPassword,
                    PasswordType = pdfInfo.PasswordType
                };

                var pdfFileInfo = new PDFInfo
                {
                    filePath = selectedFile,
                    fileName = Path.GetFileName(selectedFile)
                };

                Icon.SetPDFFileInfo(pdfFileInfo);
                LoadThumbnailAndBookmark(pdfFileInfo, false, false);
            }
            else
            {
                var doc = FakeRibbonTabViewModel.OpenDocument(selectedFile, null, ref result, false, false);
                if (doc == null || result != Result.Ok)
                {
                    return result;
                }

                if (PdfUtilView.CurCaretIndex == -1)
                {
                    foreach (var page in doc.Pages)
                    {
                        var newAddedPage = CurrentPdfDocument.Pages.Add(page);
                        if (newAddedPage == null)
                        {
                            continue;
                        }
                        InsertIconItem(newAddedPage, -1, true, true);
                    }
                }
                else
                {
                    int pageInsertedIndex = PdfUtilView.CurCaretIndex + 1;
                    var newAddedPagesIndexList = new List<int>();
                    foreach (var page in doc.Pages)
                    {
                        var newAddedPage = CurrentPdfDocument.Pages.Insert(pageInsertedIndex, page);
                        if (newAddedPage == null)
                        {
                            continue;
                        }
                        InsertIconItem(newAddedPage, pageInsertedIndex - 1, false, true);
                        pageInsertedIndex++;
                        newAddedPagesIndexList.Add(PdfUtilView.CurCaretIndex + 1);
                    }

                    PdfUtilView.CurCaretIndex = pageInsertedIndex - 1;
                    RootBookmarkTree.SyncBookmarkWithAddedPages(newAddedPagesIndexList);
                }

                RefreshCommentCollectionView();
                RefreshWhenPageChanged();
                NotifyDocChanged();
            }

            return result;
        }

        public void DoCopyFromPdf()
        {
            if (LockPDFViewModel.IsSetPermissionPassword && !LockPDFViewModel.IsAllowCopyingChecked)
            {
                FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var absorber = new TextAbsorber();
            absorber.TextSearchOptions.LimitToPageBounds = true;
            var ltPoint = CalculateSelectedAreaOnPage(SelectRectLeftTopPoint, CurPreviewIconItem.GetPage());
            var rbPoint = CalculateSelectedAreaOnPage(SelectRectRightBottomPoint, CurPreviewIconItem.GetPage());
            absorber.TextSearchOptions.Rectangle = (new Aspose.Pdf.Rectangle(ltPoint.X, ltPoint.Y, rbPoint.X, rbPoint.Y));
            absorber.ExtractionOptions = new TextExtractionOptions(TextExtractionOptions.TextFormattingMode.Raw);
            CurPreviewIconItem.GetPage().Accept(absorber);
            string extractedText = absorber.Text;
            if (!string.IsNullOrEmpty(extractedText))
            {
                extractedText = RemoveFollowingLineBreak(extractedText);// Aspose result with lots of wrong line break and whiteSpace
                if (!string.IsNullOrEmpty(extractedText))
                {
                    System.Windows.Forms.Clipboard.SetText(extractedText);
                }
            }
        }

        private string RemoveFollowingLineBreak(string str)
        {
            if (str.EndsWith("\r\n") || str.EndsWith(" "))
            {
                str = str.Trim(' ').Trim('\r', '\n');
                return RemoveFollowingLineBreak(str);
            }
            else
            {
                return str;
            }
        }

        public void NotifyDocChanged()
        {
            _isDocChanged = true;
            RefreshPDFTitle();
        }

        public void ResetDocChangedState()
        {
            _isDocChanged = false;
            IsNewPDF = false;
            SelectFileInRecentList = null;
            RefreshPDFTitle();
        }

        public bool ExecuteSaveToZipCommand(bool removeDirty)
        {
            if (!_pdfUtilView.IsCalledByWinZip)
            {
                FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_SAVE_TO_ZIP_INDEPENDENTLY_TIPS);
                return false;
            }

            var task = new Task(() => { FakeRibbonTabViewModel.ExecuteOpenTask(false); });
            task.RunSynchronously();

            if (CurrentPdfDocument == null)
            {
                return false;
            }

            if (FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
            {
                return false;
            }

            if (_pdfUtilView.CheckCurrentArchiveIsReadOnly())
            {
                return false;
            }

            var ret = PdfUtilView.ExecuteSaveToZip(CurrentPdfFileName, true, removeDirty);
            if (ret)
            {
                TrackHelper.LogPdfSaveToZipEvent();
            }

            return ret;
        }

        public void CurPreviewPageEdited()
        {
            CurPreviewIconItem.UpdateThumbnailImage();
            PreviewPageNeedUpdate(CurPreviewIconItem);
            NotifyDocChanged();
        }

        #region Signature Bar Methods

        public void ExecuteAddSignatureToList()
        {
            var dialog = new AddSignatureDialog(PdfUtilView);
            if (dialog.ShowWindow() && dialog.Item != null)
            {
                SignatureList.Insert(0, dialog.Item);
                SelectedSignature = dialog.Item;
            }
        }

        public void ExecuteDeleteSignatureFromList(SignatureItem item)
        {
            if (SignatureList.Contains(item))
            {
                if (SelectedSignature == item)
                {
                    var nextItem = SignatureList[SignatureList.IndexOf(item) + 1];
                    if (nextItem.IsInitialItem && SignatureList.Count > 2)
                    {
                        nextItem = SignatureList[SignatureList.IndexOf(item) - 1];
                    }

                    SelectedSignature = nextItem;
                }

                SignatureList.Remove(item);
                if (File.Exists(item.LocalPath))
                {
                    File.Delete(item.LocalPath);
                }
            }
        }

        public void ExecuteAddSignatureToPage(WindowsPoint leftTopPoint, WindowsPoint rightBottomPoint)
        {
            if (FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
            {
                return;
            }

            if (LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission))
            {
                FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var previewPage = PreviewPaneContextMenuViewModel.GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                var leftTopPointOnPage = CalculateSelectedAreaOnPage(leftTopPoint, previewPage);
                var rightBottomPointOnPage = CalculateSelectedAreaOnPage(rightBottomPoint, previewPage);

                if (PdfHelper.AddSignature(previewPage, AddSignature.SignatureImage, leftTopPointOnPage, rightBottomPointOnPage))
                {
                    CurPreviewPageEdited();
                }
            }
        }

        public void ExecuteDeleteSignatureFromPage(bool isInRectangle)
        {
            if (FileOperation.FileIsReadOnly(PdfUtilView.WindowHandle, CurrentPdfFileName))
            {
                return;
            }

            if (LockPDFViewModel.IsSetPermissionPassword && (LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyPagesPermission))
            {
                FlatMessageWindows.DisplayWarningMessage(PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var previewPage = PreviewPaneContextMenuViewModel.GetPreviewPageFromThumbnail();
            if (previewPage != null)
            {
                bool deleteSucceed;
                if (isInRectangle)
                {
                    var leftTopPointOnPage = CalculateSelectedAreaOnPage(SelectRectLeftTopPoint, previewPage);
                    var rightBottomPointOnPage = CalculateSelectedAreaOnPage(SelectRectRightBottomPoint, previewPage);
                    deleteSucceed = PdfHelper.DeleteSignature(previewPage, leftTopPointOnPage, rightBottomPointOnPage);
                }
                else
                {
                    var pointOnPage = CalculateSelectedAreaOnPage(MouseButtonChoosedPoint, previewPage);
                    deleteSucceed = PdfHelper.DeleteSignature(previewPage, pointOnPage);
                }

                if (deleteSucceed)
                {
                    CurPreviewPageEdited();
                }

                ClearAllSelectionOnPage();
            }
        }

        public void LoadSignaturesFromLocal()
        {
            var path = ApplicationHelper.DefaultLocalUserPdfUtilSignature;
            if (Directory.Exists(path))
            {
                var allFiles = Directory.GetFiles(path, "signature-*.png", SearchOption.TopDirectoryOnly);
                foreach (var filepath in allFiles)
                {
                    if (File.Exists(filepath))
                    {
                        try
                        {
                            var sourceImage = PdfHelper.LoadImageFromPath(filepath);
                            var item = new SignatureItem(filepath, sourceImage);
                            SignatureList.Insert(0, item);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        #endregion


        #region Comment Pane Methods

        public void NotifyPageCommentChange()
        {
            Notify(nameof(PageCommentTitle));
        }

        public ObservableCollection<CommentItem> LoadCommentSourceFromPage(Page page)
        {
            var pageComments = new ObservableCollection<CommentItem>();

            foreach (var annotation in page.Annotations)
            {
                if (annotation is TextAnnotation comment)
                {
                    var item = new CommentItem(page, comment);
                    CommentItems.Add(item);
                    pageComments.Add(item);
                    NotifyPageCommentChange();
                }
            }

            return pageComments;
        }

        public void RemoveCommentOnPage(IconItem pageIcon)
        {
            foreach (var comment in pageIcon.CommentItems)
            {
                CommentItems.Remove(comment);
            }

            NotifyPageCommentChange();
        }

        public void ExcutePrepareAddComment()
        {
            if (CurPreviewIconItem != null)
            {
                var size = ConvertPageLengthToPreviewLength(PdfHelper.DefaultCommentSize, CurPreviewIconItem.GetPage());

                var ltPoint = MouseButtonChoosedPoint;
                var rbPoint = new WindowsPoint(ltPoint.X + size, ltPoint.Y + size);

                var rect = new Rect(Math.Min(ltPoint.X, rbPoint.X),
                    Math.Min(ltPoint.Y, rbPoint.Y),
                    Math.Abs(rbPoint.X - ltPoint.X),
                    Math.Abs(rbPoint.Y - ltPoint.Y));

                var item = new CommentItem(CurPreviewIconItem.GetPage(), rect);
                CommentItems.Add(item);
                SelectedCommentItem = item;
                CurPreviewIconItem.ChangeComment(item, true);

                NotifyPageCommentChange();
                PdfUtilView.ClearCommentSearch();
                PdfUtilView.UpdateCommentsOnCanvas();
            }
        }

        public void ExcuteCancelAddComment(CommentItem item)
        {
            if (CurPreviewIconItem != null)
            {
                CommentItems.Remove(item);
                CurPreviewIconItem.ChangeComment(item, false);

                NotifyPageCommentChange();
                PdfUtilView.UpdateCommentsOnCanvas();
            }
        }

        public void ExcuteAddComment(CommentItem comment, string commentContent)
        {
            if (CurPreviewIconItem != null && comment.Annotation == null)
            {
                // Operations on the preview page should be disabled during adding comment and updating the page,
                // so set the page to loading state.
                PdfUtilView.SetWindowLoadingStatus(true);
                var previewPage = CurPreviewIconItem.GetPage();
                var leftTopPointOnPage = CalculateSelectedAreaOnPage(comment.AddPosition.TopLeft, previewPage);
                var rightBottomPointOnPage = CalculateSelectedAreaOnPage(comment.AddPosition.BottomRight, previewPage);
                var annotations = new List<TextAnnotation>();
                if (PdfHelper.AddComment(previewPage, commentContent, leftTopPointOnPage, rightBottomPointOnPage, annotations))
                {
                    comment.Annotation = annotations[0];

                    RefreshCommentCollectionView();
                    CurPreviewPageEdited();
                }
                PdfUtilView.SetWindowLoadingStatus(false);
            }
        }

        public void ExcuteDeleteComment(bool isInRectangle)
        {
            if (CurPreviewIconItem != null)
            {
                var previewPage = CurPreviewIconItem.GetPage();
                if (previewPage != null)
                {
                    bool deleteSucceed = false;
                    var annotations = new List<TextAnnotation>();

                    if (isInRectangle)
                    {
                        var leftTopPointOnPage = CalculateSelectedAreaOnPage(SelectRectLeftTopPoint, previewPage);
                        var rightBottomPointOnPage = CalculateSelectedAreaOnPage(SelectRectRightBottomPoint, previewPage);
                        deleteSucceed = PdfHelper.DeleteComment(previewPage, leftTopPointOnPage, rightBottomPointOnPage, annotations);
                    }
                    else
                    {
                        var pointOnPage = CalculateSelectedAreaOnPage(MouseButtonChoosedPoint, previewPage);
                        deleteSucceed = PdfHelper.DeleteComment(previewPage, pointOnPage, annotations);
                    }

                    if (deleteSucceed)
                    {
                        foreach (var comment in annotations)
                        {
                            var item = CommentItems.Where(x => x.Annotation == comment).FirstOrDefault();
                            if (item != null)
                            {
                                CommentItems.Remove(item);
                                CurPreviewIconItem.ChangeComment(item, false);
                            }
                        }

                        NotifyPageCommentChange();
                        RefreshCommentCollectionView();
                        CurPreviewPageEdited();
                    }
                }
            }
        }

        public void ChangeCommentCollectView(string sortItem)
        {
            var view = CollectionViewSource.GetDefaultView(CommentItems);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();

                if (sortItem.Equals(Properties.Resources.COMMENT_SORT_PAGE))
                {
                    view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CommentItem.PageNumberTitle)));
                    view.SortDescriptions.Add(new SortDescription(nameof(CommentItem.PageNumber), ListSortDirection.Ascending));
                }
                else if (sortItem.Equals(Properties.Resources.COMMENT_SORT_AUTHOR))
                {
                    view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CommentItem.User)));
                    view.SortDescriptions.Add(new SortDescription(nameof(CommentItem.User), ListSortDirection.Ascending));
                }
                else if (sortItem.Equals(Properties.Resources.COMMENT_SORT_DATE))
                {
                    view.SortDescriptions.Add(new SortDescription(nameof(CommentItem.ModifyDate), ListSortDirection.Descending));
                }
            }
        }

        public void RefreshCommentCollectionView()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(PdfUtilView.CommentList.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                SearchingResultNumber = IsInSearchingCommentMode ? view.Count : -1;
            }
        }

        #endregion

        public void SetLoadWinzipSharedService(string additionalCMDParameters)
        {
            _pdfUtilView.AdjustPaneCursor(false);

            _delayLoadWinzipSharedService = Task.Factory.StartNew(() =>
            {
                string accessPermisson = string.Format("TransformAccess({0},{1},{2},{3},{4},{5})", ((int)WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM).ToString(),
                    ((int)WzSvcProviderIDs.SPID_PDF2DOC_TRANSFORM).ToString(), ((int)WzSvcProviderIDs.SPID_IMG2JPEG_TRANSFORM).ToString(),
                    ((int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM).ToString(), ((int)WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM).ToString(),
                    ((int)WzSvcProviderIDs.SPID_SIGN_PDF_TRANSFORM).ToString());

                // service id: 1 APPLET_PDF_UTILITY
                createSessionMutex.WaitOne();
                _pdfUtilView.WinzipSharedServiceHandle = WinzipMethods.CreateSession(1, accessPermisson, additionalCMDParameters);
                createSessionMutex.ReleaseMutex();
            }).ContinueWith(task =>
            {
                try
                {
                    if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                    {
                        _jobManagement = new JobManagement();
                        _jobManagement.AddProcess(_pdfUtilView.WinzipSharedServiceHandle);
                    }

                    if (_pdfUtilView.IsVisible)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            while (true)
                            {
                                if (_pdfUtilView.WindowHandle != IntPtr.Zero)
                                {
                                    break;
                                }

                                Thread.Sleep(10);
                            }

                        }).WaitWithMsgPump();
                    }

                    if (_pdfUtilView.IsClosing)
                    {
                        if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                        {
                            WinzipMethods.DestroySession(_pdfUtilView.WinzipSharedServiceHandle);
                            _pdfUtilView.WinzipSharedServiceHandle = IntPtr.Zero;
                        }
                        _delayLoadWinzipSharedService = null;
                        _loadWinzipSharedServiceSuccess = false;
                        return;
                    }

#if WZ_APPX

                    bool ret = true;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        ret = WinzipMethods.ShowUWPSubscription(_pdfUtilView.SafeHandle);
                    }));

                    if (!ret)
                    {
                        if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
#else

                    if (WinzipMethods.IsInGracePeriod(_pdfUtilView.SafeHandle))
                    {
                        // winzip is in grace period, load grace banner
                        int gracePeriodIndex = 0;
                        int graceDaysRemaining = 0;
                        string userEmail = string.Empty;
                        if (!WinzipMethods.GetGracePeriodInfo(_pdfUtilView.SafeHandle, ref gracePeriodIndex, ref graceDaysRemaining, ref userEmail))
                        {
                            if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _pdfUtilView.LoadGraceBannerFrame(gracePeriodIndex, graceDaysRemaining, userEmail);
                            }));
                        }

                        // show grace period dialog
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinzipMethods.ShowGracePeriodDialog(_pdfUtilView.SafeHandle, false);
                        }));

                        // show grace period dialog return false, close applet
                        if (!ret)
                        {
                            if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                    }
                    else
                    {
                        // in normal trial period, load trial banner
                        int nagIndex = 0;
                        int trialDaysRemaining = 0;
                        bool isAlreadyRegistered = false;
                        string buyNowUrl = string.Empty;

                        if (!WinzipMethods.GetTrialPeriodInfo(_pdfUtilView.SafeHandle, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, ref buyNowUrl))
                        {
                            if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _pdfUtilView.LoadNagBannerFrame(nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
                            }));
                        }

                        // show nag
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinzipMethods.ShowNag(_pdfUtilView.SafeHandle);
                        }));

                        // show nag return false, close applet
                        if (!ret)
                        {
                            if (_pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                    }
#endif

                    int licenseType = -1;
                    WinzipMethods.GetLicenseStatus(_pdfUtilView.SafeHandle, ref licenseType);

                    if (licenseType != (int)WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION)
                    {
                        if (!WinzipMethods.CheckLicense(_pdfUtilView.SafeHandle))
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
                }
                catch(Exception)
                {
                    CloseAppletWhileLoadSharedService();
                    return;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (_pdfUtilView.IsLoaded)
                    {
                        _pdfUtilView.Activate();
                        _pdfUtilView.Focus();
                    }
                    _pdfUtilView.AdjustPaneCursor(true);
                }));
            });
        }

        public bool WaitLoadWinzipSharedService()
        {
            if (_delayLoadWinzipSharedService != null)
            {
                _delayLoadWinzipSharedService.WaitWithMsgPump();
                if (_loadWinzipSharedServiceSuccess == null)
                {
                    _loadWinzipSharedServiceSuccess = true;
                }
                _delayLoadWinzipSharedService = null;
            }
            return _loadWinzipSharedServiceSuccess.Value;
        }

        public void DisposeJobManagement()
        {
            if (_jobManagement != null)
            {
                //RemoveAssociatedCloseProcess
                //A normal exit does not require the Job to execute AssociatedClose
                _jobManagement.RemoveAssociatedCloseProcess();
                _jobManagement.Dispose();
            }
        }

        private void CloseAppletWhileLoadSharedService()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _delayLoadWinzipSharedService = null;
                _loadWinzipSharedServiceSuccess = false;
                _pdfUtilView.Close();
            }));
        }
    }

    public class IconSouceImage : UIObject
    {
        private ImageSource _iconImage;
        protected Page _pdfPage;
        protected double _pageWidth;
        protected double _pageHeight;
        private MemoryStream _pageStream;
        protected CancellationToken _cancellationToken;
        private bool _isLoadThumbnailCompleted = false;
        public delegate void UpdateThumbnailEventHandler(bool isCompleted);
        public event UpdateThumbnailEventHandler UpdateThumbnailEvent;
        public static object LockLoadPdfThumbnail = new object();

        public override ImageSource IconImage
        {
            get
            {
                return _iconImage;
            }
            protected set
            {
                _iconImage = value;
                Notify(nameof(IconImage));
            }
        }

        public bool IsLoadThumbnailCompleted
        {
            get
            {
                return _isLoadThumbnailCompleted;
            }
            set
            {
                if (_isLoadThumbnailCompleted != value)
                {
                    _isLoadThumbnailCompleted = value;
                }
            }
        }

        public MemoryStream PageStream
        {
            get
            {
                return _pageStream;
            }
            set
            {
                if (_pageStream != value)
                {
                    _pageStream = value;
                }
            }
        }

        public void UpdateThumbnailImage(DegreesSelected degrees = DegreesSelected.None)
        {
            LoadPdfThumbnail(true, degrees);
            Notify(nameof(IconImage));
        }

        public void LoadPdfThumbnail(bool isTheLastOne, DegreesSelected degrees)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate ()
            {
                UpdateThumbnailEvent?.Invoke(false);
            }));

            LoadPdfThumbnailInBackground(degrees);

            if (isTheLastOne)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate ()
                {
                    UpdateThumbnailEvent?.Invoke(true);
                }));
            }
        }

        public void LoadPdfThumbnailInBackground(DegreesSelected degrees)
        {
            if (_pdfPage != null)
            {
                Bitmap bitmap = null;
                IntPtr pdfBmp = IntPtr.Zero;
                try
                {
                    lock (LockLoadPdfThumbnail)
                    {
                        PageStream = _pdfPage.ConvertToPNGMemoryStream();
                    }
                    
                    var memory = new MemoryStream(PageStream.GetBuffer());
                    double baseWidth = 1024;
                    int height = (int)(_pageHeight / _pageWidth * baseWidth);
                    if (degrees == DegreesSelected.On90DegreesClockwise || degrees == DegreesSelected.On270Clockwise)
                    {
                        height = (int)(_pageWidth / _pageHeight * baseWidth);
                    }
                    _pageWidth = baseWidth;
                    _pageHeight = height;
                    var Original = new Bitmap(memory);
                    bitmap = new Bitmap(Original, (int)baseWidth, height);
                    pdfBmp = bitmap.GetHbitmap();

                    if (pdfBmp != IntPtr.Zero)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                        {
                            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(pdfBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            IconImage = bitmapSource;
                        }));
                    }

                    memory.Dispose();
                    Original.Dispose();
                }
                catch (Exception e)
                {
                    var ex = e.InnerException;
                }
                finally
                {
                    GC.Collect();
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                    }

                    if (pdfBmp != IntPtr.Zero)
                    {
                        NativeMethods.DeleteObject(pdfBmp);
                    }
                }
            }
        }
    }

    public class IconSource : INotifyPropertyChanged
    {
        private UIObjects _iconSources = new UIObjects();
        public event PropertyChangedEventHandler PropertyChanged;
        private PDFInfo _pdfFileInfo;
        private int _iconCount;

        public IconSource()
        {
            IconSources.CollectionChanged -= IconSources_CollectionChanged;
            IconSources.CollectionChanged += IconSources_CollectionChanged;
        }

        private void IconSources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var icons = sender as UIObjects;
            _iconCount = icons.Count;
            Notify(nameof(IconCount));
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UIObjects IconSources
        {
            get
            {
                return _iconSources;
            }
            protected set
            {
                if (_iconSources != value)
                {
                    _iconSources = value;
                    Notify(nameof(IconSources));
                }
            }
        }

        public int IconCount
        {
            get
            {
                return _iconCount;
            }
        }

        public PDFInfo PDFFileInfo
        {
            get
            {
                return _pdfFileInfo;
            }
            protected set
            {
                _pdfFileInfo = value;
                Notify(nameof(PDFFileInfo));
            }
        }

        public string FilePath
        {
            get
            {
                return _pdfFileInfo.filePath;
            }
        }

        public string FileName
        {
            get
            {
                return _pdfFileInfo.fileName;
            }
        }

        public string Password
        {
            get
            {
                return _pdfFileInfo.password;
            }
        }

        public void SetPDFFileInfo(PDFInfo pdfFileInfo)
        {
            PDFFileInfo = pdfFileInfo;
        }
    }

    public class IconItem : IconSouceImage
    {
        private bool _isPreviewSelected = false;

        public IconItem(Page pdfPage, double width, double height, CancellationToken cancellationToken)
        {
            PutBelow = true;
            _pdfPage = pdfPage;
            _pageWidth = width;
            _pageHeight = height;
            _cancellationToken = cancellationToken;
            CommentItems = new ObservableCollection<CommentItem>();
        }

        public ObservableCollection<CommentItem> CommentItems;

        public int CommentCount => CommentItems.Count;

        public bool PutBelow
        {
            get;
            set;
        }

        public bool IsPreviewSelected
        {
            get
            {
                return _isPreviewSelected;
            }
            set
            {
                if (_isPreviewSelected != value)
                {
                    _isPreviewSelected = value;
                    Notify(nameof(IsPreviewSelected));
                }
            }
        }

        public Page GetPage()
        {
            return _pdfPage;
        }

        public void SetPage(Page page)
        {
            _pdfPage = page;

            foreach (var item in CommentItems)
            {
                item.SetPage(page);
            }
        }

        public void SetIcon(ImageSource image)
        {
            IconImage = image;
        }

        public void SetName(string name)
        {
            Name = name;
            Notify(nameof(Name));
        }

        public void ChangeComment(CommentItem item, bool isAdd)
        {
            if (isAdd)
            {
                CommentItems.Add(item);
            }
            else
            {
                CommentItems.Remove(item);
            }

            Notify(nameof(CommentCount));
        }
    }

    public class IconThumbnailManager
    {
        private static bool _shutDown = false;
        private static List<WindowsControl> _renderList = new List<WindowsControl>();
        private static List<IconItem> _backgroundRenderList = new List<IconItem>();
        private static object _lock = new object();
        private static object _backgroundLock = new object();

        public static void Init()
        {
            int renderThreadsCount = 3;
            for (int i = 0; i < renderThreadsCount; i++)
            {
                var renderThread = new Thread(RenderThreadProc);
                renderThread.Name = "ThumbnailRenderThread";
                renderThread.IsBackground = true;
                renderThread.Start();
            }

            int defaultThreadsCount = 5;
            int workerThreads;
            int portThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            defaultThreadsCount = defaultThreadsCount > (workerThreads / 10) ? (workerThreads / 10) : defaultThreadsCount;
            for (int i = 0; i < defaultThreadsCount; i++)
            {
                var renderThread = new Thread(RenderBackgroundThreadProc);
                renderThread.Name = "BackgroundRenderThread";
                renderThread.IsBackground = true;
                renderThread.Start();
            }
        }

        public static void ShutDown()
        {
            _shutDown = true;
        }

        public static void AddRender(WindowsControl item)
        {
            lock (_lock)
            {
                if (item != null && !_renderList.Contains(item))
                {
                    _renderList.Add(item);
                }
            }
        }

        public static void RemoveRender(WindowsControl item)
        {
            lock (_lock)
            {
                if (_renderList.Contains(item))
                {
                    _renderList.Remove(item);
                }
            }
        }

        public static void AddBackgroundRender(IconItem item)
        {
            lock (_backgroundLock)
            {
                if (item != null && !_backgroundRenderList.Contains(item))
                {
                    _backgroundRenderList.Add(item);
                }
            }
        }

        public static void RemoveBackgroundRender(IconItem item)
        {
            lock (_backgroundLock)
            {
                if (_backgroundRenderList.Contains(item))
                {
                    _backgroundRenderList.Remove(item);
                }
            }
        }

        public static void ClearBackgroundRender()
        {
            lock (_backgroundLock)
            {
                _backgroundRenderList.Clear();
            }
        }

        private static void RenderThreadProc()
        {
            while (!_shutDown)
            {
                WindowsControl control = null;
                lock (_lock)
                {
                    if (_renderList.Count != 0)
                    {
                        control = _renderList[0];
                        _renderList.RemoveAt(0);
                    }
                }

                if (control != null)
                {
                    IconItem iconItem = null;
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate ()
                    {
                        iconItem = control.DataContext as IconItem;
                    }));

                    if (iconItem != null)
                    {
                        if (_renderList.Count == 0)
                        {
                            iconItem.LoadPdfThumbnail(true, DegreesSelected.None);
                        }
                        else
                        {
                            iconItem.LoadPdfThumbnail(false, DegreesSelected.None);
                        }
                        iconItem.IsLoadThumbnailCompleted = true;
                    }
                }

                Thread.Sleep(1);
            }
        }

        private static void RenderBackgroundThreadProc()
        {
            while (!_shutDown)
            {
                IconItem iconItem = null;
                lock (_backgroundLock)
                {
                    if (_backgroundRenderList.Count != 0 && _renderList.Count == 0)
                    {
                        iconItem = _backgroundRenderList[0];
                        _backgroundRenderList.RemoveAt(0);
                    }
                }

                if (iconItem != null && !iconItem.IsLoadThumbnailCompleted)
                {
                    iconItem.LoadPdfThumbnailInBackground(DegreesSelected.None);
                    iconItem.IsLoadThumbnailCompleted = true;
                }

                Thread.Sleep(1);
            }
        }
    }

    public class RecentFile : ObservableObject
    {
        private string _recentFileIndex = string.Empty;
        private string _recentFileName = string.Empty;
        private WzCloudItem4 _recentOpenFileCloudItem = new WzCloudItem4();
        private string _cloudName = string.Empty;
        private long _fileSize;
        private DateTime? _fileModifiedDate;

        public RecentFile()
        {
            IsCloudItem = false;
            IsLocalPortableDeviceItem = false;
        }

        public void UpdateFileInfo()
        {
            if (!IsCloudItem && !IsLocalPortableDeviceItem)
            {
                string itemPath = !string.IsNullOrEmpty(RecentOpenFileCloudItem.itemId) ?
                RecentOpenFileCloudItem.itemId : Path.Combine(RecentOpenFileCloudItem.parentId, RecentOpenFileCloudItem.name);

                if (!string.IsNullOrEmpty(itemPath))
                {
                    FileInfo fi = new FileInfo(itemPath);
                    if (fi.Exists)
                    {
                        FileSize = fi.Length;
                        FileModifiedDate = fi.LastWriteTime;
                    }
                }
            }
            else
            {
                var modifyDate = RecentOpenFileCloudItem.modified;
                FileModifiedDate = new DateTime(modifyDate.wYear, modifyDate.wMonth, modifyDate.wDay);

                FileSize = RecentOpenFileCloudItem.length;
            }
        }

        public string RecentFileIndex
        {
            get
            {
                return _recentFileIndex;
            }
            set
            {
                if (_recentFileIndex != value)
                {
                    _recentFileIndex = value;
                    Notify(nameof(RecentFileIndex));
                }
            }
        }

        public string RecentFileName
        {
            get
            {
                return _recentFileName;
            }
        }

        public string RecentFileFullName
        {
            get
            {
                if (IsCloudItem)
                {
                    return string.Format("({0}:{1}){2}", _cloudName, _recentOpenFileCloudItem.path, _recentFileName);
                }
                else
                {
                    return string.Format("({0}){1}", _recentOpenFileCloudItem.parentId, _recentFileName);
                }
            }
        }

        public bool IsCloudItem
        {
            get;
            set;
        }

        public bool IsLocalPortableDeviceItem
        {
            get;
            set;
        }

        public string RecentFileTooltip
        {
            get
            {
                if (IsCloudItem || IsLocalPortableDeviceItem)
                {
                    var providerDisplayName = WinzipMethods.GetProviderDisplayInfo(IntPtr.Zero, _recentOpenFileCloudItem.profile);
                    if (string.IsNullOrEmpty(_recentOpenFileCloudItem.path))
                    {
                        return string.Format("{0} ({1})", _recentFileName, providerDisplayName);
                    }
                    else
                    {
                        return string.Format("{0} ({1}: {2})", _recentFileName, providerDisplayName, _recentOpenFileCloudItem.path);
                    }
                }
                else
                {
                    return string.Format("{0} ({1})", _recentFileName , _recentOpenFileCloudItem.parentId);
                }
            }
        }

        public DateTime? FileModifiedDate
        {
            get
            {
                return _fileModifiedDate;
            }
            set
            {
                if (_fileModifiedDate != value)
                {
                    _fileModifiedDate = value;
                    Notify("FileModifiedDate");
                    Notify("FileModifiedDateString");
                }
            }
        }

        public string FileModifiedDateString
        {
            get
            {
                if (_fileModifiedDate != null)
                {
                    return ((DateTime)_fileModifiedDate).ToString("MMMM d, yyyy", System.Globalization.CultureInfo.CurrentUICulture);
                }

                return string.Empty;
            }
        }

        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    Notify("FileSize");
                }
            }
        }

        public WzCloudItem4 RecentOpenFileCloudItem
        {
            get
            {
                return _recentOpenFileCloudItem;
            }
            set
            {
                if (!_recentOpenFileCloudItem.Equals(value))
                {
                    _recentOpenFileCloudItem = value;
                    if (IsCloudItem || IsLocalPortableDeviceItem)
                    {
                        _cloudName = _recentOpenFileCloudItem.profile.name;
                        _recentFileName = _recentOpenFileCloudItem.name;
                    }
                    else
                    {
                        _recentFileName = Path.GetFileName(_recentOpenFileCloudItem.itemId);
                        if (string.IsNullOrEmpty(_recentFileName))
                        {
                            _recentFileName = _recentOpenFileCloudItem.name;
                        }
                    }

                    Notify(nameof(RecentFileName));
                    Notify(nameof(RecentFileTooltip));
                }
            }
        }
    }

    class RootBookmarkTree : ObservableObject
    {
        private BookmarkTreeViewItem _startItem;
        private List<BookmarkTreeViewItem> _selectedList = new List<BookmarkTreeViewItem>();
        private ObservableCollection<BookmarkTreeViewItem> _bookmarkItems;
        private OutlineCollection _outlines;
        private PdfUtilViewModel _viewModel;
        private Bookmarks _bookmarks;

        public RootBookmarkTree(PdfUtilViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public ObservableCollection<BookmarkTreeViewItem> BookmarkItems
        {
            get
            {
                if (_bookmarkItems == null)
                {
                    _bookmarkItems = new ObservableCollection<BookmarkTreeViewItem>();
                }

                return _bookmarkItems;
            }
            set
            {
                if (_bookmarkItems != value)
                {
                    _bookmarkItems = value;
                    Notify(nameof(BookmarkItems));
                }
            }
        }

        public OutlineCollection Outlines
        {
            get
            {
                return _outlines;
            }
            set
            {
                if (_outlines != value)
                {
                    _outlines = value;
                    Notify(nameof(Outlines));
                }
            }
        }

        public void InitBookmarkItemsList(RootBookmarkTree rootTree, OutlineCollection outlines, int curBookmarkIndex, CancellationToken cancellationToken)
        {
            var bookmarkEditor = new PdfBookmarkEditor(_viewModel.CurrentPdfDocument);

            lock (IconSouceImage.LockLoadPdfThumbnail)
            {
                _bookmarks = bookmarkEditor.ExtractBookmarks(true);
            }

            Outlines = outlines;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _viewModel.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                int curIndex = 1;
                foreach (var outline in outlines)
                {
                    if (curIndex > curBookmarkIndex)
                    {
                        var item = new BookmarkTreeViewItem(rootTree, outline);
                        CreateChildItem(ref item, outline);
                        BookmarkItems.Add(item);
                    }
                    curIndex++;
                }

                _viewModel.PdfUtilView.AdjustBookmarkTabCursor(true);
                _viewModel.IsBookmarkOptionEnable = true;
            }));
        }

        public void RefreshBookmarks()
        {
            var bookmarkEditor = new PdfBookmarkEditor(_viewModel.CurrentPdfDocument);
            lock (IconSouceImage.LockLoadPdfThumbnail)
            {
                _bookmarks = bookmarkEditor.ExtractBookmarks(true);
            }
        }

        public void CreateChildItem(ref BookmarkTreeViewItem parent, OutlineItemCollection outline)
        {
            if (outline.Count == 0)
            {
                return;
            }

            foreach (var childOutline in outline)
            {
                var item = new BookmarkTreeViewItem(this, childOutline);
                CreateChildItem(ref item, childOutline);
                item.Parent = parent;
                parent.BookmarkItems.Add(item);
            }
        }

        public BookmarkLocationInfo TryGetBookmarkLocationInfo(BookmarkTreeViewItem treeViewItem)
        {
            var bookmark = FindBookmarkByTreeViewItem(BookmarkItems, _bookmarks, treeViewItem);
            if (bookmark != null)
            {
                var locationInfo = new BookmarkLocationInfo(bookmark.PageNumber, bookmark.PageDisplay_Left, bookmark.PageDisplay_Right, bookmark.PageDisplay_Top, bookmark.PageDisplay_Bottom, bookmark.Title);
                return locationInfo;
            }

            return new BookmarkLocationInfo();
        }

        public Bookmark FindBookmarkByTreeViewItem(ObservableCollection<BookmarkTreeViewItem> items, Bookmarks bookmarks, BookmarkTreeViewItem srcItem)
        {
            if (items.Count == 0 || items.Count != bookmarks.Count)
            {
                return null;
            }

            if (items.Contains(srcItem))
            {
                return bookmarks[items.IndexOf(srcItem)];
            }

            int curIndex = 0;
            foreach (var child in items)
            {
                var res = FindBookmarkByTreeViewItem(child.BookmarkItems, bookmarks[curIndex].ChildItems, srcItem);
                if (res != null)
                {
                    return res;
                }
                curIndex++;
            }

            return null;
        }

        public void UpdatePreviewPage()
        {
            _viewModel.SelectThumbnailByCurSelectedBookmark();
        }

        public void SelectThumbnailByPage(int pageIndex)
        {
            if (_viewModel.CurrentPdfDocument.Pages.Count < pageIndex || pageIndex == 0)
            {
                return;
            }

            _viewModel.IsBookmarkSelectionChanged = true;
            _viewModel.SelectThumbnailByPage(_viewModel.CurrentPdfDocument.Pages[pageIndex]);
        }

#region Select
        public BookmarkTreeViewItem StartItem
        {
            get { return _startItem; }
            set
            {
                if (_startItem != value)
                {
                    _startItem = value;
                }
            }
        }

        public List<BookmarkTreeViewItem> SelectedList
        {
            get
            {
                return _selectedList;
            }
        }

        public void RefreshSelectedList(BookmarkTreeViewItem control)
        {
            if (_selectedList.Contains(control))
            {
                _selectedList.Remove(control);
            }
            else
            {
                _selectedList.Add(control);
            }
        }

        public void DeSelectOther(BookmarkTreeViewItem control = null, bool isDeSelectAll = false)
        {
            var itemToDeselect = new List<BookmarkTreeViewItem>();
            if (isDeSelectAll)
            {
                itemToDeselect.AddRange(SelectedList);
                foreach (var item in itemToDeselect)
                {
                    item.IsSelected = false;
                }
            }
            else
            {

                foreach (var item in SelectedList)
                {
                    if (item != control)
                    {
                        itemToDeselect.Add(item);
                    }
                }

                foreach (var item in itemToDeselect)
                {
                    item.IsSelected = false;
                }
            }
        }
#endregion

#region FindControl
        public BookmarkTreeViewItem FindControl(OutlineItemCollection outlineItem)
        {
            foreach (var item in BookmarkItems)
            {
                if (item.Level == outlineItem.Level)
                {
                    if (item.OutlineItem == outlineItem)
                    {
                        return item;
                    }
                }
                else if (item.Level < outlineItem.Level)
                {
                    var res = FindControlInChildItems(item, outlineItem);
                    if (res != null)
                    {
                        return res;
                    }
                }
            }
            return null;
        }

        private BookmarkTreeViewItem FindControlInChildItems(BookmarkTreeViewItem control, OutlineItemCollection outlineItem)
        {
            if (!control.IsBookmarkHasChild)
            {
                return null;
            }

            foreach (var child in control.BookmarkItems)
            {
                if (child.Level == outlineItem.Level)
                {
                    if (child.OutlineItem == outlineItem)
                    {
                        return child;
                    }
                }
                else if (child.Level < outlineItem.Level)
                {
                    var res = FindControlInChildItems(child, outlineItem);
                    if (res != null)
                    {
                        return res;
                    }
                }
            }

            return null;
        }

        public int GetRootIndex(BookmarkTreeViewItem control)
        {
            var parent = control.Parent;
            if (parent == null)
            {
                return BookmarkItems.IndexOf(control);
            }
            else
            {
                return GetRootIndex(parent);
            }
        }

#endregion

#region Action
        public void ExecuteGoToBookmark()
        {
            if (SelectedList.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_GOTO_BOOKMARK_TIPS);
                return;
            }

            _viewModel.SelectThumbnailByCurSelectedBookmark();
        }

        private bool InsertIntoOutlineCollection(OutlineCollection collection, int insertIndex, OutlineItemCollection item)
        {
            if (collection == null || item == null)
            {
                return false;
            }

            if (insertIndex < 0 || insertIndex > collection.Count)
            {
                return false;
            }

            List<OutlineItemCollection> tempList = new List<OutlineItemCollection>();
            for (int index = collection.Count; index >= insertIndex + 1; --index)
            {
                tempList.Add(collection[index]);
                collection.Remove(index);
            }

            collection.Add(item);

            for (int index = tempList.Count - 1; index >= 0; --index)
            {
                collection.Add(tempList[index]);
            }

            return true;
        }

        private void addSubBookMark(BookmarkTreeViewItem treeViewItem, string strBookName, bool isPreviewCommand)
        {
            if (string.IsNullOrEmpty(strBookName))
            {
                return;
            }

            double top = 0;
            if (isPreviewCommand)
            {
                top = _viewModel.CalculateBookmarkDestinationOnPage(_viewModel.MouseButtonChoosedPoint, _viewModel.CurPreviewIconItem.GetPage()).Y;
            }
            else
            {
                top = _viewModel.CalculateBookmarkDestinationOnPage(new WindowsPoint(0, _viewModel.CurPreviewScrollVerticalOffset), _viewModel.CurPreviewIconItem.GetPage()).Y;
            }

            var info = new BookmarkLocationInfo()
            {
                Name = strBookName,
                PageIndex = _viewModel.CurrentPdfDocument.Pages.IndexOf(_viewModel.CurPreviewIconItem.GetPage()),
                Top = top
            };

            if (info.PageIndex <= 0)
            {
                return;
            }

            var item = new OutlineItemCollection(Outlines);
            item.Title = info.Name;
            item.Action = GenerateBookmarkAction(info.PageIndex, info.Left, info.Top);

            var bookmarkItems = treeViewItem == null ? BookmarkItems : treeViewItem.BookmarkItems;
            Outlines outlineItem = Outlines;
            if (treeViewItem != null)
            {
                outlineItem = treeViewItem.OutlineItem;
            }

            if (bookmarkItems == null || outlineItem == null)
            {
                return;
            }
            int insertIndex = 0;
            foreach (BookmarkTreeViewItem subTreeViewItem in bookmarkItems)
            {
                if (info.PageIndex < subTreeViewItem.BookmarkLocationInfo.PageIndex)
                {
                    break;
                }
                else if (info.PageIndex == subTreeViewItem.BookmarkLocationInfo.PageIndex)
                {
                    if (info.Top < subTreeViewItem.BookmarkLocationInfo.Top)
                    {
                        break;
                    }
                }
                ++insertIndex;
            }
            if (insertIndex > bookmarkItems.Count)
            {
                return;
            }

            if (outlineItem is OutlineItemCollection)
            {
                OutlineItemCollection itemCollection = outlineItem as OutlineItemCollection;
                if (insertIndex < itemCollection.Count)
                {
                    itemCollection.Insert(insertIndex + 1, item);
                }
                else
                {
                    itemCollection.Add(item);
                }
            }
            else if (outlineItem is OutlineCollection)
            {
                OutlineCollection collection = outlineItem as OutlineCollection;
                bool isInsertSuccess = InsertIntoOutlineCollection(collection, insertIndex, item);
                if (!isInsertSuccess)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            var newTreeViewItem = new BookmarkTreeViewItem(this, item);
            if (treeViewItem == null)
            {
                BookmarkItems.Insert(insertIndex, newTreeViewItem);
                Notify(nameof(BookmarkItems));
                DeSelectOther(newTreeViewItem);
                newTreeViewItem.IsSelected = true;
            }
            else
            {
                newTreeViewItem.Parent = treeViewItem;

                treeViewItem.BookmarkItems.Insert(insertIndex, newTreeViewItem);
                treeViewItem.RefreshBookmarkChildState(true);
                DeSelectOther(newTreeViewItem);
                newTreeViewItem.IsSelected = true;
            }

            RefreshBookmarks();
            _viewModel.NotifyDocChanged();
        }

        public void ExecuteAddBookmark(BookmarkTreeViewItem treeViewItem, bool isPreviewCommand = false)
        {
            if (_viewModel.CurrentPdfDocument == null || _viewModel.CurrentPdfDocument.Pages.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.WARNING_NO_OPEN_PDF);
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var dialog = new BookmarkActionView(_viewModel.PdfUtilView, string.Empty);
            if (dialog.ShowWindow() && !string.IsNullOrEmpty(dialog.Text))
            {
                addSubBookMark(treeViewItem, dialog.Text, isPreviewCommand);
            }
        }

        public void ExecuteAddSubBookmark(BookmarkTreeViewItem treeViewItem, bool isPreviewCommand = false)
        {
            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            string title = Properties.Resources.ADD_SUBBOOKMARK_TITLE;
            var dialog = new BookmarkActionView(_viewModel.PdfUtilView, title);
            if (dialog.ShowWindow() && !string.IsNullOrEmpty(dialog.Text))
            {
                addSubBookMark(treeViewItem, dialog.Text, isPreviewCommand);
            }
        }

        public void ExecuteDelBookmark()
        {
            if (SelectedList.Count == 0)
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, Properties.Resources.EXECUTE_REMOVE_BOOKMARK_TIPS);
                return;
            }

            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            var controlToDel = new List<BookmarkTreeViewItem>();
            foreach (var item in BookmarkItems)
            {
                if (item.IsSelected)
                {
                    item.IsSelected = false;
                    Outlines.Remove(item.OutlineItem);
                    controlToDel.Add(item);
                }
                else
                {
                    if (DeleteChildBookmark(item))
                    {
                        item.RefreshBookmarkChildState();
                    }
                }
            }

            foreach (var item in controlToDel)
            {
                BookmarkItems.Remove(item);
            }

            RefreshBookmarks();
            _viewModel.NotifyDocChanged();
        }

        private bool DeleteChildBookmark(BookmarkTreeViewItem bookmarkItemControl)
        {
            if (!bookmarkItemControl.IsBookmarkHasChild)
            {
                return false;
            }

            bool res = false;
            var controlToDel = new List<BookmarkTreeViewItem>();
            foreach (var child in bookmarkItemControl.BookmarkItems)
            {
                if (child.IsSelected)
                {
                    child.IsSelected = false;
                    bookmarkItemControl.OutlineItem.Remove(child.OutlineItem);
                    controlToDel.Add(child);
                    res = true;
                }
                else
                {
                    res = DeleteChildBookmark(child);
                    if (res)
                    {
                        child.RefreshBookmarkChildState();
                    }
                }
            }

            foreach (var item in controlToDel)
            {
                bookmarkItemControl.BookmarkItems.Remove(item);
            }

            return res;
        }

        public void ExecuteRenameBookmark(BookmarkTreeViewItem bookmarkItemControl)
        {
            if (_viewModel.LockPDFViewModel.IsSetPermissionPassword && (_viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.None
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifyCommentsPermission
                || _viewModel.LockPDFViewModel.CurAllowChanges == AllowChanges.ModifySignaturePermission))
            {
                FlatMessageWindows.DisplayWarningMessage(_viewModel.PdfUtilView.WindowHandle, string.Format(Properties.Resources.PDF_LOCKED_WARNING, Properties.Resources.PDF_FILE));
                return;
            }

            string title = Properties.Resources.RENAME_BOOKMARK_TITLE;
            var dialog = new BookmarkActionView(_viewModel.PdfUtilView, title);
            dialog.InitDefaultText(bookmarkItemControl.Title);
            if (dialog.ShowWindow() && !string.IsNullOrEmpty(dialog.Text))
            {
                bookmarkItemControl.Title = dialog.Text;
                _viewModel.NotifyDocChanged();
            }
        }

#endregion

#region Sync with page operation

        public PdfAction GenerateBookmarkAction(int pageIndex, double left, double top)
        {
            var destination = ExplicitDestination.CreateDestination(
                        pageIndex,
                        ExplicitDestinationType.XYZ,
                        new double[] { pageIndex, top });
            return new GoToAction(destination);
        }

        private bool SyncBookmarkWithAddedPages(ObservableCollection<BookmarkTreeViewItem> bookmarkTreeViewItemList, List<int> addedPagesIndexList)
        {
            //assert deletedPagesIndexList is Descending 
            if (bookmarkTreeViewItemList == null || addedPagesIndexList == null)
            {
                return false;
            }

            for (int bookmarkIndex = 0; bookmarkIndex < bookmarkTreeViewItemList.Count; ++bookmarkIndex)
            {
                var bookmarkTreeViewItem = bookmarkTreeViewItemList[bookmarkIndex];
                if (bookmarkTreeViewItem == null)
                {
                    continue;
                }

                int pageIndexAdjust = 0;
                for (; pageIndexAdjust < addedPagesIndexList.Count; ++pageIndexAdjust)
                {
                    if (addedPagesIndexList[pageIndexAdjust] > bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex)
                    {
                        break;
                    }
                }

                if (pageIndexAdjust != 0)
                {
                    int newPageIndex = bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex + pageIndexAdjust;
                    bookmarkTreeViewItem.OutlineItem.Action = GenerateBookmarkAction(newPageIndex, bookmarkTreeViewItem.BookmarkLocationInfo.Left, bookmarkTreeViewItem.BookmarkLocationInfo.Top);

                    var newBookmarkLocationInfo = new BookmarkLocationInfo(bookmarkTreeViewItem.BookmarkLocationInfo);
                    newBookmarkLocationInfo.PageIndex = newPageIndex;
                    bookmarkTreeViewItem.BookmarkLocationInfo = newBookmarkLocationInfo;
                }

                SyncBookmarkWithAddedPages(bookmarkTreeViewItem.BookmarkItems, addedPagesIndexList);
            }

            return true;
        }

        public bool SyncBookmarkWithAddedPages(List<int> addedPagesIndexList)
        {
            if (BookmarkItems == null || addedPagesIndexList == null)
            {
                return false;
            }

            addedPagesIndexList.Sort((a, b) => a.CompareTo(b));
            return SyncBookmarkWithAddedPages(BookmarkItems, addedPagesIndexList);
        }

        private bool SyncBookmarkWithDeletedPages(ObservableCollection<BookmarkTreeViewItem> bookmarkTreeViewItemList, List<int> deletedPagesIndexList, bool isSyncOutline)
        {
            //assert deletedPagesIndexList is Ascending
            if (bookmarkTreeViewItemList == null || deletedPagesIndexList == null)
            {
                return false;
            }

            for (int bookmarkIndex = 0; bookmarkIndex < bookmarkTreeViewItemList.Count; ++bookmarkIndex)
            {
                var bookmarkTreeViewItem = bookmarkTreeViewItemList[bookmarkIndex];
                if (bookmarkTreeViewItem == null)
                {
                    continue;
                }

                bool isEqual = false;
                int pageIndexAdjust = 0;
                for (; pageIndexAdjust < deletedPagesIndexList.Count; ++pageIndexAdjust)
                {
                    if (deletedPagesIndexList[pageIndexAdjust] == bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex)
                    {
                        isEqual = true;
                        break;
                    }
                    else if (deletedPagesIndexList[pageIndexAdjust] > bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex)
                    {
                        break;
                    }
                }

                if (isEqual)
                {
                    if (isSyncOutline)
                    {
                        var parentOutlines = bookmarkTreeViewItem.OutlineItem.Parent;
                        if (parentOutlines is OutlineCollection)
                        {
                            var parentOutlineCollection = parentOutlines as OutlineCollection;
                            parentOutlineCollection.Remove(bookmarkTreeViewItem.OutlineItem);
                        }
                        else if (parentOutlines is OutlineItemCollection)
                        {
                            var parentOutlineItemCollection = parentOutlines as OutlineItemCollection;
                            parentOutlineItemCollection.Remove(bookmarkTreeViewItem.OutlineItem);
                        }
                    }
                    else
                    {
                        bookmarkTreeViewItemList.RemoveAt(bookmarkIndex);
                        --bookmarkIndex;
                    }
                    continue;
                }
                else if (pageIndexAdjust != 0)
                {
                    int newPageIndex = bookmarkTreeViewItem.BookmarkLocationInfo.PageIndex - pageIndexAdjust;
                    if (isSyncOutline)
                    {
                        bookmarkTreeViewItem.OutlineItem.Action = GenerateBookmarkAction(newPageIndex, bookmarkTreeViewItem.BookmarkLocationInfo.Left, bookmarkTreeViewItem.BookmarkLocationInfo.Top);
                    }
                    else
                    {
                        var newBookmarkLocationInfo = new BookmarkLocationInfo(bookmarkTreeViewItem.BookmarkLocationInfo);
                        newBookmarkLocationInfo.PageIndex = newPageIndex;
                        bookmarkTreeViewItem.BookmarkLocationInfo = newBookmarkLocationInfo;
                    }
                }

                SyncBookmarkWithDeletedPages(bookmarkTreeViewItem.BookmarkItems, deletedPagesIndexList, isSyncOutline);
            }

            return true;
        }

        public bool SyncBookmarkWithDeletedPages(List<int> deletedPagesIndexList)
        {
            if (BookmarkItems == null || deletedPagesIndexList == null)
            {
                return false;
            }

            deletedPagesIndexList.Sort((a, b) => a.CompareTo(b));

            SyncBookmarkWithDeletedPages(BookmarkItems, deletedPagesIndexList, true);
            SyncBookmarkWithDeletedPages(BookmarkItems, deletedPagesIndexList, false);
            return true;
        }

#endregion
    }

    public struct PdfLockStatus
    {
        public bool isSetOpenPassword;
        public bool isSetPermissionPassword;
        public string openPassword;
        public string permissionPassword;
        public Permissions permissions;
    }

    public struct PdfFilePasswordInfo
    {
        public bool HasEditPassword;
        public bool HasOpenPassword;
        public PasswordType PasswordType;
    }
}
