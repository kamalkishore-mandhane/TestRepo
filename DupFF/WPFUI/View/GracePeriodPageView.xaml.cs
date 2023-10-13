using DupFF.WPFUI.Controls;
using DupFF.WPFUI.Utils;
using DupFF.WPFUI.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Interop;

namespace DupFF.WPFUI.View
{
    /// <summary>
    /// Interaction logic for GracePeriodPageView.xaml
    /// </summary>
    public partial class GracePeriodPageView : BasePage
    {
        private DupFFView _parentView;
        private GracePeriodPageViewModel _viewModel;

        public GracePeriodPageView(DupFFView view)
        {
            InitializeComponent();
            _parentView = view;
        }

        public void InitDataContext(GracePeriodMode mode, int graceDaysRemaining, string userEmail)
        {
            _viewModel = new GracePeriodPageViewModel(mode, graceDaysRemaining, userEmail);
            DataContext = _viewModel;
        }

        private void GracePeriodPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
            _viewModel.IsShowTopSplitLine = SystemParameters.HighContrast;
            _viewModel.InitThemeColorList(this);
        }

        private void GracePeriodPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        public void SetGracePeriodMode(GracePeriodMode mode, int graceDaysRemaining, string userEmail)
        {
            _viewModel.UpdatePeriodAndDays(mode, graceDaysRemaining);
            if (!string.IsNullOrEmpty(userEmail))
            {
                _viewModel.UpdateUserEmail(userEmail);
            }
        }

        private void LogSignButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsLoginSuccess)
            {
                _parentView.HideUXNagBannerFrame();
            }
            else
            {
                WinZipMethods.GraceSignIn(new WindowInteropHelper(_parentView).Handle);
            }
        }

        private void LearnMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // show modal grace period dialog
            WinZipMethods.ShowGracePeriodDialog(new WindowInteropHelper(_parentView).Handle, true);
        }
    }
}
