using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ImgUtil.WPFUI.Utils;

namespace ImgUtil.WPFUI.Controls
{
    public enum ScrollingMode
    {
        None = 0,
        Horizontal,
        Vertical,
    }

    /// <summary>
    /// StackPane support touch scrolling
    /// </summary>
    public class TouchScrollingStackPane : StackPanel
    {
        protected ScrollViewer _scrollViewer;
        protected ScrollingMode _scrollingMode;
        protected bool _expandingMode;

        protected IManipulationProcessor _mp;
        protected IInertiaProcessor _ip;
        protected DispatcherTimer _timer;

        protected Point _touchStartPos;
        protected double _yScrollOffset;
        protected double _xScrollOffset;
        protected bool _isHandled;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1 || Environment.OSVersion.Version.Major > 6)
            {
                // If Ink and Handwriting services not be installed on Windows Server 2008 R2, IManipulationProcessor will not be supported.
                // We add a registry value to the HKCU\Software\WinZip Computing\WinZip\Device key called "Ink" and set the value to "Not Installed" when get this error.
                try
                {
                    _mp = new IManipulationProcessor();
                    _ip = new IInertiaProcessor();
                }
                catch
                {
                }
            }

            if (_mp != null && _ip != null)
            {
                _mp.ManipulationDelta += (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
                {
                    OnManipulationDelta(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, false);
                };
                _mp.ManipulationCompleted += OnManipulationCompleted;

                _mp.put_SupportedManipulations(MANIPULATION_PROCESSOR_MANIPULATIONS.MANIPULATION_TRANSLATE_Y);

                _ip.ManipulationDelta += (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
                {
                    OnManipulationDelta(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, true);
                };
                _ip.ManipulationCompleted += OnManipulationCompleted;

                _ip.put_DesiredDeceleration(0.002f);

                _timer = new DispatcherTimer(DispatcherPriority.Input);
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                _timer.Tick += OnTimerTick;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _ip.Process();
        }

        protected virtual void OnManipulationDelta(
            float x,
            float y,
            float translationDeltaX,
            float translationDeltaY,
            float scaleDelta,
            float expansionDelta,
            float rotationDelta,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation,
            bool inInertia)
        {
            if (_scrollingMode == ScrollingMode.Vertical)
            {
                double yScrollOffset = _yScrollOffset - y;
                double yPanningOffset;

                if (yScrollOffset < 0)
                {
                    yPanningOffset = -yScrollOffset;
                }
                else if (yScrollOffset >= _scrollViewer.ScrollableHeight)
                {
                    yPanningOffset = _scrollViewer.ScrollableHeight - yScrollOffset;
                }
                else
                {
                    yPanningOffset = 0;
                }

                NativeMethods.UpdatePanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, 0, (long)yPanningOffset, inInertia);
                _scrollViewer.ScrollToVerticalOffset(yScrollOffset);
            }

            if (_scrollingMode == ScrollingMode.Horizontal)
            {
                double xScrollOffset = _xScrollOffset - x;
                double xPanningOffset;

                if (xScrollOffset < 0)
                {
                    xPanningOffset = -xScrollOffset;
                }
                else if (xScrollOffset >= _scrollViewer.ScrollableWidth)
                {
                    xPanningOffset = _scrollViewer.ScrollableWidth - xScrollOffset;
                }
                else
                {
                    xPanningOffset = 0;
                }

                NativeMethods.UpdatePanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, (long)xPanningOffset, 0, inInertia);
                _scrollViewer.ScrollToHorizontalOffset(xScrollOffset);
            }
        }

        protected virtual void OnManipulationCompleted(
            float x,
            float y,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation)
        {
            if (_scrollingMode != ScrollingMode.None)
            {
                if (_timer.IsEnabled)
                {
                    _scrollingMode = ScrollingMode.None;
                    _timer.Stop();
                    NativeMethods.EndPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, false);
                }
                else
                {
                    float vx = _mp.GetVelocityX();
                    float vy = _mp.GetVelocityY();
                    _ip.Reset();
                    _ip.put_InitialVelocityX(vx);
                    _ip.put_InitialVelocityY(vy);
                    _ip.put_InitialOriginX(x);
                    _ip.put_InitialOriginY(y);

                    _timer.Start();
                }
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            _scrollViewer = VisualTreeHelperUtils.FindAncestor<ScrollViewer>(this);
        }

