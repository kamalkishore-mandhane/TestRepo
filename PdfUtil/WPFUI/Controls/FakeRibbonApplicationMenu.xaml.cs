using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FakeRibbonApplicationMenu.xaml
    /// </summary>
    public partial class FakeRibbonApplicationMenu : BaseMenu
    {
        public FakeRibbonApplicationMenu()
        {
            InitializeComponent();
        }

        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                const int gestureTextColumn = 4;
                var gestureText = VisualTreeHelperUtils.FindVisualChild<TextBlock>(menuItem, o => Grid.GetColumn(o) == gestureTextColumn);
                if (gestureText != null && gestureText.VerticalAlignment != VerticalAlignment.Center)
                {
                    gestureText.VerticalAlignment = VerticalAlignment.Center;
                    gestureText.Margin = new Thickness(2, 0, 2, 5);
                }

                if (menuItem.Name == "OpenRecent")
                {
                    var model = DataContext as PdfUtilViewModel;
                    if (model != null)
                    {
                        model.LoadRecentFilesXML();
                    }
                }

                if (menuItem.Name == "CreateFrom")
                {
                    menuItem.Focus();
                }
            }
        }

        private void RecentFileItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var recentFile = (sender as MenuItem).DataContext as RecentFile;
            var model = DataContext as PdfUtilViewModel;
            if (model != null && recentFile != null)
            {
                model.Executor(() => model.FakeRibbonTabViewModel.ExecuteOpenRecentItemTask(recentFile.RecentOpenFileCloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                mainMenu.IsSubmenuOpen = false;
            }
        }

        private void OpenRecent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var model = DataContext as PdfUtilViewModel;
            if (model != null)
            {
                var recentListPopup = VisualTreeHelperUtils.FindVisualChild<Popup>(OpenRecent, o => o.Name == "recentListPopup");
                if (recentListPopup != null && !recentListPopup.IsOpen)
                {
                    return;
                }

                var index = -1;
                if ((int)e.Key >= (int)Key.D1 && (int)e.Key <= (int)Key.D9)
                {
                    index = (int)e.Key - (int)Key.D0;
                }
                if ((int)e.Key >= (int)Key.NumPad1 && (int)e.Key <= (int)Key.NumPad9)
                {
                    index = (int)e.Key - (int)Key.NumPad0;
                }

                if (index < 0)
                {
                    return;
                }

                foreach (var recentFile in model.RecentList)
                {
                    int recentIndex = -1;
                    if (int.TryParse(recentFile.RecentFileIndex, out recentIndex))
                    {
                        if (index.Equals(recentIndex))
                        {
                            mainMenu.IsSubmenuOpen = false;
                            model.Executor(() => model.FakeRibbonTabViewModel.ExecuteOpenRecentItemTask(recentFile.RecentOpenFileCloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                            break;
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            mainMenu.Focus();
        }

        private void Menu_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void RecentFileItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var recentFile = (sender as MenuItem).DataContext as RecentFile;
                var model = DataContext as PdfUtilViewModel;
                if (model != null && recentFile != null)
                {
                    model.Executor(() => model.FakeRibbonTabViewModel.ExecuteOpenRecentItemTask(recentFile.RecentOpenFileCloudItem), RetryStrategy.Create(false, 0)).IgnoreExceptions();
                    mainMenu.IsSubmenuOpen = false;
                }
            }
        }

        private void MenuScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollView)
            {
                var dwonButton = scrollView.Template.FindName("DownScrollRepeatButton", scrollView) as RepeatButton;
                if (scrollView.VerticalOffset >= scrollView.ScrollableHeight)
                {
                    if (dwonButton != null)
                    {
                        dwonButton.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (dwonButton != null && dwonButton.Visibility != Visibility.Visible)
                    {
                        dwonButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
