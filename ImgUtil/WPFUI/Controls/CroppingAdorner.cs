using ImgUtil.WPFUI.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImgUtil.WPFUI.Controls
{
    class CroppingAdorner : Adorner
    {
        private double _scale;
        private double _imageRatio;
        private bool _isDraggingCropArea;
        private Point _curPos;
        private PuncturedRect _puncturedRect;
        private Canvas _cropCanvas;
        private CropSelectionTab _selectionTab;
        private CropDetailBox _detailBox;
        private CropThumb _crtTopLeft, _crtTopRight, _crtBottomLeft, _crtBottomRight;
        private CropThumb _crtTop, _crtLeft, _crtBottom, _crtRight;
        private VisualCollection _vc;
        private ImageInfo _imageInfo;
        private FrameworkElement _wrapperElement;
        private ImgUtilView _view;

        public CroppingAdorner(UIElement adornedElement, ImageInfo imageInfo, ImgUtilView view, double scale, FrameworkElement wrapperElement, Action loadedAction)
            : base(adornedElement)
        {
            _isDraggingCropArea = false;
            _imageInfo = imageInfo;
            _imageRatio = imageInfo.Ratio;
            _wrapperElement = wrapperElement;
            _scale = scale;

            _vc = new VisualCollection(this);

            _selectionTab = new CropSelectionTab(view);

            var color = Colors.Black;
            color.A = 110;
            var colorBrush = new SolidColorBrush(color);
            _puncturedRect = new PuncturedRect
            {
                IsHitTestVisible = false,
                RectInterior = new Rect(0, 0, imageInfo.UnitWidth * scale, imageInfo.UnitHeight * scale),
                RectExterior = new Rect(0, 0, imageInfo.UnitWidth * scale, imageInfo.UnitHeight * scale),
                Fill = colorBrush
            };
            _vc.Add(_puncturedRect);

            _cropCanvas = new Canvas { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            _vc.Add(_cropCanvas);

            _cropCanvas.Children.Add(_selectionTab);

            BuildCorner(ref _crtTop, Cursors.SizeNS);
            BuildCorner(ref _crtBottom, Cursors.SizeNS);
            BuildCorner(ref _crtLeft, Cursors.SizeWE);
            BuildCorner(ref _crtRight, Cursors.SizeWE);
            BuildCorner(ref _crtTopRight, Cursors.SizeNESW);
            BuildCorner(ref _crtBottomLeft, Cursors.SizeNESW);
            BuildCorner(ref _crtTopLeft, Cursors.SizeNWSE);
            BuildCorner(ref _crtBottomRight, Cursors.SizeNWSE);

            // Add handlers for Cropping.
            _crtBottomLeft.DragDelta += HandleBottomLeft;
            _crtBottomRight.DragDelta += HandleBottomRight;
            _crtTopLeft.DragDelta += HandleTopLeft;
            _crtTopRight.DragDelta += HandleTopRight;
            _crtTop.DragDelta += HandleTop;
            _crtBottom.DragDelta += HandleBottom;
            _crtRight.DragDelta += HandleRight;
            _crtLeft.DragDelta += HandleLeft;

            view.MouseRightButtonDown += CropArea_MouseRightButtonDown;
            view.MouseMove += CropArea_MouseMove;
            view.MouseRightButtonUp += CropArea_MouseRightButtonUp;
            _view = view;

            if (adornedElement is FrameworkElement frameworkElement)
            {
                frameworkElement.SizeChanged += new SizeChangedEventHandler(AdornedElement_SizeChanged);
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                loadedAction();
            }));
        }

        public double CropScale => _scale;

        public Int32Rect GetCroppedArea()
        {
            var rcInterior = _puncturedRect.RectInterior;

            var rcInteriorSize = UnitsToPx(rcInterior.Width / _scale, rcInterior.Height / _scale, true);
            // top left point always round down
            var rcInteriorTopLeft = UnitsToPx(rcInterior.Left / _scale, rcInterior.Top / _scale, false);

            if (rcInteriorTopLeft.X + rcInteriorSize.X > _imageInfo.PixelWidth)
            {
                rcInteriorSize.X = (int)_imageInfo.PixelWidth - rcInteriorTopLeft.X;
            }

            if (rcInteriorTopLeft.Y + rcInteriorSize.Y > _imageInfo.PixelHeight)
            {
                rcInteriorSize.Y = (int)_imageInfo.PixelHeight - rcInteriorTopLeft.Y;
            }

            return new Int32Rect(rcInteriorTopLeft.X, rcInteriorTopLeft.Y, rcInteriorSize.X, rcInteriorSize.Y);
        }

        private void Thumb_DrageComplete(object sender, DragCompletedEventArgs e)
        {
            _detailBox?.Hide();
        }

        private void HandleThumb(double multiplierLeft, double multiplierTop, double multiplierWidth, double multiplierHeight, double dx, double dy)
        {
            var rcInterior = _puncturedRect.RectInterior;
            var rcExterior = _puncturedRect.RectExterior;

            // drag left
            if (dx * multiplierLeft < 0)
            {
                dx = Math.Max(rcExterior.Left - rcInterior.Left, dx);
            }

            // drag top
            if (dy * multiplierTop < 0)
            {
                dy = Math.Max(rcExterior.Top - rcInterior.Top, dy);
            }

            if (dx == 0 && dy == 0)
            {
                return;
            }

            if (rcInterior.Width + multiplierWidth * dx < 3)
            {
                dx = -(rcInterior.Width - 3) / multiplierWidth;
            }

            if (rcInterior.Height + multiplierHeight * dy < 3)
            {
                dy = -(rcInterior.Height - 3) / multiplierHeight;
            }

            double newLeft = 0, newTop = 0, newRight = 0, newBottom = 0;

            var deltaW = multiplierWidth * dx;
            var deltaH = multiplierHeight * dy;

            if (deltaH == 0 && deltaW == 0)
            {
                return;
            }

            var newWidth = rcInterior.Width + deltaW;
            var newHeight = rcInterior.Height + deltaH;

            // maintain aspect ratio case
            if (_selectionTab.IsMaintainRatio)
            {
                // if drag thumb on the corner, check we expand/contract base on width or height
                var isBaseOnWidth = (multiplierWidth * multiplierHeight != 0) && ((deltaW >= 0 && deltaH <= 0) || ((deltaW * deltaH > 0) && (deltaW > deltaH * _imageRatio)));
                var isBaseOnHeight = (multiplierWidth * multiplierHeight != 0) && ((deltaW <= 0 && deltaH >= 0) || ((deltaW * deltaH > 0) && (deltaW < deltaH * _imageRatio)));

                if ((multiplierWidth < 0 && multiplierHeight == 0)			// left
                    || (multiplierWidth > 0 && multiplierHeight == 0)		// right
                    || isBaseOnWidth)
                {
                    newHeight = newWidth / _imageRatio;
                }

                if ((multiplierWidth == 0 && multiplierHeight < 0)			// top
                    || (multiplierWidth == 0 && multiplierHeight > 0)		// bottom
                    || isBaseOnHeight)
                {
                    newWidth = newHeight * _imageRatio;
                }

                // expand to Top Left
                if ((multiplierWidth < 0 && multiplierHeight == 0)			// left
                    || (multiplierWidth == 0 && multiplierHeight < 0)		// top
                    || (multiplierWidth < 0 && multiplierHeight < 0))		// top left
                {
                    newRight = rcInterior.Right;
                    newBottom = rcInterior.Bottom;

                    // top edge reach the boundary
                    if (rcInterior.Bottom - newHeight < rcExterior.Top)
                    {
                        newHeight = rcInterior.Bottom - rcExterior.Top;
                        newWidth = newHeight * _imageRatio;
                    }

                    // left edge reach the boundary
                    if (rcInterior.Right - newWidth < rcExterior.Left)
                    {
                        newWidth = rcInterior.Right - rcExterior.Left;
                        newHeight = newWidth / _imageRatio;
                    }

                    newLeft = Math.Min(rcInterior.Right - newWidth, rcInterior.Right);
                    newTop = Math.Min(rcInterior.Bottom - newHeight, rcInterior.Bottom);
                }

                // expand to Bottom Left
                if (multiplierWidth < 0 && multiplierHeight > 0)
                {
                    newRight = rcInterior.Right;
                    newTop = rcInterior.Top;

                    // bottom edge reach the boundary
                    if (rcInterior.Top + newHeight > rcExterior.Bottom)
                    {
                        newHeight = rcExterior.Bottom - rcInterior.Top;
                        newWidth = newHeight * _imageRatio;
                    }

                    // left edge reach the boundary
                    if (rcInterior.Right - newWidth < rcExterior.Left)
                    {
                        newWidth = rcInterior.Right - rcExterior.Left;
                        newHeight = newWidth / _imageRatio;
                    }

                    newLeft = Math.Min(rcInterior.Right - newWidth, rcInterior.Right);
                    newBottom = Math.Max(rcInterior.Top + newHeight, rcInterior.Top);
                }

                // expand to Top Right
                if (multiplierWidth > 0 && multiplierHeight < 0)
                {
                    newLeft = rcInterior.Left;
                    newBottom = rcInterior.Bottom;

                    // top edge reach the boundary
                    if (rcInterior.Bottom - newHeight < rcExterior.Top)
                    {
                        newHeight = rcInterior.Bottom - rcExterior.Top;
                        newWidth = newHeight * _imageRatio;
                    }

                    // right edge reach the boundary
                    if (rcInterior.Left + newWidth > rcExterior.Right)
                    {
                        newWidth = rcExterior.Right - rcInterior.Left;
                        newHeight = newWidth / _imageRatio;
                    }

                    newRight = Math.Max(rcInterior.Left + newWidth, rcInterior.Left);
                    newTop = Math.Min(rcInterior.Bottom - newHeight, rcInterior.Bottom);
                }

                // expand to Bottom Right
                if ((multiplierWidth > 0 && multiplierHeight == 0)			// right
                    || (multiplierWidth == 0 && multiplierHeight > 0)		// bottom
                    || (multiplierWidth > 0 && multiplierHeight > 0))       // bottom right
                {
                    newLeft = rcInterior.Left;
                    newTop = rcInterior.Top;

                    // bottom edge reach the boundary
                    if (rcInterior.Top + newHeight > rcExterior.Bottom)
                    {
                        newHeight = rcExterior.Bottom - rcInterior.Top;
                        newWidth = newHeight * _imageRatio;
                    }

                    // right edge reach the boundary
                    if (rcInterior.Left + newWidth > rcExterior.Right)
                    {
                        newWidth = rcExterior.Right - rcInterior.Left;
                        newHeight = newWidth / _imageRatio;
                    }

                    newRight = Math.Max(rcInterior.Left + newWidth, rcInterior.Left);
                    newBottom = Math.Max(rcInterior.Top + newHeight, rcInterior.Top);
                }
            }
            else
            {
                // make sure they are inside image
                newLeft = Math.Max(rcInterior.Left + multiplierLeft * dx, rcExterior.Left);
                newTop = Math.Max(rcInterior.Top + multiplierTop * dy, rcExterior.Top);

                newRight = Math.Min(newLeft + newWidth, rcExterior.Right);
                newBottom = Math.Min(newTop + newHeight, rcExterior.Bottom);
            }

            rcInterior = new Rect(newLeft, newTop, newRight - newLeft, newBottom - newTop);

            _puncturedRect.RectInterior = rcInterior;

            _selectionTab.SetPos((rcInterior.Left + rcInterior.Right) / 2, rcInterior.Top, GetBoundaryRect());
            _selectionTab.SetEnableCrop(rcInterior.Width != 0 && rcInterior.Height != 0);

            SetThumbs(_puncturedRect.RectInterior);

            if (_detailBox == null)
            {
                _detailBox = new CropDetailBox();
                _cropCanvas.Children.Add(_detailBox);
            }
            _detailBox.Show();
            _detailBox.SetPos(rcInterior.Right, rcInterior.Bottom, GetBoundaryRect());
            var rcInteriorPx = UnitsToPx(rcInterior.Width / _scale, rcInterior.Height / _scale, true);
            _detailBox.RefreshDetail(rcInteriorPx.Y, rcInteriorPx.X);
        }

        // Handler for Cropping from the bottom-left.
        private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 0, -1, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the bottom-right.
        private void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 1, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the top-right.
        private void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 1, 1, -1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the top-left.
        private void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 1, -1, -1, args.HorizontalChange, args.VerticalChange);
            }
        }
        // Handler for Cropping from the top.

        private void HandleTop(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 1, 0, -1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the left.
        private void HandleLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 0, -1, 0, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the right.
        private void HandleRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 1, 0, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the bottom.
        private void HandleBottom(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 0, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        private void SetThumbs(Rect rc)
        {
            _crtBottomRight.SetPos(rc.Right, rc.Bottom);
            _crtTopLeft.SetPos(rc.Left, rc.Top);
            _crtTopRight.SetPos(rc.Right, rc.Top);
            _crtBottomLeft.SetPos(rc.Left, rc.Bottom);
            _crtTop.SetPos(rc.Left + rc.Width / 2, rc.Top);
            _crtBottom.SetPos(rc.Left + rc.Width / 2, rc.Bottom);
            _crtLeft.SetPos(rc.Left, rc.Top + rc.Height / 2);
            _crtRight.SetPos(rc.Right, rc.Top + rc.Height / 2);
        }

        private void BuildCorner(ref CropThumb crt, Cursor crs)
        {
            if (crt != null) return;

            crt = new CropThumb();
            crt.Cursor = crs;

            crt.DragCompleted += Thumb_DrageComplete;

            _cropCanvas.Children.Add(crt);
        }

        private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fel = sender as FrameworkElement;
            Rect rcInterior = _puncturedRect.RectInterior;
            bool fFixupRequired = false;
            double
                intLeft = rcInterior.Left,
                intTop = rcInterior.Top,
                intWidth = rcInterior.Width,
                intHeight = rcInterior.Height;

            if (rcInterior.Left > fel.RenderSize.Width)
            {
                intLeft = fel.RenderSize.Width;
                intWidth = 0;
                fFixupRequired = true;
            }

            if (rcInterior.Top > fel.RenderSize.Height)
            {
                intTop = fel.RenderSize.Height;
                intHeight = 0;
                fFixupRequired = true;
            }

            if (rcInterior.Right > fel.RenderSize.Width)
            {
                intWidth = Math.Max(0, fel.RenderSize.Width - intLeft);
                fFixupRequired = true;
            }

            if (rcInterior.Bottom > fel.RenderSize.Height)
            {
                intHeight = Math.Max(0, fel.RenderSize.Height - intTop);
                fFixupRequired = true;
            }
            if (fFixupRequired)
            {
                _puncturedRect.RectInterior = new Rect(intLeft, intTop, intWidth, intHeight);
            }
        }

        private void CropArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(AdornedElement);
            if (position.X < _puncturedRect.RectInterior.Right
                && position.X > _puncturedRect.RectInterior.Left
                && position.Y < _puncturedRect.RectInterior.Bottom
                && position.Y > _puncturedRect.RectInterior.Top)
            {
                _isDraggingCropArea = true;
                _curPos = position;
                Cursor = Cursors.Hand;
            }
            else
            {
                _isDraggingCropArea = false;
            }
        }

        private void CropArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingCropArea)
            {
                var newPos = e.GetPosition(AdornedElement);
                var dx = newPos.X - _curPos.X;
                var dy = newPos.Y - _curPos.Y;

                var rcInterior = _puncturedRect.RectInterior;
                var rcExterior = _puncturedRect.RectExterior;

                // move left
                if (dx < 0)
                {
                    dx = Math.Max(rcExterior.Left - rcInterior.Left, dx);
                }
                // move right
                else
                {
                    dx = Math.Min(rcExterior.Right - rcInterior.Right, dx);
                }

                // move top
                if (dy < 0)
                {
                    dy = Math.Max(rcExterior.Top - rcInterior.Top, dy);
                }
                // move bottom
                {
                    dy = Math.Min(rcExterior.Bottom - rcInterior.Bottom, dy);
                }

                _curPos = newPos;

                rcInterior = new Rect(rcInterior.Left + dx, rcInterior.Top + dy, rcInterior.Width, rcInterior.Height);

                _puncturedRect.RectInterior = rcInterior;

                _selectionTab.SetPos((rcInterior.Left + rcInterior.Right) / 2, rcInterior.Top, GetBoundaryRect());

                SetThumbs(_puncturedRect.RectInterior);

                if (_detailBox != null)
                {
                    _detailBox.Show();
                    _detailBox.SetPos(rcInterior.Right, rcInterior.Bottom, GetBoundaryRect());
                }
            }
        }

        private void CropArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingCropArea = false;
            Cursor = null;
            _detailBox?.Hide();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _puncturedRect.Arrange(_puncturedRect.RectExterior);

            _selectionTab.SetPos((_puncturedRect.RectInterior.Left + _puncturedRect.RectInterior.Right) / 2, _puncturedRect.RectInterior.Top, GetBoundaryRect());

            SetThumbs(_puncturedRect.RectInterior);

            _cropCanvas.Arrange(_puncturedRect.RectExterior);
            return finalSize;
        }

        private System.Drawing.Point UnitsToPx(double x, double y, bool isRounded)
        {
            if (isRounded)
            {
                return new System.Drawing.Point(Math.Max(Convert.ToInt32((x * _imageInfo.Dpi_X / 96)), 1), Math.Max(Convert.ToInt32((y * _imageInfo.Dpi_Y / 96)), 1));
            }
            else
            {
                return new System.Drawing.Point((int)(x * _imageInfo.Dpi_X / 96), (int)(y * _imageInfo.Dpi_Y / 96));
            }
        }

        private Rect GetBoundaryRect()
        {
            var topLeftPt = _wrapperElement.TranslatePoint(new Point(0, 0), AdornedElement);
            var rightBottomPt = _wrapperElement.TranslatePoint(new Point(_wrapperElement.ActualWidth, _wrapperElement.ActualHeight), AdornedElement);
            return new Rect(topLeftPt, rightBottomPt);
        }

        protected override int VisualChildrenCount { get { return _vc.Count; } }
        protected override Visual GetVisualChild(int index) { return _vc[index]; }

        public void UpdateAdorner(double newScale)
        {
            var rcInterior = _puncturedRect.RectInterior;

            var selectionTabHeight = _selectionTab.ActualHeight + 3;

            var newCropScale = newScale * 0.9;
            if (AdornedElement is FrameworkElement element && (_wrapperElement.ActualHeight < (element.ActualHeight + selectionTabHeight * 2)))
            {
                newCropScale = (_imageInfo.UnitHeight * newScale - selectionTabHeight * 2) / _imageInfo.UnitHeight;
            }

            var resizeRatio = newCropScale / _scale;
            _scale = newCropScale;
            rcInterior = new Rect(rcInterior.Left * resizeRatio, rcInterior.Top * resizeRatio, rcInterior.Width * resizeRatio, rcInterior.Height * resizeRatio);
            _puncturedRect.RectExterior = new Rect(0, 0, _imageInfo.UnitWidth * _scale, _imageInfo.UnitHeight * _scale);
            _puncturedRect.RectInterior = rcInterior;

            _selectionTab.SetPos((rcInterior.Left + rcInterior.Right) / 2, rcInterior.Top, GetBoundaryRect());
            _selectionTab.SetEnableCrop(rcInterior.Width != 0 && rcInterior.Height != 0);

            SetThumbs(rcInterior);

            if (_detailBox != null)
            {
                _detailBox.SetPos(rcInterior.Right, rcInterior.Bottom, GetBoundaryRect());
                var rcInteriorPx = UnitsToPx(rcInterior.Width / _scale, rcInterior.Height / _scale, true);
                _detailBox.RefreshDetail(rcInteriorPx.Y, rcInteriorPx.X);
            }
        }

        public void CleanHandlers()
        {
            _view.MouseRightButtonDown -= CropArea_MouseRightButtonDown;
            _view.MouseMove -= CropArea_MouseMove;
            _view.MouseRightButtonUp -= CropArea_MouseRightButtonUp;
        }
    }

    class PuncturedRect : Shape
    {
        public PuncturedRect() : this(new Rect(0, 0, double.MaxValue, double.MaxValue), new Rect()) { }

        public PuncturedRect(Rect rectExterior, Rect rectInterior)
        {
            RectInterior = rectInterior;
            RectExterior = rectExterior;
        }

        public static readonly DependencyProperty RectInteriorProperty = DependencyProperty.Register("RectInterior", typeof(Rect), typeof(FrameworkElement),
                new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender, null, new CoerceValueCallback(CoerceRectInterior), false ), null);

        private static object CoerceRectInterior(DependencyObject d, object value)
        {
            var pr = (PuncturedRect)d;
            var rcExterior = pr.RectExterior;
            var rcProposed = (Rect)value;
            double left = Math.Min(Math.Max(rcProposed.Left, rcExterior.Left), rcExterior.Right);
            double top = Math.Min(Math.Max(rcProposed.Top, rcExterior.Top), rcExterior.Bottom);
            double width = Math.Min(rcProposed.Right, rcExterior.Right) - left;
            double height = Math.Min(rcProposed.Bottom, rcExterior.Bottom) - top;
            rcProposed = new Rect(left, top, Math.Max(width, 0), Math.Max(height, 0));
            return rcProposed;
        }

        public Rect RectInterior
        {
            get { return (Rect)GetValue(RectInteriorProperty); }
            set { SetValue(RectInteriorProperty, value); }
        }


        public static readonly DependencyProperty RectExteriorProperty = DependencyProperty.Register("RectExterior", typeof(Rect), typeof(FrameworkElement),
                new FrameworkPropertyMetadata( new Rect(0, 0, double.MaxValue, double.MaxValue), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender, null, null, false), null);

        public Rect RectExterior
        {
            get { return (Rect)GetValue(RectExteriorProperty); }
            set { SetValue(RectExteriorProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var pthgExt = new PathGeometry();
                var pthfExt = new PathFigure { StartPoint = RectExterior.TopLeft };
                pthfExt.Segments.Add(new LineSegment(RectExterior.TopRight, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.BottomRight, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.BottomLeft, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.TopLeft, false));
                pthgExt.Figures.Add(pthfExt);

                var rectIntSect = Rect.Intersect(RectExterior, RectInterior);
                var pthgInt = new PathGeometry();
                var pthfInt = new PathFigure { StartPoint = rectIntSect.TopLeft };
                pthfInt.Segments.Add(new LineSegment(rectIntSect.TopRight, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomRight, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomLeft, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.TopLeft, false));
                pthgInt.Figures.Add(pthfInt);

                var cmbg = new CombinedGeometry(GeometryCombineMode.Exclude, pthgExt, pthgInt);
                return cmbg;
            }
        }
    }

    class CropThumb : Thumb
    {
        int _width = 6;

        public CropThumb() : base()
        {
            Width = _width;
            Height = _width;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRoundedRectangle(Brushes.DodgerBlue, new Pen(Brushes.Black, 0.5), new Rect(new Size(_width, _width)), 1, 1);
        }

        protected override Visual GetVisualChild(int index)
        {
            return null;
        }

        internal void SetPos(double x, double y)
        {
            Canvas.SetTop(this, y - _width / 2);
            Canvas.SetLeft(this, x - _width / 2);
        }
    }

    class ImageInfo
    {
        public ImageInfo(double pixelHeight, double pixelWidth, double dpiX, double dpiY)
        {
            PixelHeight = pixelHeight;
            PixelWidth = pixelWidth;
            Dpi_X = dpiX;
            Dpi_Y = dpiY;
            Ratio = pixelWidth / pixelHeight;
            UnitHeight = (pixelHeight / dpiY) * 96;
            UnitWidth = (pixelWidth / dpiX) * 96;
        }

        public double PixelHeight { get; private set; }

        public double PixelWidth { get; private set; }

        public double Dpi_X { get; private set; }

        public double Dpi_Y { get; private set; }

        public double Ratio { get; private set; }

        public double UnitHeight { get; private set; }

        public double UnitWidth { get; private set; }
    }
}
