using Microsoft.Win32;
using PdfUtil.WPFUI.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for MovePagesDialog.xaml
    /// </summary>
    public partial class MovePagesDialog : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isPutAfter;
        private int _destPage;
        private int _totalPageNumber;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MovePagesDialog(PdfUtilView view, int totalPageNumber)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _totalPageNumber = totalPageNumber;
            Notify("TotalPageNumber");
        }

        public bool IsPutAfter
        { 
            get { return _isPutAfter; } 
        }

        public int DestPage
        {
            get { return _destPage; }
        }

        public int TotalPageNumber
        {
            get { return _totalPageNumber; }
        }

        public bool ShowWindow()
        {
            Owner.IsEnabled = false;
            ShowDialog();
            Owner.IsEnabled = true;
            return DialogResult == true;
        }

        private void MovePagesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void MovePagesDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _isPutAfter = AfterRadioButton.IsChecked == true;
            _destPage = numericUpDown.Value;
            DialogResult = true;
        }

        private void MovePagesDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _isPutAfter = AfterRadioButton.IsChecked == true;
                _destPage = numericUpDown.Value;
                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }
    }
}
