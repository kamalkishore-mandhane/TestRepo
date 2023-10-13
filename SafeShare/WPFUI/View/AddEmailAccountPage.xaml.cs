using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System.Windows;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for AddEmailAccountPage.xaml
    /// </summary>
    public partial class AddEmailAccountPage : BasePage
    {
        public AddEmailAccountPage(ManageEmailPageViewModel manageEmailPageViewModel)
        {
            InitializeComponent();
            DataContext = manageEmailPageViewModel;
        }

        public void Clear()
        {
            SenderNameTextBox.Text = string.Empty;
            SenderAddressTextBox.Text = string.Empty;
            UserNameTextBox.Text = string.Empty;
            PasswordTextBox.Password = string.Empty;
            SenderNameTextBox.Focus();
            SenderNameTextBox.Select(SenderNameTextBox.Text.Length, 0);
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

        private void AddEmailAccountPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void AddEmailAccountPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}