using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System.Windows;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ServiceSettingsPage.xaml
    /// </summary>
    public partial class ServiceSettingsPage : BasePage
    {
        public ServiceSettingsPage(ManageEmailPageViewModel manageEmailPageViewModel)
        {
            InitializeComponent();
            DataContext = manageEmailPageViewModel;
            AccountsListView.ItemTemplate = TryFindResource("ManageEmailAccountDataTemplate") as DataTemplate;
        }

        public ServiceSettingsPage(ManageCloudPageViewModel manageCloudPageViewModel)
        {
            InitializeComponent();
            DataContext = manageCloudPageViewModel;
            AccountsListView.ItemTemplate = TryFindResource("ManageCloudAccountDataTemplate") as DataTemplate;
        }

        private void ServiceSettingsPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (DataContext is ManageEmailPageViewModel viewModel)
            {
                viewModel.ClearErrorMessage();

                if (viewModel.ManageAccountContext.SelectedAccount != null)
                {
                    AccountsListView.ScrollToCenterOfView(viewModel.ManageAccountContext.SelectedAccount);
                }
            }

            Button_AddAccount.Focus();
        }

        private void ServiceSettingsPageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        public override void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                NavigationService.GoBack();
            }
        }
    }
}