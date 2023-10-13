using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SafeShare.WPFUI.Utils
{
    public static class ItemsControlExtensions
    {
        public static void ScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            if (!itemsControl.TryScrollToCenterOfView(item))
            {
                if (itemsControl is ListBox listBox)
                {
                    listBox.ScrollIntoView(item);
                }

                itemsControl.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    itemsControl.TryScrollToCenterOfView(item);
                }));
            }
        }

        private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;

            if (container != null)
            {
                ScrollContentPresenter presenter = null;
                for (Visual visual = container; visual != null && visual != itemsControl; visual = VisualTreeHelper.GetParent(visual) as Visual)
                {
                    if (visual is ScrollContentPresenter)
                    {
                        presenter = visual as ScrollContentPresenter;
                        break;
                    }
                }

                if (presenter != null)
                {
                    IScrollInfo scrollInfo;
                    if (!presenter.CanContentScroll)
                    {
                        scrollInfo = presenter;
                    }
                    else
                    {
                        if (presenter.Content is IScrollInfo)
                        {
                            scrollInfo = FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo;
                        }
                        else
                        {
                            scrollInfo = presenter;
                        }
                    }

                    Size size = container.RenderSize;
                    Point center = container.TransformToAncestor(scrollInfo as Visual).Transform(new Point(size.Width / 2, size.Height / 2));
                    center.Y += scrollInfo.VerticalOffset;
                    center.X += scrollInfo.HorizontalOffset;

                    if (scrollInfo is StackPanel || scrollInfo is VirtualizingStackPanel)
                    {
                        double logicalCenter = itemsControl.ItemContainerGenerator.IndexFromContainer(container) + 0.5;
                        Orientation orientation = scrollInfo is StackPanel ? (scrollInfo as StackPanel).Orientation : (scrollInfo as VirtualizingStackPanel).Orientation;
                        if (orientation == Orientation.Horizontal)
                        {
                            center.X = logicalCenter;
                        }
                        else
                        {
                            center.Y = logicalCenter;
                        }
                    }

                    if (scrollInfo.CanVerticallyScroll)
                    {
                        scrollInfo.SetVerticalOffset(CenteringOffet(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
                    }

                    if (scrollInfo.CanHorizontallyScroll)
                    {
                        scrollInfo.SetHorizontalOffset(CenteringOffet(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
                    }

                    return true;
                }
            }

            return false;
        }

        private static double CenteringOffet(double center, double viewport, double extent)
        {
            return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
        }

        private static DependencyObject FirstVisualChild(Visual visual)
        {
            return visual != null && VisualTreeHelper.GetChildrenCount(visual) == 0 ? VisualTreeHelper.GetChild(visual, 0) : null;
        }
    }
}