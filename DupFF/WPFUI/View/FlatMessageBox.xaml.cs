using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DupFF.WPFUI.View
{
    /// <summary>
    /// Interaction logic for FlatMessageBox.xaml
    /// </summary>
    public partial class FlatMessageBox : BaseWindow
    {
        public FlatMessageBox()
        {
            InitializeComponent();
        }

        public static bool ShowWarning(Window owner, string content)
        {
            var window = new FlatMessageBox();
            if (owner?.IsVisible ?? false)
            {
                window.Owner = owner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            window.DialogTitle.Text = content;
            window.ButtonNo.Visibility = Visibility.Collapsed;
            window.ButtonYes.Content = Properties.Resources.OK;
            window.DialogIcon.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Warning.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return window.ShowDialog() == true;
        }

        public static bool ShowQuestion(Window owner, string content)
        {
            var window = new FlatMessageBox();
            window.Owner = owner;
            window.DialogTitle.Text = content;
            window.DialogIcon.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Question.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return window.ShowDialog() == true;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ButtonNo_Click(null, null);
                    break;
                default:
                    break;
            }
        }

        private void FlatMessageBox_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void FlatMessageBox_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
