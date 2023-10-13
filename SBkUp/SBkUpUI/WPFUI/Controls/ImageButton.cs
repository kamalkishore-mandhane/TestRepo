using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SBkUpUI.WPFUI.Controls
{
    public class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageButton));
        public static readonly DependencyProperty ButtonPathSourceProperty = DependencyProperty.Register("ButtonPathSource", typeof(Geometry), typeof(ImageButton), new UIPropertyMetadata(null));

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

        public bool HasButtonPathSource
        {
            get
            {
                if (ButtonPathSource is null)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(ButtonPathSource.ToString());
            }
        }

        public bool HasImageSourceString
        {
            get
            {
                if (ImageSource is null)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(ImageSource.ToString());
            }
        }
    }

    public class ImageToggleButton : ToggleButton
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageToggleButton));

        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton), new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }
    }
}
