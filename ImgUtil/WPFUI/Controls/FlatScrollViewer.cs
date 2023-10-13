using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.Controls
{
    public class FlatScrollViewer : ScrollViewer
    {
        private static DependencyProperty IsSlideScrollProperty = DependencyProperty.Register(
            "IsSlideScroll", typeof(bool), typeof(FlatScrollViewer), new PropertyMetadata(true));

        private const double _animateTime = 0.2; // 0.2s
        private const double _dispearTime = 300; // 300ms

        private DispatcherTimer _mouseHoverDt;
        private DispatcherTimer _delayDispearDt;

        private bool _isCursorOnScrollBar;
        protected bool _isScrollBarAppear;
        private Point _previousPos;


        public bool IsSlideScroll
        {
            get { return (bool)this.GetValue(IsSlideScrollProperty); }
            protected set { this.SetValue(IsSlideScrollProperty, value); }
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitDispatcherTimer();
            IsSlideScroll = true;
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
            };

            _mouseHoverDt = new DispatcherTimer();
            _mouseHoverDt.Interval = TimeSpan.FromMilliseconds(System.Windows.Forms.SystemInformation.MouseHoverTime);
            _mouseHoverDt.Tick += (sender, e) =>
            {
                _delayDispearDt.Stop();
                _mouseHoverDt.Stop();
                EnableScrollBar();
            };
        }

        protected void StartOrResetDispearTimer()
        {
            if (_delayDispearDt.IsEnabled)
            {
                _delayDispearDt.Stop();
            }

            _delayDispearDt.Start();
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

            if (IsSlideScroll)
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
            }
            else
            {
                HiddenScrollBar();
            }
        }

        protected virtual void FlatScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (IsSlideScroll)
            {
                VerticalScrollBar.MouseEnter += ScrollBarMouseEnter;
                HorizontalScrollBar.MouseEnter += ScrollBarMouseEnter;

                VerticalScrollBar.MouseLeave += ScrollBarMouseLeave;
                HorizontalScrollBar.MouseLeave += ScrollBarMouseLeave;

                //// It needs make scrollbar slideout firstly.
                ScrollBarWidthHeightAnimation(VerticalScrollBar, false);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, false);
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

        protected virtual void EnableScrollBar()
        {
            if (!_isScrollBarAppear)
            {
                _isScrollBarAppear = true;
                ScrollBarWidthHeightAnimation(VerticalScrollBar, true);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, true);
            }
        }

        protected virtual void HiddenScrollBar()
        {
            if (_isScrollBarAppear)
            {
                _isScrollBarAppear = false;
                ScrollBarWidthHeightAnimation(VerticalScrollBar, false);
                ScrollBarWidthHeightAnimation(HorizontalScrollBar, false);
            }
        }

        protected virtual void ScrollBarWidthHeightAnimation(ScrollBar scrollbar, bool show)
        {
            if (scrollbar == null)
            {
                return;
            }

            Storyboard storyboard = new Storyboard();

            if (show)
            {
                DoubleAnimation appearAnimation = new DoubleAnimation(0, 18.0, TimeSpan.FromSeconds(_animateTime));
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
                DoubleAnimation disappearAnimation = new DoubleAnimation(18.0, 0, TimeSpan.FromSeconds(_animateTime));
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
    }
}
