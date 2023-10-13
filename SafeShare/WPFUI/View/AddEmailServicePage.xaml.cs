using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System.Windows;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for AddEmailServicePage.xaml
    /// </summary>
    public partial class AddEmailServicePage : BasePage
    {
        public AddEmailServicePage(ManageEmailPageViewModel manageEmailPageViewModel)
        {
            InitializeComponent();
            DataContext = manageEmailPageViewModel;
        }

        public void Clear()
        {
            ServiceNameTextBox.Text = string.Empty;
            ServerTextBox.Text = string.Empty;
            PortTextBox.Text = "25";
            EncryptionComboBox.SelectedIndex = 0;
            LoginCheckBox.IsChecked = false;
        }

        private void AddEmailServicePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            ServiceNameTextBox.Focus();
            ServiceNameTextBox.Select(ServiceNameTextBox.Text.Length, 0);
        }

        public override void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is ManageEmailPageViewModel viewModel)
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    viewModel.ExecuteBackCommand(this);
                }
            }
        }

        private void AddEmailServicePage_UnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}