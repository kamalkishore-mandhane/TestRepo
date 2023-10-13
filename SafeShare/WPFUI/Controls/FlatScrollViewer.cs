using SafeShare.WPFUI.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SafeShare.WPFUI.Controls
{
    /// <summary>
    /// FlatScrollViewer
    /// </summary>
    public class FlatScrollViewer : ScrollViewer
    {
        private static DependencyProperty IsSlideScrollProperty = DependencyProperty.Register(
            "IsSlideScroll", typeof(bool), typeof(FlatScrollViewer), new PropertyMetadata(true));

        private const double _animateTime = 0.2; // 0.2s
        private const double _dispearTime = 300; // 300ms

        private Point _previousPos;
        private DispatcherTimer _mouseHoverDt;

        private DispatcherTimer _delayDispearDt;

        private bool _isCursorOnScrollBar;

        private bool _isCursorOnScrollRepeatButton;

        private bool _isScrollRepeatButtonExist;

        private bool _isFlatScrollBarExist;

        protected bool _isScrollBarAppear;

        protected bool _isScrollRepeatButtonAppear;

        public delegate void ScrollBarAppearChangedEventHandler(bool value);

        public event ScrollBarAppearChangedEventHandler ScrollBarAppearChanged;

        internal ISlideScrollBarNotifier _notifier;

        public bool IsSlideScroll
        {
            get { return (bool)this.GetValue(IsSlideScrollProperty); }
            protected set { this.SetValue(IsSlideScrollProperty, value); }
        }

        public bool IsScrollBarAppear
        {
            get { return _isScrollBarAppear; }
            protected set
            {
                if (_isScrollBarAppear != value)
                {
                    _isScrollBarAppear = value;
                    if (IsSlideScroll)
                    {
                        ScrollBarAppearChanged?.Invoke(value);
                    }
                }
            }
        }

        public bool IsScrollRepeatButtonAppear
        {
            get
            {
                return _isScrollRepeatButtonAppear;
            }
            protected set
            {
                if (_isScrollRepeatButtonAppear != value)
                {
                    _isScrollRepeatButtonAppear = value;
                }
            }
        }

        public void InitNotifier(ISlideScrollBarNotifier notifier)
        {
            this.Unloaded += FlatScrollViewer_Unloaded;
            _notifier = notifier;
            this.EnableSlide(true);
            _notifier.ModeChanged += ScrollBarModeChanged;
        }

        private void FlatScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            _notifier.ModeChanged -= ScrollBarModeChanged;
            this.Unloaded -= FlatScrollViewer_Unloaded;
        }

        protected void ScrollBarModeChanged(bool autoHide)
        {
            this.EnableSlide(autoHide);
        }

        protected ScrollBar VerticalScrollBar
        {
            get
            {
                return this.Template.FindName("PART_VerticalScrollBar", this) as ScrollBar;
            }
        }

        protected ScrollBar HorizontalScrollBar
        {
            get
            {
                return this.Template.FindName("PART_HorizontalScrollBar", this) as ScrollBar;
            }
        }

        protected RepeatButton LeftScrollRepeatButton
        {
            get
            {
                return this.Template.FindName("LeftScrollRepeatButton", this) as RepeatButton;
            }
        }

        protected RepeatButton RightScrollRepeatButton
        {
            get
            {
                return this.Template.FindName("RightScrollRepeatButton", this) as RepeatButton;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitDispatcherTimer();
            IsSlideScroll = true;

            if (IsSlideScroll)
            {
                IsScrollBarAppear = false;
                IsScrollRepeatButtonAppear = false;
                this.Loaded += FlatScrollViewerLoaded;
                this.Unloaded -= FlatScrollViewerLoaded;
            }
            else
            {
                IsScrollBarAppear = true;
                IsScrollRepeatButtonAppear = true;
            }
        }

        protected virtual void FlatScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (IsSlideScroll && VerticalScrollBar != null && HorizontalScrollBar != null)
            {
                _isFlatScrollBarExist = true;
                VerticalScrollBar.MouseEnter += ScrollBarMouseEnter;
                HorizontalScrollBar.MouseEnter += ScrollBarMouseEnter;

                VerticalScrollBar.MouseLeave += ScrollBarMouseLeave;
                HorizontalScrollBar.MouseLeave += ScrollBarMouseLeave;

                //// It needs make scrollbar slideout firstly.
                ScrollBarWidthHeightAnimation(VerticalScrollBar, false);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, false);
            }
            else
            {
                _isFlatScrollBarExist = false;
            }

            if (IsSlideScroll && LeftScrollRepeatButton != null && RightScrollRepeatButton != null)
            {
                _isScrollRepeatButtonExist = true;

                LeftScrollRepeatButton.MouseEnter += ScrollRepeatButtonMouseEnter;
                LeftScrollRepeatButton.MouseLeave += ScrollRepeatButtonMouseLeave;
                RightScrollRepeatButton.MouseEnter += ScrollRepeatButtonMouseEnter;
                RightScrollRepeatButton.MouseLeave += ScrollRepeatButtonMouseLeave;

                ScrollRepeatButtonWidthAnimation(LeftScrollRepeatButton, false);
                ScrollRepeatButtonWidthAnimation(RightScrollRepeatButton, false);
            }
            else
            {
                _isScrollRepeatButtonExist = false;
            }
        }

        protected void ScrollBarMouseLeave(object sender, MouseEventArgs e)
        {
            _isCursorOnScrollBar = false;
        }

        protected void ScrollBarMouseEnter(object sender, MouseEventArgs e)
        {
            _isCursorOnScrollBar = true;
        }

        protected void ScrollRepeatButtonMouseEnter(object sender, MouseEventArgs e)
        {
            _isCursorOnScrollRepeatButton = true;
        }

        protected void ScrollRepeatButtonMouseLeave(object sender, MouseEventArgs e)
        {
            _isCursorOnScrollRepeatButton = false;
        }

        private void InitDispatcherTimer()
        {
            _delayDispearDt = new DispatcherTimer();
            _delayDispearDt.Interval = TimeSpan.FromMilliseconds(_dispearTime);
            _delayDispearDt.Tick += (sender, e) =>
            {
                if (!_isCursorOnScrollBar)
                {
                    _delayDispearDt.Stop();
                    HiddenScrollBar();
                }

                if (!_isCursorOnScrollRepeatButton)
                {
                    _delayDispearDt.Stop();
                    HiddenScrollRepeatButton();
                }
            };

            _mouseHoverDt = new DispatcherTimer();
            _mouseHoverDt.Interval = TimeSpan.FromMilliseconds(System.Windows.Forms.SystemInformation.MouseHoverTime);
            _mouseHoverDt.Tick += (sender, e) =>
            {
                _delayDispearDt.Stop();
                _mouseHoverDt.Stop();
                EnableScrollBar();
                EnableScrollRepeatButton();
            };
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (IsSlideScroll)
            {
                _previousPos = e.GetPosition(this);
            }
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsSlideScroll && IsTheOriginalSourceDirectlyInThisScrollViewer(e))
            {
                Point mousePosition = e.GetPosition(this);
                if (Math.Abs(mousePosition.X - _previousPos.X) > System.Windows.Forms.SystemInformation.MouseHoverSize.Width ||
                    Math.Abs(mousePosition.Y - _previousPos.Y) > System.Windows.Forms.SystemInformation.MouseHoverSize.Height)
                {
                    if (_mouseHoverDt.IsEnabled)
                    {
                        _mouseHoverDt.Stop();
                    }
                    _mouseHoverDt.Start();

                    _previousPos = mousePosition;
                }
            }
        }

        // If ScrollViewer is nested, only the inner one can have hehaviour, like hover.
        // It's more sample to implement this logic just by set e.Handled = true. but the Region selection is affected.
        // This fuction is to solve this problem.
        private bool IsTheOriginalSourceDirectlyInThisScrollViewer(RoutedEventArgs e)
        {
            FlatScrollViewer viewer = e.OriginalSource as FlatScrollViewer;
            if (viewer == null)
            {
                viewer = VisualTreeHelperUtils.FindAncestor<FlatScrollViewer>(e.OriginalSource as DependencyObject);
            }
            return viewer == this;
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (IsSlideScroll)
            {
                StartOrResetDispearTimer();

                if (_mouseHoverDt.IsEnabled)
                {
                    _mouseHoverDt.Stop();
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            if (IsSlideScroll)
            {
                FlatScrollViewer viewer = e.Source as FlatScrollViewer;
                if ((viewer != null) && (VisualTreeHelperUtils.FindAncestor<ListBox>(viewer as DependencyObject) is ListBox))
                {
                    ListBox listBox = VisualTreeHelperUtils.FindAncestor<ListBox>(viewer as DependencyObject) as ListBox;
                    if ((listBox != null) && (listBox.Items.Count == 0))
                    {
                        FlatScrollViewer parentScrollViewer = VisualTreeHelperUtils.FindAncestor<FlatScrollViewer>(e.OriginalSource as DependencyObject);
                        if (parentScrollViewer != null)
                        {
                            //// If a nested ScrollViewer's content is null, and it has a ScrollViewer parent,
                            //// when scroll over it, it should scroll the parent ScrollViewer.
                            e.Handled = true;
                            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                            eventArg.Source = e.Source;
                            parentScrollViewer.RaiseEvent(eventArg);
                        }
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            EnableScrollBar();
            EnableScrollRepeatButton();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            EnableScrollBar();
            EnableScrollRepeatButton();
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);

            if (IsTheOriginalSourceDirectlyInThisScrollViewer(e))
            {
                EnableScrollBar();
                EnableScrollRepeatButton();
            }
        }

        protected virtual void EnableScrollBar()
        {
            if (!_isScrollBarAppear && _isFlatScrollBarExist)
            {
                IsScrollBarAppear = true;
                ScrollBarWidthHeightAnimation(VerticalScrollBar, true);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, true);
            }
        }

        protected virtual void HiddenScrollBar()
        {
            if (_isScrollBarAppear && _isFlatScrollBarExist)
            {
                IsScrollBarAppear = false;
                ScrollBarWidthHeightAnimation(VerticalScrollBar, false);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, false);
            }
        }

        protected void EnableScrollRepeatButton()
        {
            if (!_isScrollRepeatButtonAppear && _isScrollRepeatButtonExist)
            {
                IsScrollRepeatButtonAppear = true;
                ScrollRepeatButtonWidthAnimation(LeftScrollRepeatButton, true);
                ScrollRepeatButtonWidthAnimation(RightScrollRepeatButton, true);
            }
        }

        protected void HiddenScrollRepeatButton()
        {
            if (_isScrollRepeatButtonAppear && _isScrollRepeatButtonExist)
            {
                IsScrollRepeatButtonAppear = false;
                ScrollRepeatButtonWidthAnimation(LeftScrollRepeatButton, false);
                ScrollRepeatButtonWidthAnimation(RightScrollRepeatButton, false);
            }
        }

        protected void StartOrResetDispearTimer()
        {
            if (_delayDispearDt.IsEnabled)
            {
                _delayDispearDt.Stop();
            }

            _delayDispearDt.Start();
        }

        protected void StartOrResetMouseHoverTimer()
        {
            if (_mouseHoverDt.IsEnabled)
            {
                _mouseHoverDt.Stop();
            }

            _mouseHoverDt.Start();
        }

        protected virtual void ScrollBarWidthHeightAnimation(ScrollBar scrollbar, bool show)
        {
            Storyboard storyboard = new Storyboard();

            if (show)
            {
                DoubleAnimation appearAnimation = new DoubleAnimation(0, 8.0, TimeSpan.FromSeconds(_animateTime));
                appearAnimation.BeginTime = TimeSpan.FromSeconds(0.0);
                if (scrollbar.Orientation == Orientation.Vertical)
                {
                    Storyboard.SetTargetProperty(appearAnimation, new PropertyPath(ScrollBar.WidthProperty));
                }
                else
                {
                    Storyboard.SetTargetProperty(appearAnimation, new PropertyPath(ScrollBar.HeightProperty));
                }

                storyboard.Children.Add(appearAnimation);
            }
            else
            {
                DoubleAnimation disappearAnimation = new DoubleAnimation(8.0, 0, TimeSpan.FromSeconds(_animateTime));
                disappearAnimation.BeginTime = TimeSpan.FromSeconds(0.0);

                if (scrollbar.Orientation == Orientation.Vertical)
                {
                    Storyboard.SetTargetProperty(disappearAnimation, new PropertyPath(ScrollBar.WidthProperty));
                }
                else
                {
                    Storyboard.SetTargetProperty(disappearAnimation, new PropertyPath(ScrollBar.HeightProperty));
                }

                storyboard.Children.Add(disappearAnimation);
            }

            storyboard.Begin(scrollbar);
        }

        protected virtual void ScrollRepeatButtonWidthAnimation(RepeatButton repeatButton, bool show)
        {
            Storyboard storyboard = new Storyboard();

            if (show)
            {
                repeatButton.Visibility = Visibility.Visible;
                DoubleAnimation appearAnimation = new DoubleAnimation(0, 12.0, TimeSpan.FromSeconds(_animateTime));
                appearAnimation.BeginTime = TimeSpan.FromSeconds(0.0);
                Storyboard.SetTargetProperty(appearAnimation, new PropertyPath(RepeatButton.WidthProperty));

                storyboard.Children.Add(appearAnimation);
            }
            else
            {
                DoubleAnimation disappearAnimation = new DoubleAnimation(12.0, 0, TimeSpan.FromSeconds(_animateTime));
                disappearAnimation.BeginTime = TimeSpan.FromSeconds(0.0);
                disappearAnimation.Completed += (sender, e) => DisappearAnimation_Completed(sender, e, repeatButton);
                Storyboard.SetTargetProperty(disappearAnimation, new PropertyPath(RepeatButton.WidthProperty));

                storyboard.Children.Add(disappearAnimation);
            }

            storyboard.Begin(repeatButton);
        }

        private void DisappearAnimation_Completed(object sender, EventArgs e, RepeatButton repeatButton)
        {
            repeatButton.Visibility = Visibility.Collapsed;
        }

        public void EnableSlide(bool enable)
        {
            IsSlideScroll = enable;
            if (_delayDispearDt.IsEnabled)
            {
                _delayDispearDt.Stop();
            }
            if (_mouseHoverDt.IsEnabled)
            {
                _mouseHoverDt.Stop();
            }

            if (!enable)
            {
                EnableScrollBar();
                EnableScrollRepeatButton();
            }
            else
            {
                HiddenScrollBar();
                HiddenScrollRepeatButton();
            }
        }
    }
}