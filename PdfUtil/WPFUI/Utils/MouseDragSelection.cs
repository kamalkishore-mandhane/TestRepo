using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PdfUtil.WPFUI.Utils
{
    internal sealed class PrepareDragSelection
    {
        private readonly ListView _listBox;
        private readonly ScrollContentPresenter _scrollContent;

        private int _lastClickTickCount;

        public bool MouseCaptured
        {
            get;
            set;
        }

        public bool IsMoved
        {
            get;
            set;
        }

        public bool CanMouseDragSelection
        {
            get;
            set;
        }

        public ListViewItem HitItem
        {
            get;
            set;
        }

        public bool IsNoModifiersKeyPressed
        {
            get
            {
                return ((Keyboard.Modifiers & ModifierKeys.Control) == 0) && ((Keyboard.Modifiers & ModifierKeys.Shift) == 0);
            }
        }

        public PrepareDragSelection(ListView listBox, ScrollContentPresenter scrollContent)
        {
            _listBox = listBox;
            _scrollContent = scrollContent;
            _lastClickTickCount = Environment.TickCount - 2 * System.Windows.Forms.SystemInformation.DoubleClickTime;
        }

        public bool MayDoubleClick(MouseButtonEventArgs e)
        {
            if (Environment.TickCount - _lastClickTickCount <= System.Windows.Forms.SystemInformation.DoubleClickTime && e.ClickCount > 1)
            {
                _lastClickTickCount = Environment.TickCount - 2 * System.Windows.Forms.SystemInformation.DoubleClickTime;
                return true;
            }
            _lastClickTickCount = Environment.TickCount;
            return false;
        }

        public void Reset(UIElement element)
        {
            if (element == null && !IsMoved)
            {
                _listBox.SelectedItems.Clear();
            }
            else if (HitItem != null && !IsMoved && IsNoModifiersKeyPressed)
            {
                //var listbox = _listBox as FilePaneListView;
                //listbox.NeedFireSelectedChangedEvent = false;
                _listBox.UnselectAll();
                //listbox.NeedFireSelectedChangedEvent = true;

                HitItem.IsSelected = true;
                HitItem.Focus();
            }

            HitItem = null;
            IsMoved = false;
            MouseCaptured = false;
        }

        public void HitTest(Point mouse)
        {
            CanMouseDragSelection = true;

            VisualTreeHelper.HitTest(_scrollContent, new HitTestFilterCallback(HitTestFilter),
                  new HitTestResultCallback(HitTestResult), new PointHitTestParameters(mouse));
        }

        private HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            return HitTestResultBehavior.Continue;
        }

        private HitTestFilterBehavior HitTestFilter(DependencyObject o)
        {
            if (o is ListViewItem)
            {
                HitItem = o as ListViewItem;

                if (HitItem.IsSelected)
                {
                    CanMouseDragSelection = false;
                    return HitTestFilterBehavior.Stop;
                }
            }
            else if (o is TextBlock || o is TextBox || o is CheckBox || o is Image ||
                 o is System.Windows.Shapes.Ellipse || o is ProgressBar)
            {
                if ((o as FrameworkElement).Visibility != Visibility.Visible)
                {
                    CanMouseDragSelection = true;
                }
                else
                {
                    CanMouseDragSelection = false;
                }
            }

            return HitTestFilterBehavior.Continue;
        }
    }

    public sealed class ListViewSelector
    {
        private readonly ListView _listBox;

        private PrepareDragSelection _prepareDragSelection;
        private ScrollContentPresenter _scrollContent;
        private SelectionAdorner _selectionRect;
        private AutoScroller _autoScroller;
        private ItemsControlSelector _selector;
        private ScrollViewer _scrollViewer;

        private int _startIndex;
        private double _itemHeight;
        private Point _end;
        private Point _start;

        public int StartIndex
        {
            get { return _startIndex; }
        }

        public Point End
        {
            get { return this._end; }
        }

        public double ItemHeight
        {
            get { return this._itemHeight; }
        }

        public ListViewSelector(ListView listBox)
        {
            _listBox = listBox;
            if (_listBox.IsLoaded)
            {
                Init();
            }
            else
            {
                _listBox.Loaded += OnListBoxLoaded;
            }
        }

        private bool Init()
        {
            _scrollViewer = VisualTreeHelperUtils.FindVisualChild<ScrollViewer>(_listBox);
            // Because in the GridViewScrollViewTemplate, there are two ScrollConentPresenters: GridViewHeaderRowPresenter & ScrollContentPresenter,
            // and if use VisualTreeHelperUtils.FindVisualChild<ScrollContentPresenter>(_listBox), it will find GridViewHeaderRowPresenter, not ScrollContentPresenter,
            // and DouleClick Item will not work normally,
            // so we use _scrollViewer.Template.FindName("PART_ScrollContentPresenter", _scrollViewer) to find its owen ScrollContentPresenter.
            if (_scrollViewer != null)
            {
                _scrollContent = _scrollViewer.Template.FindName("PART_ScrollContentPresenter", _scrollViewer) as ScrollContentPresenter;
                if (_scrollContent != null)
                {
                    _autoScroller = new AutoScroller(_listBox, this);
                    _autoScroller.OffsetChanged += OnOffsetChanged;

                    _selectionRect = new SelectionAdorner(_scrollContent);
                    _scrollContent.AdornerLayer.Add(_selectionRect);

                    _selector = new ItemsControlSelector(_listBox, this);
                    _prepareDragSelection = new PrepareDragSelection(_listBox, _scrollContent);

                    _scrollViewer.PreviewMouseDown += OnPreviewMouseDown;
                    _scrollViewer.LostMouseCapture += OnLostMouseCapture;
                }
            }

            return _scrollContent != null;
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_prepareDragSelection.MouseCaptured)
            {
                UIElement element = _scrollContent.InputHitTest(_end) as UIElement;
                _prepareDragSelection.Reset(element);

                _autoScroller.UnRegister();
                StopSelection();
            }
        }

        private void SetMouseDragSelection(bool canSelection)
        {
            if (canSelection)
            {
                _listBox.MouseUp += OnMouseUp;
                _listBox.MouseMove += OnMouseMove;

                _autoScroller.Register();
            }
            else
            {
                _listBox.MouseUp -= OnMouseUp;
                _listBox.MouseMove -= OnMouseMove;

                _autoScroller.UnRegister();
            }
        }

        private void OnListBoxLoaded(object sender, EventArgs e)
        {
            if (Init())
            {
                _listBox.Loaded -= OnListBoxLoaded;
            }
        }

        private void OnOffsetChanged(object sender, OffsetChangedEventArgs e)
        {
            _selector.Scroll(e.HorizontalChange, e.VerticalChange);

            UpdateSelection();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_prepareDragSelection.MouseCaptured)
            {
                _end = e.GetPosition(_scrollContent);
                Mouse.Capture(null); // Will Call OnLostMouseCapture
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _end = e.GetPosition(_scrollContent);
            if (_prepareDragSelection.IsMoved)
            {
                _autoScroller.Update(_end);
                UpdateSelection();
            }
            else
            {
                Vector diff = _start - _end;
                if (_prepareDragSelection.MouseCaptured && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _prepareDragSelection.IsMoved = true;
                    StartSelection();
                }
            }
        }

        /// <summary>
        /// Check that the mouse is inside the scroll content (could be on the scroll bars).
        /// And Check need mouse box selection or not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Because ScrollContentPresenter is covered by ScrollBar in the FlatScrollViewer,
            // so when click ScrollBar, it will select the below item on it,
            // and we will ignore the mouse click from ScrollBar.
            ScrollBar scrollbar = VisualTreeHelperUtils.FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject);
            if (scrollbar != null)
            {
                return;
            }

            Point mouse = e.GetPosition(_scrollContent);
            if ((mouse.X >= 0) && (mouse.X < _scrollContent.ActualWidth) &&
                (mouse.Y >= 0) && (mouse.Y < _scrollContent.ActualHeight))
            {
                _prepareDragSelection.HitTest(mouse);
                if (_prepareDragSelection.MayDoubleClick(e))
                {
                    return;
                }

                //stylus drag selection not supported.
                if (e.StylusDevice != null)
                {
                    return;
                }

                if (Mouse.MiddleButton != MouseButtonState.Pressed && _prepareDragSelection.CanMouseDragSelection)
                {
                    SetMouseDragSelection(true);
                    _prepareDragSelection.MouseCaptured = Mouse.Capture(_scrollContent, CaptureMode.Element);

                    if (_prepareDragSelection.MouseCaptured)
                    {
                        SetStartIndex();
                        SetItemHeight();
                        _scrollViewer.Focus();
                        _start = mouse;
                        _end = mouse;
                        e.Handled = _prepareDragSelection.IsNoModifiersKeyPressed;
                    }
                }
                else
                {
                    SetMouseDragSelection(false);
                }
            }
        }

        private void SetStartIndex()
        {
            _startIndex = _listBox.Items.Count;
            if (_prepareDragSelection.HitItem != null)
            {
                for (int i = 0; i < _listBox.Items.Count; i++)
                {
                    if (_listBox.ItemContainerGenerator.IndexFromContainer(_prepareDragSelection.HitItem) == i)
                    {
                        _startIndex = i;
                        break;
                    }
                }
            }
        }

        private void SetItemHeight()
        {
            for (int i = 0; i < _listBox.Items.Count; i++)
            {
                FrameworkElement item = _listBox.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (item != null)
                {
                    _itemHeight = item.ActualHeight;
                    return;
                }
            }
        }

        private void StopSelection()
        {
            _selectionRect.IsEnabled = false;
            _autoScroller.IsEnabled = false;
        }

        private void StartSelection()
        {
            if (_prepareDragSelection.IsNoModifiersKeyPressed)
            {
                //var listbox = _listBox as FilePaneListView;
                //listbox.NeedFireSelectedChangedEvent = false;
                _listBox.SelectedItems.Clear();
                //listbox.NeedFireSelectedChangedEvent = true;
            }

            _selector.Reset(_start, _end);
            UpdateSelection();

            _selectionRect.IsEnabled = true;
            _autoScroller.IsEnabled = true;
        }

        /// <summary>
        /// Draw the selecion rectangle.
        /// </summary>
        private void UpdateSelection()
        {
            Point start = _autoScroller.TranslatePoint(_start);

            double endY = this._end.Y;
            if (this._end.Y > this._scrollContent.ActualHeight)
            {
                endY = this._scrollContent.ActualHeight;
            }
            else if (this._end.Y < 0)
            {
                endY = -1;
            }

            double x = Math.Min(start.X, _end.X);
            double y = Math.Min(start.Y, endY);
            double width = Math.Abs(_end.X - start.X);
            double height = Math.Abs(endY - start.Y);
            Rect area = new Rect(x, y, width, height);
            _selectionRect.SelectionArea = area;

            Point topLeft = _scrollContent.TranslatePoint(area.TopLeft, _listBox);
            Point bottomRight = _scrollContent.TranslatePoint(area.BottomRight, _listBox);
            _selector.UpdateSelection(new Rect(topLeft, bottomRight));
        }

        /// <summary>
        /// Automatically scrolls an ItemsControl when the mouse is dragged outside
        /// of the control.
        /// </summary>
        private sealed class AutoScroller
        {
            private readonly DispatcherTimer _autoScroll = new DispatcherTimer();
            private readonly ItemsControl _itemsControl;
            private readonly ScrollViewer _scrollViewer;
            private readonly ScrollContentPresenter _scrollContent;
            private readonly ListViewSelector _listBoxSelector;
            private bool _isEnabled;
            private Point _offset;
            private Point _mouse;

            public AutoScroller(ItemsControl itemsControl, ListViewSelector listBoxSelector)
            {
                if (itemsControl == null)
                {
                    throw new ArgumentNullException("itemsControl");
                }
                _itemsControl = itemsControl;
                _scrollViewer = VisualTreeHelperUtils.FindVisualChild<ScrollViewer>(itemsControl);
                _scrollViewer.ScrollChanged += OnScrollChanged;
                _scrollContent = VisualTreeHelperUtils.FindVisualChild<ScrollContentPresenter>(_scrollViewer);

                _autoScroll.Tick += delegate { PreformScroll(); };
                _autoScroll.Interval = TimeSpan.FromMilliseconds(GetRepeatRate());
                _listBoxSelector = listBoxSelector;
            }

            public event EventHandler<OffsetChangedEventArgs> OffsetChanged;

            public bool IsEnabled
            {
                get
                {
                    return _isEnabled;
                }
                set
                {
                    if (_isEnabled != value)
                    {
                        _isEnabled = value;
                        _autoScroll.IsEnabled = false;
                        _offset = new Point();
                    }
                }
            }

            /// <summary>
            /// Translates the specified point by the current scroll offset.
            /// </summary>
            /// <param name="point">The point to translate.</param>
            /// <returns>A new point offset by the current scroll amount.</returns>
            public Point TranslatePoint(Point point)
            {
                return new Point(point.X - _offset.X, point.Y - _offset.Y);
            }

            public void UnRegister()
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
            }

            public void Register()
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }

            public void Update(Point mouse)
            {
                _mouse = mouse;

                if (!_autoScroll.IsEnabled)
                {
                    PreformScroll();
                }
            }

            /// <summary>
            /// Returns the default repeat rate in milliseconds.
            /// The RepeatButton uses the SystemParameters.KeyboardSpeed as the default value for the Interval property.
            /// KeyboardSpeed returns a value between 0 (400ms) and 31 (33ms).
            /// </summary>
            /// <returns></returns>
            private static int GetRepeatRate()
            {
                const double Ratio = (400.0 - 33.0) / 31.0;
                return 400 - (int)(SystemParameters.KeyboardSpeed * Ratio);
            }

            private double CalculateOffset(int startIndex, int endIndex)
            {
                return (endIndex - startIndex) * _listBoxSelector.ItemHeight;
            }

            private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                if (IsEnabled)
                {
                    double horizontal = e.HorizontalChange;
                    double vertical = e.VerticalChange;

                    if (_scrollViewer.CanContentScroll)
                    {
                        if (e.VerticalChange < 0)
                        {
                            int start = (int)e.VerticalOffset;
                            int end = (int)(e.VerticalOffset - e.VerticalChange);
                            vertical = -CalculateOffset(start, end);
                        }
                        else
                        {
                            int start = (int)(e.VerticalOffset - e.VerticalChange);
                            int end = (int)e.VerticalOffset;
                            vertical = CalculateOffset(start, end);
                        }
                    }

                    _offset.X += horizontal;
                    _offset.Y += vertical;

                    var callback = OffsetChanged;
                    if (callback != null)
                    {
                        callback(this, new OffsetChangedEventArgs(horizontal, vertical));
                    }
                }
            }

            private void PreformScroll()
            {
                bool scrolled = false;

                if (_mouse.X > _scrollContent.ActualWidth)
                {
                    _scrollViewer.LineRight();
                    scrolled = true;
                }
                else if (_mouse.X < 0)
                {
                    _scrollViewer.LineLeft();
                    scrolled = true;
                }

                if (_mouse.Y > _scrollContent.ActualHeight)
                {
                    _scrollViewer.LineDown();
                    scrolled = true;
                }
                else if (_mouse.Y < 0)
                {
                    _scrollViewer.LineUp();
                    scrolled = true;
                }

                _autoScroll.IsEnabled = scrolled;
            }
        }

        /// <summary>Enables the selection of items by a specified rectangle.</summary>
        private sealed class ItemsControlSelector
        {
            private readonly ItemsControl _itemsControl;
            private readonly ListViewSelector _listBoxSelector;

            private Rect _previousArea;

            public ItemsControlSelector(ItemsControl itemsControl, ListViewSelector listBoxSelector)
            {
                if (itemsControl == null)
                {
                    throw new ArgumentNullException("itemsControl");
                }
                _itemsControl = itemsControl;
                this._listBoxSelector = listBoxSelector;
            }

            public void Reset(Point start, Point end)
            {
                _previousArea = new Rect(start, end);
            }

            public void Scroll(double x, double y)
            {
                _previousArea.Offset(-x, -y);
            }

            /// <summary>
            /// Updates the controls selection based on the specified area.
            /// Check each item to see if it intersects with the area.
            /// </summary>
            /// <param name="area">
            /// The selection area, relative to the control passed in the contructor.
            /// </param>
            public void UpdateSelection(Rect area)
            {
                bool outsideDown = this._listBoxSelector.End.Y > this._listBoxSelector._scrollContent.ActualHeight;
                bool outsideUp = this._listBoxSelector.End.Y < 0;

                for (int i = 0; i < _itemsControl.Items.Count; i++)
                {
                    FrameworkElement item = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (item != null)
                    {
                        Point topLeft = item.TranslatePoint(new Point(0, 0), _itemsControl);
                        Rect itemBounds = new Rect(topLeft.X, topLeft.Y, item.ActualWidth, item.ActualHeight);
                        if (itemBounds.IntersectsWith(area))
                        {
                            if ((outsideDown && i < this._listBoxSelector.StartIndex)
                                || (outsideUp && i > this._listBoxSelector.StartIndex))
                            {
                                continue;
                            }
                            Selector.SetIsSelected(item, true);
                        }
                        else if (itemBounds.IntersectsWith(_previousArea))
                        {
                            Selector.SetIsSelected(item, false);
                        }
                        else if ((outsideDown && i < this._listBoxSelector.StartIndex)
                            || (outsideUp && i > this._listBoxSelector.StartIndex))
                        {
                            Selector.SetIsSelected(item, false);
                        }
                    }
                }
                _previousArea = area;
            }
        }

        private sealed class OffsetChangedEventArgs : EventArgs
        {
            private readonly double _horizontal;
            private readonly double _vertical;

            internal OffsetChangedEventArgs(double horizontal, double vertical)
            {
                _horizontal = horizontal;
                _vertical = vertical;
            }

            public double HorizontalChange
            {
                get { return _horizontal; }
            }

            public double VerticalChange
            {
                get { return _vertical; }
            }
        }

        /// <summary>Draws a selection rectangle on an AdornerLayer.</summary>
        private sealed class SelectionAdorner : Adorner
        {
            private Rect _selectionRect;

            public SelectionAdorner(UIElement parent)
                : base(parent)
            {
                IsHitTestVisible = false;
                IsEnabledChanged += delegate { InvalidateVisual(); };
            }

            public Rect SelectionArea
            {
                get
                {
                    return _selectionRect;
                }
                set
                {
                    _selectionRect = value;
                    InvalidateVisual();
                }
            }

            /// <summary>
            /// Participates in rendering operations that are directed by the layout system.
            /// </summary>
            /// <param name="drawingContext">The drawing instructions.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (IsEnabled)
                {
                    double[] x = { SelectionArea.Left + 0.5, SelectionArea.Right + 0.5 };
                    double[] y = { SelectionArea.Top + 0.5, SelectionArea.Bottom + 0.5 };
                    drawingContext.PushGuidelineSet(new GuidelineSet(x, y));

                    Brush fill = SystemColors.HighlightBrush.Clone();
                    fill.Opacity = 0.4;
                    drawingContext.DrawRectangle(fill, new Pen(SystemColors.HighlightBrush, 1.0), SelectionArea);
                }
            }
        }
    }
}