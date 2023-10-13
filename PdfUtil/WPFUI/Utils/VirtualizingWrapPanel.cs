using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PdfUtil.WPFUI.Utils
{
    /// <summary>
    /// http://www.codeproject.com/Articles/75847/Virtualizing-WrapPanel
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {

        #region Fields

        UIElementCollection _children;
        ItemsControl _itemsControl;
        IItemContainerGenerator _generator;
        private Point _offset = new Point(0, 0);
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Size _viewsize = new Size(0, 0);
        private int _visibleSections = 0;
        private int firstIndex = 0;
        private Size childSize;
        private Size _pixelMeasuredViewport = new Size(0, 0);
        Dictionary<UIElement, Rect> _realizedChildLayout = new Dictionary<UIElement, Rect>();
        WrapPanelAbstraction _abstractPanel;

        private const double _scrollLineDelta = 16.0;

        #endregion

        #region Properties

        private Size ChildSlotSize
        {
            get
            {
                return new Size(ItemWidth, ItemHeight);
            }
        }

        #endregion

        #region Dependency Properties

        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get
            {
                return (double)base.GetValue(ItemHeightProperty);
            }
            set
            {
                base.SetValue(ItemHeightProperty, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get
            {
                return (double)base.GetValue(ItemWidthProperty);
            }
            set
            {
                base.SetValue(ItemWidthProperty, value);
            }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.PositiveInfinity));
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.PositiveInfinity));
        public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal));

        #endregion

        #region Methods

        public void SetFirstRowViewItemIndex(int index)
        {
            SetVerticalOffset((index) / Math.Max(1, Math.Floor((_viewsize.Width) / childSize.Width)));
            SetHorizontalOffset((index) / Math.Max(1, Math.Floor((_viewsize.Height) / childSize.Height)));
        }

        private void Resizing(object sender, EventArgs e)
        {
            _visibleSections = 0;

            if (_viewsize.Width != 0)
            {
                int firstIndexCache = firstIndex;
                _abstractPanel = null;
                MeasureOverride(_viewsize);
                SetFirstRowViewItemIndex(firstIndex);
                firstIndex = firstIndexCache;
            }
        }

        public int GetFirstVisibleSection()
        {
            int section;
            if (_abstractPanel == null)
            {
                return 0;
            }
            var maxSection = _abstractPanel.Max(x => x.Section);
            if (Orientation == Orientation.Horizontal)
            {
                section = (int)_offset.Y;
            }
            else
            {
                section = (int)_offset.X;
            }
            if (section > maxSection)
                section = maxSection;
            return section;
        }

        public int GetFirstVisibleIndex()
        {
            int section = GetFirstVisibleSection();
            if (_abstractPanel == null)
            {
                return 0;
            }
            var item = _abstractPanel.Where(x => x.Section == section).FirstOrDefault();
            if (item != null)
                return item._index;
            return 0;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = _generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated)
                {
                    if (itemIndex >= 0)
                        _generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private void ComputeExtentAndViewport(Size pixelMeasuredViewportSize, int visibleSections)
        {
            _visibleSections = visibleSections;

            if (Orientation == Orientation.Horizontal)
            {
                _viewport.Height = _visibleSections;
                _viewport.Width = pixelMeasuredViewportSize.Width;

                _viewsize.Height = pixelMeasuredViewportSize.Height;
                _viewsize.Width = pixelMeasuredViewportSize.Width;
            }
            else
            {
                _viewport.Width = _visibleSections;
                _viewport.Height = pixelMeasuredViewportSize.Height;

                _viewsize.Height = pixelMeasuredViewportSize.Height;
                _viewsize.Width = pixelMeasuredViewportSize.Width;
            }

            if (Orientation == Orientation.Horizontal)
            {
                if (_abstractPanel.SectionCount <= ViewportHeight)
                {
                    _extent.Height = _abstractPanel.SectionCount;
                    if (_extent.Height * childSize.Height > pixelMeasuredViewportSize.Height)
                    {
                        _extent.Height += 1;
                    }
                }
                else
                {
                    _extent.Height = _abstractPanel.SectionCount + 1;
                }
            }
            else
            {
                if (_abstractPanel.SectionCount <= ViewportWidth)
                {
                    _extent.Width = _abstractPanel.SectionCount;
                    if (_extent.Width * childSize.Width > pixelMeasuredViewportSize.Width)
                    {
                        _extent.Width += 1;
                    }
                }
                else
                {
                    _extent.Width = _abstractPanel.SectionCount + 1;
                }
            }
            _owner.InvalidateScrollInfo();
        }

        private void ResetScrollInfo()
        {
            _offset.X = 0;
            _offset.Y = 0;
        }

        private int GetNextSectionClosestIndex(int itemIndex)
        {
            int itemRow = (itemIndex + 1) / _abstractPanel._averageItemsPerSection;
            if ((itemIndex + 1) % _abstractPanel._averageItemsPerSection > 0)
            {
                itemRow++;
            }

            int row = _abstractPanel._itemCount / _abstractPanel._averageItemsPerSection;
            if (_abstractPanel._itemCount % _abstractPanel._averageItemsPerSection > 0)
            {
                row++;
            }

            if (itemRow == row)
            {
                return itemIndex;
            }
            return Math.Min(itemIndex + _abstractPanel._averageItemsPerSection, _abstractPanel._itemCount - 1);
        }

        private int GetLastSectionClosestIndex(int itemIndex)
        {
            var index = itemIndex - _abstractPanel._averageItemsPerSection;
            if (index < 0)
            {
                return itemIndex;
            }
            return index;
        }

        private void NavigateDown()
        {
            var gen = _generator.GetItemContainerGeneratorForPanel(this);
            UIElement selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }
            DependencyObject next = null;
            if (Orientation == Orientation.Horizontal)
            {
                int nextIndex = GetNextSectionClosestIndex(itemIndex);
                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == _abstractPanel._itemCount - 1)
                    return;
                next = gen.ContainerFromIndex(itemIndex + 1);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex + 1);
                }
            }
            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }
            (next as UIElement).Focus();
        }

        private void NavigateLeft()
        {
            var gen = _generator.GetItemContainerGeneratorForPanel(this);

            UIElement selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }
            DependencyObject next = null;
            if (Orientation == Orientation.Vertical)
            {
                int nextIndex = GetLastSectionClosestIndex(itemIndex);
                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == 0)
                    return;
                next = gen.ContainerFromIndex(itemIndex - 1);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex - 1);
                }
            }
            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }
            (next as UIElement).Focus();
        }

        private void NavigateRight()
        {
            var gen = _generator.GetItemContainerGeneratorForPanel(this);
            UIElement selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }
            DependencyObject next = null;
            if (Orientation == Orientation.Vertical)
            {
                int nextIndex = GetNextSectionClosestIndex(itemIndex);
                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == _abstractPanel._itemCount - 1)
                    return;
                next = gen.ContainerFromIndex(itemIndex + 1);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex + 1);
                }
            }
            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }
            (next as UIElement).Focus();
        }

        private void NavigateUp()
        {
            var gen = _generator.GetItemContainerGeneratorForPanel(this);
            UIElement selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }
            DependencyObject next = null;
            if (Orientation == Orientation.Horizontal)
            {
                int nextIndex = GetLastSectionClosestIndex(itemIndex);
                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == 0)
                    return;
                next = gen.ContainerFromIndex(itemIndex - 1);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex - 1);
                }
            }
            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }
            (next as UIElement).Focus();
        }


        #endregion

        #region Override

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    NavigateDown();
                    e.Handled = true;
                    break;
                case Key.Left:
                    NavigateLeft();
                    e.Handled = true;
                    break;
                case Key.Right:
                    NavigateRight();
                    e.Handled = true;
                    break;
                case Key.Up:
                    NavigateUp();
                    e.Handled = true;
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            _abstractPanel = null;
            if (args.Action != NotifyCollectionChangedAction.Add)
            {
                ResetScrollInfo();
            }

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            this.SizeChanged += new SizeChangedEventHandler(this.Resizing);
            base.OnInitialized(e);
            _itemsControl = ItemsControl.GetItemsOwner(this);
            _children = InternalChildren;
            _generator = ItemContainerGenerator;
        }

        protected void SetAverageItemsPerSection(Size availableSize)
        {
            Size realizedFrameSize = availableSize;

            int itemCount = _itemsControl.Items.Count;

            int firstVisibleIndex = firstIndex;
            GeneratorPosition startPos = _generator.GeneratorPositionFromIndex(firstVisibleIndex);

            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            int current = firstVisibleIndex;

            using (_generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                bool isHorizontal = Orientation == Orientation.Horizontal;
                double maxItemSizeHeight = 0;
                double maxItemSizeWidth = 0;
                int sectionNum = 1;
                while (current < itemCount)
                {
                    bool newlyRealized;

                    // Get or create the child
                    UIElement child = _generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= _children.Count)
                        {
                            base.AddInternalChild(child);
                        }
                        else
                        {
                            base.InsertInternalChild(childIndex, child);
                        }
                        _generator.PrepareItemContainer(child);
                        child.Measure(ChildSlotSize);
                    }
                    else
                    {
                        // The child has already been created, let's be sure it's in the right spot
                        Debug.Assert(child == _children[childIndex], "Wrong child was generated");
                    }

                    current++;
                    childIndex++;
                    if (sectionNum * maxItemSizeHeight >= realizedFrameSize.Height)
                    {
                        break;
                    }
                    if (maxItemSizeWidth * (current - firstVisibleIndex) >= realizedFrameSize.Width)
                    {
                        sectionNum++;
                    }
                    var width = child.DesiredSize.Width;
                    maxItemSizeWidth = Math.Max(maxItemSizeWidth, width);
                    maxItemSizeHeight = Math.Max(maxItemSizeHeight, child.DesiredSize.Height);
                }

                childSize = new Size(maxItemSizeWidth, maxItemSizeHeight);
                _abstractPanel._averageItemsPerSection = Math.Max(1, (int)(realizedFrameSize.Width / maxItemSizeWidth));
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_itemsControl == null || _itemsControl.Items.Count == 0)
                return availableSize;

            if (_abstractPanel == null)
            {
                _abstractPanel = new WrapPanelAbstraction(_itemsControl.Items.Count);
            }

            _pixelMeasuredViewport = availableSize;

            _realizedChildLayout.Clear();
            SetAverageItemsPerSection(availableSize);

            Size realizedFrameSize = availableSize;

            int itemCount = _itemsControl.Items.Count;

            int firstVisibleIndex = GetFirstVisibleIndex();
            firstIndex = firstVisibleIndex;
            GeneratorPosition startPos = _generator.GeneratorPositionFromIndex(firstVisibleIndex);

            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            int current = firstVisibleIndex;
            int visibleSections = 1;

            using (_generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                bool stop = false;
                bool isHorizontal = Orientation == Orientation.Horizontal;
                double currentX = 0;
                double currentY = 0;
                double interval = 0;
                int currentSection = GetFirstVisibleSection();
                while (current < itemCount)
                {
                    UIElement child;
                    bool newlyRealized;

                    // Get or create the child
                    child = _generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= _children.Count)
                        {
                            base.AddInternalChild(child);
                        }
                        else
                        {
                            base.InsertInternalChild(childIndex, child);
                        }
                        _generator.PrepareItemContainer(child);
                        child.Measure(ChildSlotSize);
                    }
                    else
                    {
                        // The child has already been created, let's be sure it's in the right spot
                        Debug.Assert(child == _children[childIndex], "Wrong child was generated");
                    }

                    var multiple = realizedFrameSize.Width / childSize.Width;
                    if (current == firstVisibleIndex && this.HorizontalAlignment == HorizontalAlignment.Center)
                    {
                        if (realizedFrameSize.Width >= childSize.Width)
                        {
                            if (multiple > itemCount)
                            {
                                currentX = ((multiple - (itemCount)) * childSize.Width) / (itemCount + 1);
                                interval = currentX;
                            }
                            else
                            {
                                currentX = ((multiple - _abstractPanel._averageItemsPerSection) * childSize.Width) / ((int)multiple + 1);
                                interval = currentX;
                            }
                        }
                        else
                        {
                            currentX = (realizedFrameSize.Width - childSize.Width) / 2;
                        }
                    }
                    Rect childRect = new Rect(new Point(currentX, currentY), childSize);
                    if (isHorizontal)
                    {
                        if (childRect.Right > realizedFrameSize.Width) //wrap to a new line
                        {
                            if (current != firstVisibleIndex)
                            {
                                currentY = currentY + childSize.Height;
                            }
                            currentX = 0;
                            if (this.HorizontalAlignment == HorizontalAlignment.Center)
                            {
                                if (realizedFrameSize.Width >= childSize.Width)
                                {
                                     currentX = interval;
                                }
                                else
                                {
                                    currentX = (realizedFrameSize.Width - childSize.Width) / 2;
                                }
                            }
                            childRect.X = currentX;
                            childRect.Y = currentY;
                            currentSection++;

                            if (currentY > realizedFrameSize.Height)
                            {
                                stop = true;
                            }
                            else
                            {
                                visibleSections++;
                            }
                        }

                        currentX = childRect.Right + interval;
                    }

                    _realizedChildLayout.Add(child, childRect);
                    _abstractPanel[current].Section = _abstractPanel[current].Section;
                    _abstractPanel[current].SectionIndex = _abstractPanel[current].SectionIndex;


                    if (stop)
                        break;
                    current++;
                    childIndex++;
                }
            }
            CleanUpItems(firstVisibleIndex, current - 1);

            ComputeExtentAndViewport(availableSize, visibleSections);

            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_children != null)
            {
                foreach (UIElement child in _children)
                {
                    if (_realizedChildLayout.ContainsKey(child))
                    {
                        var layoutInfo = _realizedChildLayout[child];
                        child.Arrange(layoutInfo);
                    }
                }
            }
            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            _abstractPanel = null;
            SetFirstRowViewItemIndex(index);
            MeasureOverride(_viewsize);
            firstIndex = index;
        }

        #endregion

        #region IScrollInfo Members

        private bool _canHScroll = false;
        public bool CanHorizontallyScroll
        {
            get { return _canHScroll; }
            set { _canHScroll = value; }
        }

        private bool _canVScroll = false;
        public bool CanVerticallyScroll
        {
            get { return _canVScroll; }
            set { _canVScroll = value; }
        }

        public double ExtentHeight
        {
            get
            {
                if (_abstractPanel == null)
                {
                    _extent.Height = 0.0;
                }
                return _extent.Height;
            }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public void LineDown()
        {
            if (Orientation == Orientation.Vertical)
                SetVerticalOffset(VerticalOffset + _scrollLineDelta);
            else
                SetVerticalOffset(VerticalOffset + 1);
        }

        public void LineLeft()
        {
            if (Orientation == Orientation.Horizontal)
                SetHorizontalOffset(HorizontalOffset - _scrollLineDelta);
            else
                SetHorizontalOffset(HorizontalOffset - 1);
        }

        public void LineRight()
        {
            if (Orientation == Orientation.Horizontal)
                SetHorizontalOffset(HorizontalOffset + _scrollLineDelta);
            else
                SetHorizontalOffset(HorizontalOffset + 1);
        }

        public void LineUp()
        {
            if (Orientation == Orientation.Vertical)
                SetVerticalOffset(VerticalOffset - _scrollLineDelta);
            else
                SetVerticalOffset(VerticalOffset - 1);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var gen = (ItemContainerGenerator)_generator.GetItemContainerGeneratorForPanel(this);
            var element = (UIElement)visual;
            int itemIndex = gen.IndexFromContainer(element);
            while (itemIndex == -1)
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
                itemIndex = gen.IndexFromContainer(element);
            }
            Rect elementRect = _realizedChildLayout[element];
            if (Orientation == Orientation.Horizontal)
            {
                double viewportHeight = _pixelMeasuredViewport.Height;
                if (elementRect.Bottom > viewportHeight)
                    _offset.Y += 1;
                else if (elementRect.Top < 0)
                    _offset.Y -= 1;
            }
            else
            {
                double viewportWidth = _pixelMeasuredViewport.Width;
                if (elementRect.Right > viewportWidth)
                    _offset.X += 1;
                else if (elementRect.Left < 0)
                    _offset.X -= 1;
            }
            InvalidateMeasure();
            return elementRect;
        }

        public void MouseWheelDown()
        {
            PageDown();
        }

        public void MouseWheelLeft()
        {
            PageLeft();
        }

        public void MouseWheelRight()
        {
            PageRight();
        }

        public void MouseWheelUp()
        {
            PageUp();
        }

        public void PageDown()
        {
            _abstractPanel?.Correct();
            SetVerticalOffset(VerticalOffset + _viewport.Height * 0.8);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - _viewport.Width * 0.8);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + _viewport.Width * 0.8);
        }

        public void PageUp()
        {
            _abstractPanel?.Correct();
            SetVerticalOffset(VerticalOffset - _viewport.Height * 0.8);
        }

        private ScrollViewer _owner;
        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || _viewport.Width >= _extent.Width)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Width >= _extent.Width)
                {
                    offset = _extent.Width - _viewport.Width;
                }
            }

            _offset.X = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            InvalidateMeasure();
            firstIndex = GetFirstVisibleIndex();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }

            _offset.Y = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            //_trans.Y = -offset;

            InvalidateMeasure();
            firstIndex = GetFirstVisibleIndex();
        }

        public double ViewportHeight
        {
            get
            {
                if (_abstractPanel == null)
                {
                    _viewport.Height = 0.0;
                }
                return _viewport.Height;
            }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        #endregion

        #region helper data structures

        class ItemAbstraction
        {
            public ItemAbstraction(WrapPanelAbstraction panel, int index)
            {
                _panel = panel;
                _index = index;
            }

            WrapPanelAbstraction _panel;

            public readonly int _index;

            public void Correct()
            {
                _sectionIndex = -1;
                SectionIndex = SectionIndex;
                _section = -1;
                Section = Section;
            }

            int _sectionIndex = -1;
            public int SectionIndex
            {
                get
                {
                    if (_sectionIndex == -1)
                    {
                        if ((_index + 1) % _panel._averageItemsPerSection > 0)
                        {
                            return (_index + 1) % _panel._averageItemsPerSection - 1;
                        }
                        return _panel._averageItemsPerSection - 1;
                    }
                    return _sectionIndex;
                }
                set
                {
                    if (_sectionIndex == -1)
                        _sectionIndex = value;
                }
            }

            int _section = -1;
            public int Section
            {
                get
                {
                    if (_section == -1)
                    {
                        return _index / _panel._averageItemsPerSection;
                    }
                    else
                    {
                        return _section;
                    }
                }
                set
                {
                    if (_section == -1)
                        _section = value;
                }
            }
        }

        class WrapPanelAbstraction : IEnumerable<ItemAbstraction>
        {
            public WrapPanelAbstraction(int itemCount)
            {
                List<ItemAbstraction> items = new List<ItemAbstraction>(itemCount);
                for (int i = 0; i < itemCount; i++)
                {
                    ItemAbstraction item = new ItemAbstraction(this, i);
                    items.Add(item);
                }

                Items = new ReadOnlyCollection<ItemAbstraction>(items);
                _averageItemsPerSection = _averageItemsPerSectionDefault;
                _itemCount = itemCount;
            }

            public readonly int _itemCount;
            public int _averageItemsPerSection;

            // The initial setting of VirtualizingWrapPanel is that as long as the Items change,
            // it will scroll to the starting position. But our requirement is that when an item is added,
            // the page should keep the current position without scrolling.
            //
            // The original value of _averageItemsPerSection is itemCount, but this will cause an error
            // in the first calculation of Section after the WrapPanelAbstraction instance created.
            // Therefore, the requirement of keeping the current position of the page without scrolling
            // after adding an item cannot be guaranteed, so the default value of averageItemsPerSection
            // needs to be set to 1.
            private const int _averageItemsPerSectionDefault = 1;

            public int SectionCount
            {
                get
                {
                    int ret = Items.Count / _averageItemsPerSection;
                    if (Items.Count % _averageItemsPerSection != 0)
                    {
                        ret += 1;
                    }
                    return ret;
                }
            }

            private ReadOnlyCollection<ItemAbstraction> Items { get; set; }

            public ItemAbstraction this[int index]
            {
                get { return Items[index]; }
            }

            #region IEnumerable<ItemAbstraction> Members

            public IEnumerator<ItemAbstraction> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            public void Correct()
            {
                foreach(var item in Items)
                {
                    item.Correct();
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}
