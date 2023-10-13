using Microsoft.Win32;
using PdfUtil.WPFUI.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for NameNewPdfDialog.xaml
    /// </summary>
    public partial class NameNewPdfDialog : BaseWindow
    {
        private bool _isLoaded = false;

        public NameNewPdfDialog(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
        }

        public string RunDialog(string name)
        {
            inputTextbox.Text = name;
            BaseShowWindow();
            return DialogResult == true ? inputTextbox.Text + extensionTextBlock.Text : string.Empty;
        }

        private void NameNewPdfDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            inputTextbox.SelectAll();
            inputTextbox.Focus();
            _isLoaded = true;
        }

        private void NameNewPdfDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                ProcessInputText();
                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessInputText();
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded)
            {
                if (IsNullOrWhitespace(inputTextbox.Text))
                {
                    ContinueButton.IsEnabled = false;
                }
                else
                {
                    ContinueButton.IsEnabled = true;
                }
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

        private void ProcessInputText()
        {
            inputTextbox.Text = inputTextbox.Text.TrimStart(' ').TrimEnd(' ');
        }

        private void NameNewPdfDialog_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
