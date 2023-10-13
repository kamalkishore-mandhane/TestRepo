using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Navigation;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ExperiencePage.xaml
    /// </summary>
    public partial class ExperiencePage : BasePage
    {
        public ExperiencePage()
        {
            InitializeComponent();
        }

        public void InitDataContext()
        {
            var viewModel = new ExperiencePageViewModel(this);
            this.DataContext = viewModel;
        }

        public void SkipToNextPage()
        {
            Progress.Value = 0;
            Progress.Visibility = Visibility.Hidden;
            WorkFlowManager.Clear();
            var page = new FrontPage();
            NavigationService.Navigate(page);
        }

        private void ExperiencePageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            SkipDoneButton.Focus();
            NavigationCommandsManager.RemoveBrowseRefreshKeyGestures();
            MainWindow.AdjustPaneCursor(false);
        }

        private void ExperiencePageView_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;

            NavigationCommandsManager.ResetBrowseRefreshKeyGestures();
        }
    }
}