        protected override void OnStylusButtonDown(StylusButtonEventArgs e)
        {
            base.OnStylusButtonDown(e);

            var point = e.GetPosition(_scrollViewer);
            _touchStartPos = point;
            _scrollingMode = ScrollingMode.None;
            _isHandled = true;
            if (_mp != null && _scrollViewer != null)
            {
                _yScrollOffset = _scrollViewer.VerticalOffset + point.Y;
                _xScrollOffset = _scrollViewer.HorizontalOffset + point.X;

                _mp.ProcessDown(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    NativeMethods.EndPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, false);
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnStylusButtonUp(StylusButtonEventArgs e)
        {
            base.OnStylusButtonUp(e);

            if (_mp == null || _scrollViewer == null)
            {
                return;
            }

            var point = e.GetPosition(_scrollViewer);
            _mp.ProcessUp(e.StylusDevice.Id, (float)point.X, (float)point.Y);
            if (e.StylusDevice.Captured == this)
            {
                e.StylusDevice.Capture(null);
            }
        }

        protected override void OnStylusLeave(StylusEventArgs e)
        {
            base.OnStylusLeave(e);
            if (e.StylusDevice.Captured == this)
            {
                e.StylusDevice.Capture(null);
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            if (_mp == null || _scrollViewer == null)
            {
                base.OnStylusMove(e);
                return;
            }

            var point = e.GetPosition(_scrollViewer);
            if (_scrollingMode != ScrollingMode.None)
            {
                try
                {
                    _mp.ProcessMove(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                }
                catch
                {
                }
                e.Handled = true;
            }
            if (!_isHandled)
            {
                DoGesture(point, e);
            }
        }

        private void DoGesture(Point touchCurrentPos, StylusEventArgs e)
        {
            double xOffset = _touchStartPos.X - touchCurrentPos.X;
            double yOffset = _touchStartPos.Y - touchCurrentPos.Y;
            if (Math.Abs(yOffset) > Math.Abs(xOffset)
                && _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                _scrollingMode = ScrollingMode.Vertical;
                e.StylusDevice.Capture(this);
                NativeMethods.BeginPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle);

                e.Handled = true;
                _isHandled = true;
                try
                {
                    _mp.ProcessMove(e.StylusDevice.Id, (float)touchCurrentPos.X, (float)touchCurrentPos.Y);
                }
                catch
                {
                }
            }
        }

        protected override void OnStylusSystemGesture(System.Windows.Input.StylusSystemGestureEventArgs e)
        {
            if (_mp == null || _scrollViewer == null)
            {
                base.OnStylusSystemGesture(e);
                return;
            }

            var point = e.GetPosition(_scrollViewer);
            double xOffset = _touchStartPos.X - point.X;
            double yOffset = _touchStartPos.Y - point.Y;
            if (e.SystemGesture == SystemGesture.Flick)
            {
                if (Math.Abs(yOffset) == 0 && Math.Abs(xOffset) == 0)
                {
                    e.Handled = true;
                    _isHandled = false;
                    return;
                }
            }
            else if (e.SystemGesture == SystemGesture.Drag && (_scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible || _scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible))
            {
                if (Math.Abs(yOffset) == 0 && Math.Abs(xOffset) == 0)
                {
                    e.Handled = true;
                    _isHandled = false;
                    return;
                }

                if (Math.Abs(yOffset) > Math.Abs(xOffset))
                {
                    _scrollingMode = ScrollingMode.Vertical;
                }
                if (Math.Abs(yOffset) < Math.Abs(xOffset))
                {
                    _scrollingMode = ScrollingMode.Horizontal;
                }

                e.StylusDevice.Capture(this);
                NativeMethods.BeginPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle);

                e.Handled = true;
                try
                {
                    _mp.ProcessMove(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                }
                catch
                {
                }
                return;
            }

            base.OnStylusSystemGesture(e);
        }
    }

    public class FilePaneTouchScrollingStackPane : TouchScrollingStackPane
    {
        protected override void OnManipulationDelta(
                float x,
                float y,
                float translationDeltaX,
                float translationDeltaY,
                float scaleDelta,
                float expansionDelta,
                float rotationDelta,
                float cumulativeTranslationX,
                float cumulativeTranslationY,
                float cumulativeScale,
                float cumulativeExpansion,
                float cumulativeRotation,
                bool inInertia)
        {
            if (_scrollingMode != ScrollingMode.None)
            {
                double yScrollOffset = _yScrollOffset - y;
                double yPanningOffset;

                if (yScrollOffset < 0)
                {
                    yPanningOffset = -yScrollOffset;
                }
                else if (yScrollOffset >= _scrollViewer.ScrollableHeight)
                {
                    yPanningOffset = _scrollViewer.ScrollableHeight - yScrollOffset;
                }
                else
                {
                    yPanningOffset = 0;
                }

                NativeMethods.UpdatePanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, 0, (long)yPanningOffset, inInertia);
                _scrollViewer.ScrollToVerticalOffset(yScrollOffset);
            }
        }

        protected override void OnStylusButtonDown(StylusButtonEventArgs e)
        {
            base.OnStylusButtonDown(e);

            var point = e.GetPosition(_scrollViewer);
            _touchStartPos = point;
            _scrollingMode = ScrollingMode.None;
            _isHandled = true;
            if (_mp != null && _scrollViewer != null && _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                _yScrollOffset = _scrollViewer.VerticalOffset + point.Y;
                _mp.ProcessDown(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    NativeMethods.EndPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle, false);
                }
            }
        }

        protected override void OnStylusButtonUp(StylusButtonEventArgs e)
        {
            base.OnStylusButtonUp(e);

            if (_mp == null || _scrollViewer == null)
            {
                return;
            }

            if (_scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                var point = e.GetPosition(_scrollViewer);
                _mp.ProcessUp(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                if (e.StylusDevice.Captured == this)
                {
                    e.StylusDevice.Capture(null);
                }
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            if (_mp == null || _scrollViewer == null)
            {
                base.OnStylusMove(e);
                return;
            }

            var point = e.GetPosition(_scrollViewer);
            if (_scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible && _scrollingMode == ScrollingMode.Vertical)
            {
                try
                {
                    _mp.ProcessMove(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                }
                catch
                {
                }
                e.Handled = true;
            }
            if (!_isHandled)
            {
                DoGesture(point, e);
            }
        }

        private void DoGesture(Point touchCurrentPos, StylusEventArgs e)
        {
            double xOffset = _touchStartPos.X - touchCurrentPos.X;
            double yOffset = _touchStartPos.Y - touchCurrentPos.Y;
            if (Math.Abs(yOffset) > Math.Abs(xOffset)
                && _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                _scrollingMode = ScrollingMode.Vertical;
                e.StylusDevice.Capture(this);
                NativeMethods.BeginPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle);

                e.Handled = true;
                _isHandled = true;
                try
                {
                    _mp.ProcessMove(e.StylusDevice.Id, (float)touchCurrentPos.X, (float)touchCurrentPos.Y);
                }
                catch
                {
                }
            }
        }

        protected override void OnStylusSystemGesture(System.Windows.Input.StylusSystemGestureEventArgs e)
        {
            if (_mp == null || _scrollViewer == null)
            {
                base.OnStylusSystemGesture(e);
                return;
            }

            var point = e.GetPosition(_scrollViewer);
            double xOffset = _touchStartPos.X - point.X;
            double yOffset = _touchStartPos.Y - point.Y;
            if (e.SystemGesture == SystemGesture.Flick)
            {
                if (Math.Abs(yOffset) == 0 && Math.Abs(xOffset) == 0)
                {
                    e.Handled = true;
                    _isHandled = false;
                    return;
                }
            }
            else if (e.SystemGesture == SystemGesture.Drag)
            {
                if (Math.Abs(yOffset) == 0 && Math.Abs(xOffset) == 0)
                {
                    e.Handled = true;
                    _isHandled = false;
                    return;
                }
                if (Math.Abs(yOffset) > Math.Abs(xOffset) && _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    _scrollingMode = ScrollingMode.Vertical;
                    e.StylusDevice.Capture(this);
                    NativeMethods.BeginPanningFeedback(((HwndSource)HwndSource.FromVisual(this)).Handle);

                    e.Handled = true;
                    try
                    {
                        _mp.ProcessMove(e.StylusDevice.Id, (float)point.X, (float)point.Y);
                    }
                    catch
                    {
                    }
                    return;
                }
            }

            base.OnStylusSystemGesture(e);
        }
    }
}
