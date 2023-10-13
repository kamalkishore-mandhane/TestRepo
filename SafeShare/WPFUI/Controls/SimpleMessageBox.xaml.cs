using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace SafeShare.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for SafeShareMessageBox.xaml
    /// </summary>
    public partial class SimpleMessageBox : BaseWindow
    {
        public SimpleMessageBox(string content, bool defaultYes)
        {
            InitializeComponent();
            InitContent(content, defaultYes);

            if (Application.Current?.MainWindow.Visibility == Visibility.Visible)
            {
                Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Topmost = true;
            }
        }

        private void InitContent(string content, bool defaultYes)
        {
            if (!string.IsNullOrEmpty(content))
            {
                ContextTextBlock.Text = content;
            }

            if (defaultYes)
            {
                OkButton.Margin = CancelButton.Margin;
                CancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static bool Show(string content, bool defaultYes)
        {
            var window = new SimpleMessageBox(content, defaultYes);
            return window.BaseShowWindow();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void SimpleMessageBoxWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void SimpleMessageBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            OkButton.Focus();
        }

        private void SimpleMessageBoxWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}