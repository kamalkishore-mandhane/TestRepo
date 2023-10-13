using Microsoft.Win32;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Interop;

namespace PdfUtil.WPFUI.View
{
    /// <summary>
    /// Interaction logic for GracePeriodPageView.xaml
    /// </summary>
    public partial class GracePeriodPageView : BasePage
    {
        private PdfUtilView _parentView;
        private GracePeriodPageViewModel _viewModel;

        public GracePeriodPageView(PdfUtilView view)
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
                WinzipMethods.GraceSignIn(new WindowInteropHelper(_parentView).Handle);
            }
        }

        private void LearnMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // show modal grace period dialog
            WinzipMethods.ShowGracePeriodDialog(new WindowInteropHelper(_parentView).Handle, true);
        }
    }
}
