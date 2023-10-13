using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for RibbonImageButton.xaml
    /// </summary>
    public partial class RibbonImageButton : BaseButton
    {
        public static readonly DependencyProperty GeometrySourceProperty = DependencyProperty.Register("GeometrySource", typeof(DrawingBrush), typeof(RibbonImageButton));
        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(object), typeof(RibbonImageButton));

        public RibbonImageButton()
        {
            InitializeComponent();
        }

        public DrawingBrush GeometrySource
        {
            get { return (DrawingBrush)this.GetValue(GeometrySourceProperty); }
            set { this.SetValue(GeometrySourceProperty, value); }
        }

        public object ButtonText
        {
            get { return (object)this.GetValue(ButtonTextProperty); }
            set { this.SetValue(ButtonTextProperty, value); }
        }

        private void ImageRibbonButton_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void ImageRibbonButton_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
