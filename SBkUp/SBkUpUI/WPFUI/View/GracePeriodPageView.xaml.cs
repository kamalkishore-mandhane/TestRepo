using Microsoft.Win32;
using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SBkUpUI.WPFUI.View
{
    /// <summary>
    /// Interaction logic for GracePeriodPageView.xaml
    /// </summary>
    public partial class GracePeriodPageView : BasePage
    {
        private SBkUpView _parentView;
        private GracePeriodPageViewModel _viewModel;

        public GracePeriodPageView(SBkUpView view)
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
