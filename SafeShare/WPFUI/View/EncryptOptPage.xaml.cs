using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for EncryptOptPage.xaml
    /// </summary>
    public partial class EncryptOptPage : BasePage
    {
        private EmailEncryptionPageViewModel _emailEncryptionModel;
        private EncryptOptPageViewModel _viewModel;
        private bool _backChecked = false;

        public EncryptOptPage(EmailEncryptionPageViewModel viewModel)
        {
            _emailEncryptionModel = viewModel;
            InitializeComponent();
            InitDataContext();
        }

        public void InitDataContext()
        {
            _viewModel = new EncryptOptPageViewModel(this, _emailEncryptionModel.IsPasswordCustom, _emailEncryptionModel.SuggestedPassword);
            DataContext = _viewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_backChecked)
            {
                _viewModel.ResetCopyErrorText();
                WorkFlowManager.GoBack();
            }
            else
            {
                if (_viewModel.CanGoBack(_emailEncryptionModel))
                {
                    WorkFlowManager.GoBack();
                }

                _backChecked = true;
            }
        }

        private void CopySuggestedButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClickCopySuggestedButtonAction();
        }

        private void CopyCustomButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClickCopyCustomButtonAction();
        }

        private void EncryptOptPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            _backChecked = false;

            if (!string.IsNullOrEmpty(_emailEncryptionModel.SuggestedPassword))
            {
                _viewModel.SuggestedPassword = _emailEncryptionModel.SuggestedPassword;
            }
            if (!string.IsNullOrEmpty(_emailEncryptionModel.CustomPassword))
            {
                _viewModel.CustomPassword = _emailEncryptionModel.CustomPassword;
                _viewModel.CustomVerifyPassword = _emailEncryptionModel.CustomPassword;
            }

            if (_emailEncryptionModel.IsPasswordCustom)
            {
                CopyCustomButton.Focus();
            }
            else
            {
                CopySuggestedButton.Focus();
            }
        }

        private void EncryptOptPageView_UnLoaded(object sender, RoutedEventArgs e)
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

        private void CustomPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.ResetCopyErrorText(false, true);
        }
    }
}