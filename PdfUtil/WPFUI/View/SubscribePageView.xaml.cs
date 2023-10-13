using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.ViewModel;

namespace PdfUtil.WPFUI.View
{
    /// <summary>
    /// Interaction logic for SubscribePageView.xaml
    /// </summary>
    public partial class SubscribePageView : BasePage
    {
        private Brush GreenTheme = new SolidColorBrush(Color.FromArgb(0xFF, 75, 123, 64));
        private Brush OrangeTheme = new SolidColorBrush(Color.FromArgb(0xFF, 193, 92, 31));
        private Brush RedTheme = new SolidColorBrush(Color.FromArgb(0xFF, 154, 0, 12));
        private Brush BlueTheme = new SolidColorBrush(Color.FromArgb(0xFF, 76, 112, 161));
        private PdfUtilView _pdfUtilView;
        private string _buyNowUrl;

        public SubscribePageView(PdfUtilView view)
        {
            _pdfUtilView = view;
            InitializeComponent();
        }

        public void InitDataContext(TrialPeriodMode mode, int trialDaysRemaining, string buyNowUrl)
        {
            _buyNowUrl = buyNowUrl;
            var actionPaneRootViewModel = new SubscribePageViewModel(this.Dispatcher);

            DataContext = actionPaneRootViewModel;
            SetTrialPeriodMode(mode, trialDaysRemaining);
        }

        public void SetTrialPeriodMode(TrialPeriodMode mode, int trialDaysRemaining)
        {
            switch (mode)
            {
                case TrialPeriodMode.GreenTrialPeriod:
                    if (!SystemParameters.HighContrast)
                    {
                        SubscribePage.Background = GreenTheme;
                        BuyNowButton.Foreground = GreenTheme;
                    }
                    TrialPeriodTextBlock.Text = Properties.Resources.TRIAL_REMAIN_TITLE;
                    RemainDaysTextBlock.Text = string.Format(Properties.Resources.TRIAL_REMAIN_DAYS, trialDaysRemaining);
                    RemainDaysTextBlock.Visibility = Visibility.Visible;
                    BuyNowButton.Content = Properties.Resources.BUY_NOW_BUTTON_TITLE;
                    break;
                case TrialPeriodMode.OrangeTrialPeriod:
                    if (!SystemParameters.HighContrast)
                    {
                        SubscribePage.Background = OrangeTheme;
                        BuyNowButton.Foreground = OrangeTheme;
                    }
                    TrialPeriodTextBlock.Text = Properties.Resources.TRIAL_REMAIN_TITLE;
                    RemainDaysTextBlock.Text = string.Format(Properties.Resources.TRIAL_REMAIN_DAYS, trialDaysRemaining);
                    RemainDaysTextBlock.Visibility = Visibility.Visible;
                    BuyNowButton.Content = Properties.Resources.BUY_NOW_BUTTON_TITLE;
                    break;
                case TrialPeriodMode.RedTrialPeriod:
                    if (!SystemParameters.HighContrast)
                    {
                        SubscribePage.Background = RedTheme;
                        BuyNowButton.Foreground = RedTheme;
                    }
                    TrialPeriodTextBlock.Text = Properties.Resources.TRIAL_EXPRIED_TITLE;
                    RemainDaysTextBlock.Visibility = Visibility.Collapsed;
                    BuyNowButton.Content = Properties.Resources.BUY_NOW_BUTTON_TITLE;
                    break;
                case TrialPeriodMode.BlueSubscribe:
                    if (!SystemParameters.HighContrast)
                    {
                        SubscribePage.Background = BlueTheme;
                        BuyNowButton.Foreground = BlueTheme;
                    }
                    TrialPeriodTextBlock.Text = Properties.Resources.SUBSCRIBE_SUCESS;
                    RemainDaysTextBlock.Visibility = Visibility.Collapsed;
                    BuyNowButton.Visibility = Visibility.Collapsed;
                    DismissButton.Visibility = Visibility.Visible;
                    break;
                default:
                    if (!SystemParameters.HighContrast)
                    {
                        SubscribePage.Background = GreenTheme;
                        BuyNowButton.Foreground = GreenTheme;
                    }
                    TrialPeriodTextBlock.Text = Properties.Resources.TRIAL_REMAIN_TITLE;
                    RemainDaysTextBlock.Text = Properties.Resources.TRIAL_REMAIN_DAYS;
                    BuyNowButton.Content = Properties.Resources.BUY_NOW_BUTTON_TITLE;
                    break;
            }
        }

        private void BuyNowButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(_buyNowUrl));
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            _pdfUtilView.HideUXNagBannerFrame();
        }

        private void SubscribePage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void SubscribePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private new void SetHighContrastTheme(bool highContrast)
        {
            var viewModel = DataContext as SubscribePageViewModel;
            if (viewModel != null)
            {
                if (highContrast)
                {
                    viewModel.IsShowTopSplitLine = true;
                }
                else
                {
                    viewModel.IsShowTopSplitLine = false;
                }
            }

            Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (highContrast)
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/WinZipColorTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }
    }
}
