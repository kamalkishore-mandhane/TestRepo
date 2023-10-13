using Microsoft.Win32;
using SafeShare.Properties;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for OtherOptPage.xaml
    /// </summary>
    public partial class OtherOptPage : BasePage
    {
        public OtherOptPage()
        {
            InitializeComponent();
        }

        public void InitDataContext(OtherOpts opt)
        {
            var viewModel = new OtherOptPageViewModels(this, opt);
            DataContext = viewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OtherOptPageViewModels viewModel)
            {
                if (viewModel.IsSaveCopyPanelVisible)
                {
                    Settings.Default.LocalDeviceChecked = viewModel.IsLocalDeviceChecked;
                    Settings.Default.SaveFilePath = viewModel.SaveCopyPath;
                }
                else
                {
                    Settings.Default.DeleteFileDays = viewModel.DeleteDays;
                    WorkFlowManager.ShareOptionData.ExpirationDays = viewModel.DeleteDays;
                }

                Settings.Default.Save();
            }

            NavigationService.GoBack();
        }

        private void LocalDeviceTextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (TextBox)sender;
            if (e.Source is TextBox)
            {
                element.SelectAll();
                e.Handled = true;
            }
        }

        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as OtherOptPageViewModels)?.ClickBrowserButtonAction();
        }

        private void OtherOptPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            if (DataContext is OtherOptPageViewModels viewModel)
            {
                if (viewModel.IsSaveCopyPanelVisible)
                {
                    viewModel.IsLocalDeviceChecked = Settings.Default.LocalDeviceChecked;
                }
                else
                {
                    viewModel.DeleteDays = Settings.Default.DeleteFileDays;
                    Num_Days.NumTextBox.Focus();
                    Num_Days.NumTextBox.Select(Num_Days.NumTextBox.Text.Length, 0);
                }
            }
        }

        private void OtherOptPageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        public override void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BackButton_Click(null, null);
            }
        }
    }
}