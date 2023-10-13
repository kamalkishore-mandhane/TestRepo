using Microsoft.Win32;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SBkUpUI.WPFUI.View
{
    /// <summary>
    /// Interaction logic for DuplicateName.xaml
    /// </summary>
    public partial class DuplicateNameView : BaseWindow
    {
        private string _name = string.Empty;

        public DuplicateNameView(string name)
        {
            InitializeComponent();
            OverwriteRadioButton.IsChecked = true;
            NewNameRadioButton.IsChecked = false;
            _name = name;
            ContentTextBlock.Text = string.Format(Properties.Resources.DUPLICATE_NAME_VIEW_CONTENT, "'" + name + "'");
        }

        public string FileName
        {
            get
            {
                return _name;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (OverwriteRadioButton.IsChecked.HasValue && OverwriteRadioButton.IsChecked.Value)
            {
                DialogResult = true;
                return;
            }
            if (NewNameRadioButton.IsChecked.HasValue && NewNameRadioButton.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    FlatMessageBox.ShowWarning(this, Properties.Resources.ERROR_NAME_EMPTY);
                    NameTextBox.Focus();
                    return;
                }
                if (NameTextBox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 || Util.IsReservedName(NameTextBox.Text))
                {
                    FlatMessageBox.ShowWarning(this, Properties.Resources.ERROR_NAME_INVALID);
                    NameTextBox.Focus();
                    return;
                }
                if (Path.Combine(SBkUpViewModel.JobFolder, NameTextBox.Text + Swjf.Extension).Length > 256)
                {
                    FlatMessageBox.ShowWarning(this, Properties.Resources.ERROR_NAME_TOO_LONG);
                    NameTextBox.Focus();
                    return;
                }
                if (NameDuplicate(NameTextBox.Text))
                {
                    ContentTextBlock.Text = string.Format(Properties.Resources.DUPLICATE_NAME_VIEW_CONTENT, "'" + NameTextBox.Text + "'");
                    NameTextBox.Focus();
                    return;
                }

                _name = NameTextBox.Text;
                if (_name.EndsWith(Swjf.Extension))
                {
                    _name = _name.Substring(0, _name.Length - Swjf.Extension.Length);
                }
                DialogResult = true;

                return;
            }
        }

        public static bool NameDuplicate(string name)
        {
            if (name.EndsWith(Swjf.Extension))
            {
                return File.Exists(Path.Combine(SBkUpViewModel.JobFolder, name));
            }
            return File.Exists(Path.Combine(SBkUpViewModel.JobFolder, name + Swjf.Extension));
        }

        private void NewNameRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            NameTextBox.IsEnabled = true;
            NameTextBox.Focus();
        }

        private void OverwriteRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            NameTextBox.IsEnabled = false;
        }

        private void DuplicateNameView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void DuplicateNameView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void DuplicateNameView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
