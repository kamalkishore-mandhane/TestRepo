using PdfUtil.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PdfUtil.WPFUI.Utils
{
    class DragHilightBorder : Border
    {
        public static readonly DependencyProperty HilightBackgroundProperty = DependencyProperty.Register(
            "HilightBackground", typeof(Brush), typeof(DragHilightBorder), new PropertyMetadata(null));

        public static readonly DependencyProperty HilightBorderBrushProperty = DependencyProperty.Register(
            "HilightBorderBrush", typeof(Brush), typeof(DragHilightBorder), new PropertyMetadata(null));

        public static readonly DependencyProperty HilightEnableProperty = DependencyProperty.Register(
            "HilightEnable", typeof(bool), typeof(DragHilightBorder), new PropertyMetadata(false));

        private Rectangle ListBoxItemIconViewLine = null;
        private Rectangle ListBoxItemIconViewTopLine = null;
        private int IndexInListView = -1;
        private bool _isDragOver;

        static DragHilightBorder()
        {
            BackgroundProperty.OverrideMetadata(typeof(DragHilightBorder), new FrameworkPropertyMetadata(null, null, BackgroundCoerceValueCallback));
            BorderBrushProperty.OverrideMetadata(typeof(DragHilightBorder), new FrameworkPropertyMetadata(null, null, BorderBrushCoerceValueCallback));
        }

        public Brush HilightBackground
        {
            get { return (Brush)this.GetValue(HilightBackgroundProperty); }
            set { this.SetValue(HilightBackgroundProperty, value); }
        }

        public Brush HilightBorderBrush
        {
            get { return (Brush)this.GetValue(HilightBorderBrushProperty); }
            set { this.SetValue(HilightBorderBrushProperty, value); }
        }

        public bool HilightEnable
        {
            get { return (bool)this.GetValue(HilightEnableProperty); }
            set { this.SetValue(HilightEnableProperty, value); }
        }

        private static object BackgroundCoerceValueCallback(DependencyObject d, object baseValue)
        {
            return ((DragHilightBorder)d).BackgroundCoerceValue(baseValue);
        }

        private static object BorderBrushCoerceValueCallback(DependencyObject d, object baseValue)
        {
            return ((DragHilightBorder)d).BorderBrushCoerceValue(baseValue);
        }

        private object BackgroundCoerceValue(object baseValue)
        {
            if (HilightEnable && _isDragOver)
            {
                return HilightBackground;
            }
            return baseValue;
        }

        private object BorderBrushCoerceValue(object baseValue)
        {
            if (HilightEnable && _isDragOver)
            {
                return HilightBorderBrush;
            }
            return baseValue;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            _isDragOver = true;

            if (ListBoxItemIconViewLine == null)
            {
                var parent = VisualTreeHelper.GetParent(this);
                ListBoxItemIconViewLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(parent, o => o.Name == "listBoxItemIconViewLine");
                ListBoxItemIconViewTopLine = VisualTreeHelperUtils.FindVisualChild<Rectangle>(parent, o => o.Name == "listBoxItemIconViewTopLine");
            }

            var listview = VisualTreeHelperUtils.FindSelfOrAncestor<ListView>(this);
            IndexInListView = listview.Items.IndexOf(this.DataContext);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            var listview = VisualTreeHelperUtils.FindSelfOrAncestor<ListView>(this);
            if (listview.SelectedItems.Count == listview.Items.Count)
            {
                return;
            }

            IndexInListView = listview.Items.IndexOf(this.DataContext);

            if (IndexInListView == 0)
            {
                var point = e.GetPosition(this);
                if (point.Y < 100)
                {
                    ListBoxItemIconViewTopLine.Visibility = Visibility.Visible;
                    ListBoxItemIconViewLine.Visibility = Visibility.Hidden;
                }
                else
                {
                    ListBoxItemIconViewTopLine.Visibility = Visibility.Hidden;
                    ListBoxItemIconViewLine.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ListBoxItemIconViewLine.Visibility = Visibility.Visible;
            }

            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            _isDragOver = false;

            if (ListBoxItemIconViewLine != null)
            {
                (this.DataContext as IconItem).PutBelow = ListBoxItemIconViewLine.Visibility == Visibility.Visible;
                ListBoxItemIconViewLine.Visibility = Visibility.Hidden;
                ListBoxItemIconViewTopLine.Visibility = Visibility.Hidden;
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            _isDragOver = false;

            if (ListBoxItemIconViewLine != null)
            {
                ListBoxItemIconViewLine.Visibility = Visibility.Hidden;
                ListBoxItemIconViewTopLine.Visibility = Visibility.Hidden;
            }
        }
    }
}
