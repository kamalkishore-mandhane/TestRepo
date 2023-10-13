using Microsoft.Win32;
using PdfUtil.WPFUI.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for BookmarkActionDialog.xaml
    /// </summary>
    public partial class BookmarkActionView : BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public BookmarkActionView(PdfUtilView view, string title)
        {
            InitializeComponent();
            Owner = view;
            SetTitle(title);
        }

        public string Text
        {
            get
            {
                return inputTextBox.Text;
            }
        }

        public void InitDefaultText(string text)
        {
            inputTextBox.Text = text;
            inputTextBox.SelectAll();
            inputTextBox.Focus();
        }

        private void BookmarkActionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            inputTextBox.Focus();
        }

        private void BookmarkActionDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (!IsNullOrWhitespace(inputTextBox.Text))
                {
                    ProcessInputText();
                    DialogResult = true;
                    Close();
                    e.Handled = true;
                }
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessInputText();
            DialogResult = true;
            Close();
        }

        private void ProcessInputText()
        {
            inputTextBox.Text = inputTextBox.Text.TrimStart(' ').TrimEnd(' ');
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private void SetTitle(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                Title = title;
            }
            else
            {
                Title = Properties.Resources.ADD_BOOKMARK_TITLE;
            }
        }

        private void BookmarkActionDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }

        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNullOrWhitespace(inputTextBox.Text))
            {
                okBtn.IsEnabled = false;
            }
            else
            {
                okBtn.IsEnabled = true;
            }
        }

        private bool IsNullOrWhitespace(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }

            for (var i = 0; i < str.Length; i++)
            {
                if (!char.IsWhiteSpace(str, i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
