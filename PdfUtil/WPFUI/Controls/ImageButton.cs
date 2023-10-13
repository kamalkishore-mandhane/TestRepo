using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PdfUtil.WPFUI.Controls
{
    class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty ButtonPathSourceProperty = DependencyProperty.Register("ButtonPathSource", typeof(Geometry), typeof(ImageButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ButtonBrushSourceProperty = DependencyProperty.Register("ButtonBrushSource", typeof(DrawingBrush), typeof(ImageButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(ImageButton));

        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public Geometry ButtonPathSource
        {
            get { return (Geometry)this.GetValue(ButtonPathSourceProperty); }
            set { this.SetValue(ButtonPathSourceProperty, value); }
        }

        public DrawingBrush ButtonBrushSource
        {
            get { return (DrawingBrush)this.GetValue(ButtonBrushSourceProperty); }
            set { this.SetValue(ButtonBrushSourceProperty, value); }
        }

        public string ButtonText
        {
            get { return (string)this.GetValue(ButtonTextProperty); }
            set { this.SetValue(ButtonTextProperty, value); }
        }

        public bool HasPathSourceString
        {
            get
            {
                if(ButtonPathSource is null)
                    return false;
                return !string.IsNullOrEmpty(ButtonPathSource.ToString()); 
            }
        }

        public bool HasImageSourceString
        {
            get
            {
                if (ImageSource is null)
                    return false;
                return !string.IsNullOrEmpty(ImageSource.ToString());
            }
        }

        public bool HasBrushSourceString
        {
            get
            {
                if (ButtonBrushSource is null)
                    return false;
                return !string.IsNullOrEmpty(ButtonBrushSource.ToString());
            }
        }
    }

    class ImageToggleButton : ToggleButton
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageToggleButton));
        public static readonly DependencyProperty ButtonPathSourceProperty = DependencyProperty.Register("ButtonPathSource", typeof(Geometry), typeof(ImageToggleButton), new UIPropertyMetadata(null));

        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton), new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public Geometry ButtonPathSource
        {
            get { return (Geometry)this.GetValue(ButtonPathSourceProperty); }
            set { this.SetValue(ButtonPathSourceProperty, value); }
        }

        public bool HasPathSourceString
        {
            get
            {
                if (ButtonPathSource is null)
                    return false;
                return !string.IsNullOrEmpty(ButtonPathSource.ToString());
            }
        }

        public bool HasImageSourceString
        {
            get
            {
                if (ImageSource is null)
                    return false;
                return !string.IsNullOrEmpty(ImageSource.ToString());
            }
        }
    }
}
