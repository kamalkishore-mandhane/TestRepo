using Microsoft.Win32;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SafeShare.WPFUI.View
{
    /// <summary>
    /// Interaction logic for ConversionPage.xaml
    /// </summary>
    public partial class ConversionPage : BasePage
    {
        private ConversionPageViewModel _viewModel;
        private FileListPage _fileListPage;
        private EmailEncryptionPage _emailEncryptionPage;

        public ConversionPage(FileListPage filePage, EmailEncryptionPage emailEncryptionPage)
        {
            _fileListPage = filePage;
            _emailEncryptionPage = emailEncryptionPage;
            InitializeComponent();
        }

        public void InitDataContext()
        {
            _viewModel = new ConversionPageViewModel(this, _fileListPage, _emailEncryptionPage);
            this.DataContext = _viewModel;
        }

        private void SaveCopyButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new OtherOptPage();
            page.InitDataContext(ViewModel.OtherOpts.SAVE_COPY_OPT);
            NavigationService.Navigate(page);
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new OtherOptPage();
            page.InitDataContext(ViewModel.OtherOpts.SCHEDULE_DELETION_OPT);
            NavigationService.Navigate(page);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WorkFlowManager.GoBack();
        }

        private void SeeAllButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClickSeeAllAction();
        }

        private void ConvertOptButton_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as Button;
            if (s != null)
            {
                _viewModel.ClickConvertOptAction(this, (ViewModel.ConvertType)Convert.ToInt32(s.Tag.ToString()));
            }
        }

        private void ConversionPageView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            _viewModel.UpdateConvertCheckStatus();
            ShareButton.Focus();
        }

        private void ConversionPageView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        public void UpdatePage(FileListPage filePage, EmailEncryptionPage emailEncryptionPage)
        {
            _fileListPage = filePage;
            _emailEncryptionPage = emailEncryptionPage;
            _viewModel.UpdateViewModelDate(_fileListPage, _emailEncryptionPage);
        }
    }
}