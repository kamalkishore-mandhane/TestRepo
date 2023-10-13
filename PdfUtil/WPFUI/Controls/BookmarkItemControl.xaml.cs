using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Facades;
using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for BookmarkItemControl.xaml
    /// </summary>
    public partial class BookmarkItemControl : BaseUserControl
    {
        private bool _isMouseDown = false;
        private bool _isRightMouseDown = false;

        public BookmarkItemControl()
        {
            InitializeComponent();
        }

        public bool IsRightMouseDown
        {
            get
            {
                return _isRightMouseDown;
            }
            set
            {
                if (_isRightMouseDown != value)
                {
                    _isRightMouseDown = value;
                }
            }
        }

        public bool IsMouseDown
        {
            get
            {
                return _isMouseDown;
            }
            set
            {
                if (_isMouseDown != value)
                {
                    _isMouseDown = value;
                }
            }
        }

        private void BookmarkItem_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            var bookmarkTreeViewItem = DataContext as BookmarkTreeViewItem;
            if (bookmarkTreeViewItem != null && bookmarkTreeViewItem.Control == null)
            {
                bookmarkTreeViewItem.Control = this;
                if (bookmarkTreeViewItem.IsSelected)
                {
                    this.RefreshTreeViewSelected(true);
                }
            }

            var treeViewItem = VisualTreeHelperUtils.FindAncestor<TreeViewItem>(this);
            if (treeViewItem != null)
            {
                treeViewItem.Selected += TreeViewItem_Selected;
                treeViewItem.Unselected += TreeViewItem_Unselected;
                treeViewItem.Expanded += TreeViewItem_Expanded;
                treeViewItem.Collapsed += TreeViewItem_Collapsed;
                treeViewItem.KeyDown += TreeViewItem_KeyDown;
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var bookmark = DataContext as BookmarkTreeViewItem;
                if (bookmark != null)
                {
                    bookmark.RootTree.UpdatePreviewPage();
                }
                e.Handled = true;
            }
        }

        private void BookmarkItem_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;

            var treeViewItem = VisualTreeHelperUtils.FindAncestor<TreeViewItem>(this);
            if (treeViewItem != null)
            {
                treeViewItem.Selected -= TreeViewItem_Selected;
                treeViewItem.Unselected -= TreeViewItem_Unselected;
                treeViewItem.Expanded -= TreeViewItem_Expanded;
                treeViewItem.Collapsed -= TreeViewItem_Collapsed;
                treeViewItem.KeyDown -= TreeViewItem_KeyDown;
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (KeyboardUtil.IsShiftKeyDown || KeyboardUtil.IsCtrlKeyDown)
            {
                return;
            }

            var source = e.OriginalSource as TreeViewItem;
            var bookmarkTreeViewItem = DataContext as BookmarkTreeViewItem;
            if (source == null || bookmarkTreeViewItem == null || _isMouseDown)
            {
                return;
            }

            if (source.Header == bookmarkTreeViewItem)
            {
                bookmarkTreeViewItem.IsSelected = true;
            }
            else
            {
                var treeViewItem = VisualTreeHelperUtils.FindAncestor<TreeViewItem>(this);
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false;
                }
            }
        }

        private void TreeViewItem_Unselected(object sender, RoutedEventArgs e)
        {
            if (KeyboardUtil.IsShiftKeyDown || KeyboardUtil.IsCtrlKeyDown)
            {
                return;
            }

            var bookmarkTreeViewItem = DataContext as BookmarkTreeViewItem;
            if (bookmarkTreeViewItem != null)
            {
                bookmarkTreeViewItem.IsSelected = false;
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var bookmark = DataContext as BookmarkTreeViewItem;
            if (bookmark != null)
            {
                bookmark.OutlineItem.Open = true;
            }
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            var bookmark = DataContext as BookmarkTreeViewItem;
            if (bookmark != null)
            {
                bookmark.OutlineItem.Open = false;
            }
        }

        public void ExpandTreeViewItem(bool isExpanded)
        {
            var treeViewItem = VisualTreeHelperUtils.FindAncestor<TreeViewItem>(this);
            if (treeViewItem != null)
            {
                treeViewItem.IsExpanded = isExpanded;
            }
        }

        public void RefreshTreeViewSelected(bool isSelected)
        {
            if (KeyboardUtil.IsShiftKeyDown || KeyboardUtil.IsCtrlKeyDown)
            {
                return;
            }

            var treeViewItem = VisualTreeHelperUtils.FindAncestor<TreeViewItem>(this);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = isSelected;
            }
        }

        private void ItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            if (e.ChangedButton == MouseButton.Right)
            {
                _isRightMouseDown = true;
            }

            var bookmark = DataContext as BookmarkTreeViewItem;
            if (bookmark == null)
            {
                return;
            }

            if (!KeyboardUtil.IsCtrlKeyDown && !KeyboardUtil.IsShiftKeyDown && !(e.ChangedButton == MouseButton.Right && bookmark.IsSelected))
            {
                bookmark.IsSelected = true;
                bookmark.RootTree.DeSelectOther(bookmark);
                bookmark.RootTree.StartItem = bookmark;
            }
            else if (KeyboardUtil.IsCtrlKeyDown && !KeyboardUtil.IsShiftKeyDown)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    bookmark.IsSelected = !bookmark.IsSelected;
                    bookmark.RootTree.StartItem = bookmark;
                }
            }
            else if (!KeyboardUtil.IsCtrlKeyDown && KeyboardUtil.IsShiftKeyDown)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    bookmark.IsSelected = true;
                    bookmark.RootTree.DeSelectOther(bookmark);
                    bookmark.RootTree.StartItem = bookmark;
                    return;
                }

                BookmarkTreeViewItem parent;
                bookmark.RootTree.DeSelectOther(bookmark);
                if (bookmark.RootTree.StartItem == null)
                {
                    parent = bookmark.Parent;
                    if (parent == null)
                    {
                        bookmark.RootTree.StartItem = bookmark.RootTree.BookmarkItems[0];
                    }
                    else
                    {
                        bookmark.RootTree.StartItem = parent.BookmarkItems[0];
                    }
                }

                parent = bookmark.RootTree.StartItem.Parent;
                int startIndex;
                int curIndex;
                if (parent == null)
                {
                    startIndex = bookmark.RootTree.BookmarkItems.IndexOf(bookmark.RootTree.StartItem);
                    curIndex = bookmark.RootTree.BookmarkItems.IndexOf(bookmark);
                    if (startIndex != -1 && curIndex != -1)
                    {
                        if (startIndex < curIndex)
                        {
                            for (int i = startIndex; i <= curIndex; i++)
                            {
                                bookmark.RootTree.BookmarkItems[i].IsSelected = true;
                            }
                        }
                        else
                        {
                            for (int i = curIndex; i <= startIndex; i++)
                            {
                                bookmark.RootTree.BookmarkItems[i].IsSelected = true;
                            }
                        }
                    }
                }
                else
                {
                    startIndex = parent.BookmarkItems.IndexOf(bookmark.RootTree.StartItem);
                    curIndex = parent.BookmarkItems.IndexOf(bookmark);
                    if (startIndex != -1 && curIndex != -1)
                    {
                        if (startIndex < curIndex)
                        {
                            for (int i = startIndex; i <= curIndex; i++)
                            {
                                parent.BookmarkItems[i].IsSelected = true;
                            }
                        }
                        else
                        {
                            for (int i = curIndex; i <= startIndex; i++)
                            {
                                parent.BookmarkItems[i].IsSelected = true;
                            }
                        }
                    }
                    else
                    {
                        int startItemRootIndex = bookmark.RootTree.GetRootIndex(bookmark.RootTree.StartItem);
                        int curItemRootIndex = bookmark.RootTree.GetRootIndex(bookmark);
                        if (startItemRootIndex == -1 || curItemRootIndex == -1)
                        {
                            return;
                        }

                        if (startItemRootIndex < curItemRootIndex)
                        {
                            for (int i = startIndex; i < parent.BookmarkItems.Count; i++)
                            {
                                parent.BookmarkItems[i].IsSelected = true;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= startIndex; i++)
                            {
                                parent.BookmarkItems[i].IsSelected = true;
                            }
                        }
                    }
                }
            }
        }

        private void ItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _isRightMouseDown = false;
        }
    }

    class BookmarkTreeViewItem : ObservableObject
    {
        private ObservableCollection<BookmarkTreeViewItem> _bookmarkItems;
        private RootBookmarkTree _rootTree;
        private OutlineItemCollection _outlineItem;
        private BookmarkLocationInfo _bookmarkLocationInfo;
        private BookmarkTreeViewItem _parent;
        private bool _isSelected = false;
        private BookmarkItemControl _control;
        private Font _titleFont;

        public BookmarkTreeViewItem(RootBookmarkTree rootTree, OutlineItemCollection outlineItem)
        {
            _rootTree = rootTree;
            _outlineItem = outlineItem;
            const int defaultFontSize = 10;
            _titleFont = new Font(new FontFamily("Segoe UI"), defaultFontSize);
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

        public BookmarkItemControl Control
        {
            get
            {
                return _control;
            }
            set
            {
                if (_control != value)
                {
                    _control = value;
                }
            }
        }

        public RootBookmarkTree RootTree
        {
            get { return _rootTree; }
            set
            {
                if (_rootTree != value)
                {
                    _rootTree = value;
                }
            }
        }

        public OutlineItemCollection OutlineItem
        {
            get { return _outlineItem; }
            set
            {
                if (_outlineItem != value)
                {
                    _outlineItem = value;
                }
            }
        }

        public BookmarkLocationInfo BookmarkLocationInfo
        {
            get
            {
                if (!_bookmarkLocationInfo.HasValue())
                {
                    _bookmarkLocationInfo = RootTree.TryGetBookmarkLocationInfo(this);
                }

                return _bookmarkLocationInfo;
            }
            set
            {
                if (_bookmarkLocationInfo != value)
                {
                    _bookmarkLocationInfo = value;
                }
            }
        }

        public bool IsBookmarkHasChild
        {
            get
            {
                return BookmarkItems.Count != 0;
            }
        }

        public int Level
        {
            get { return OutlineItem.Level; }
        }

        public string Title
        {
            get
            {
                return OutlineItem.Title;
            }
            set
            {
                if (OutlineItem.Title != value)
                {
                    OutlineItem.Title = value;
                    Notify(nameof(Title));
                    Notify(nameof(SelectedBorderWidth));
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    Notify(nameof(IsSelected));
                    if (RootTree != null)
                    {
                        RootTree.RefreshSelectedList(this);

                        if (!KeyboardUtil.IsCtrlKeyDown && !KeyboardUtil.IsShiftKeyDown && _isSelected && Control != null && Control.IsMouseDown && !Control.IsRightMouseDown)
                        {
                            RootTree.SelectThumbnailByPage(BookmarkLocationInfo.PageIndex);
                        }
                    }

                    if (_control != null)
                    {
                        _control.RefreshTreeViewSelected(value);
                    }
                }
            }
        }

        public double SelectedBorderWidth
        {
            get 
            {
                var size = System.Windows.Forms.TextRenderer.MeasureText(Title, _titleFont);
                return size.Width;
            }
        }

        public BookmarkTreeViewItem Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    Notify(nameof(Parent));
                }
            }
        }



        public void RefreshBookmarkChildState(bool isForceExpand = false)
        {
            if (isForceExpand)
            {
                Control.ExpandTreeViewItem(true);
            }

            Notify(nameof(IsBookmarkHasChild));
        }
    }

    public struct BookmarkLocationInfo
    {
        public int PageIndex;
        public double Left;
        public double Right;
        public double Top;
        public double Bottom;
        public string Name;

        public BookmarkLocationInfo(BookmarkLocationInfo bookmarkLocationInfo)
        {
            PageIndex = bookmarkLocationInfo.PageIndex;
            Left = bookmarkLocationInfo.Left;
            Right = bookmarkLocationInfo.Right;
            Top = bookmarkLocationInfo.Top;
            Bottom = bookmarkLocationInfo.Bottom;
            Name = bookmarkLocationInfo.Name;
        }

        public BookmarkLocationInfo(int pageIndex, double left, double right, double top, double bottom, string name)
        {
            PageIndex = pageIndex;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            Name = name;
        }

        public static bool operator ==(BookmarkLocationInfo left, BookmarkLocationInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BookmarkLocationInfo left, BookmarkLocationInfo right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool HasValue()
        {
            return PageIndex != 0;
        }
    }
}
