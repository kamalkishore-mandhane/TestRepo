using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PdfUtil.WPFUI.Utils
{
    class SnapsToDevicePixelsBitmap : Image
    {
        private Point _pixelOffset;

        public SnapsToDevicePixelsBitmap()
        {
            LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            // This event just means that layout happened somewhere.  However, this is
            // what we need since layout anywhere could affect our pixel positioning.
            Point pixelOffset = GetPixelOffset();
            if (!AreClose(pixelOffset, _pixelOffset))
            {
                InvalidateVisual();
            }
        }

        // Gets the matrix that will convert a point from "above" the
        // coordinate space of a visual into the the coordinate space
        // "below" the visual.
        private Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix m = Matrix.Identity;

                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix cm = transform.Value;
                    m = Matrix.Multiply(m, cm);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                m.Translate(offset.X, offset.Y);

                return m;
            }

            return Matrix.Identity;
        }

        private Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError)
        {
            if (v != null)
            {
                Matrix visualTransform = GetVisualTransform(v);
                if (inverse)
                {
                    if (!throwOnError && !visualTransform.HasInverse)
                    {
                        return new Point(0, 0);
                    }
                    visualTransform.Invert();
                }
                point = visualTransform.Transform(point);
            }
            return point;
        }

        private Point ApplyVisualTransform(Point point, Visual v, bool inverse)
        {
            return TryApplyVisualTransform(point, v, inverse, true);
        }

        private Point GetPixelOffset()
        {
            Point pixelOffset = new Point();

            PresentationSource ps = PresentationSource.FromVisual(this);
            if (ps != null)
            {
                Visual rootVisual = ps.RootVisual;

                // Transform (0,0) from this element up to pixels.
                pixelOffset = this.TransformToAncestor(rootVisual).Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, false);
                pixelOffset = ps.CompositionTarget.TransformToDevice.Transform(pixelOffset);

                // Round the origin to the nearest whole pixel.
                pixelOffset.X = Math.Round(pixelOffset.X);
                pixelOffset.Y = Math.Round(pixelOffset.Y);

                // Transform the whole-pixel back to this element.
                pixelOffset = ps.CompositionTarget.TransformFromDevice.Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, true);
                pixelOffset = rootVisual.TransformToDescendant(this).Transform(pixelOffset);
            }

            return pixelOffset;
        }

        private bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        private bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double delta = value1 - value2;
            return (delta < 1.53E-06) && (delta > -1.53E-06);
        }

        protected override void OnRender(DrawingContext dc)
        {
            BitmapSource bitmapSource = (BitmapSource)Source;
            if (bitmapSource != null)
            {
                _pixelOffset = GetPixelOffset();
                this.VisualBitmapScalingMode = System.Windows.Media.BitmapScalingMode.HighQuality;
                dc.DrawImage(bitmapSource, new Rect(_pixelOffset, RenderSize));
            }
        }
    }
}
