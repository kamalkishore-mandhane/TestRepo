using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SBkUpUI.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for LargeImageRibbonButton.xaml
    /// </summary>
    public partial class LargeImageRibbonButton : BaseButton
    {
        public static readonly DependencyProperty LargeImageSourceProperty = DependencyProperty.Register("LargeImageSource", typeof(ImageSource), typeof(LargeImageRibbonButton));
        public static readonly DependencyProperty LargePathSourceProperty = DependencyProperty.Register("LargePathSource", typeof(Geometry), typeof(LargeImageRibbonButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(object), typeof(LargeImageRibbonButton));

        public LargeImageRibbonButton()
        {
            InitializeComponent();
        }

        public ImageSource LargeImageSource
        {
            get { return (ImageSource)this.GetValue(LargeImageSourceProperty); }
            set { this.SetValue(LargeImageSourceProperty, value); }
        }

        public Geometry LargePathSource
        {
            get { return (Geometry)this.GetValue(LargePathSourceProperty); }
            set { this.SetValue(LargePathSourceProperty, value); }
        }

        public object ButtonText
        {
            get { return (object)this.GetValue(ButtonTextProperty); }
            set { this.SetValue(ButtonTextProperty, value); }
        }

        private void RibbonLargeImageButton_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void RibbonLargeImageButton_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
