using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FileNotSupportWarningDialog.xaml
    /// </summary>
    public partial class FileNotSupportWarningDialog : BaseWindow
    {
        public FileNotSupportWarningDialog(string[] fileList)
        {
            InitializeComponent();

            if (Application.Current?.MainWindow.IsLoaded ?? false)
            {
                Owner = Application.Current.MainWindow;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            FileList = new List<string>();
            foreach (var file in fileList)
            {
                FileList.Add(file);
            }

            FileListBox.ItemsSource = FileList;
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private List<string> FileList
        {
            get;
            set;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CancelBtn.IsFocused || e.Key == Key.Escape)
            {
                CancelBtn_Click(null, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
