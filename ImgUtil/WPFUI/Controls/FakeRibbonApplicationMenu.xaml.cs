using ImgUtil.WPFUI.Model;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ImgUtil.WPFUI.Controls
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
            if (sender is MenuItem menuItem)
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
                    var model = DataContext as ImgUtilViewModel;
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
            if (DataContext is ImgUtilViewModel model && (sender as MenuItem).DataContext is RecentFile recentFile)
            {
                mainMenu.IsSubmenuOpen = false;
                model.RibbonCommands.OpenRecentCommand.Execute(recentFile);
            }
        }

        private void OpenRecent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ImgUtilViewModel model)
            {
                var recentListPopup = VisualTreeHelperUtils.FindVisualChild<Popup>(OpenRecent, o => o.Name == "recentListPopup");
                if (recentListPopup != null && !recentListPopup.IsOpen)
                {
                    return;
                }

                if ((int)e.Key >= (int)Key.D1 && (int)e.Key <= (int)Key.D9)
                {
                    var key = (int)e.Key - (int)Key.D1;
                    if (key > model.RecentFileList.Count - 1)
                    {
                        return;
                    }
                    model.RibbonCommands.OpenRecentCommand.Execute(model.RecentFileList[key]);
                    mainMenu.IsSubmenuOpen = false;
                    e.Handled = true;
                }

                if ((int)e.Key >= (int)Key.NumPad1 && (int)e.Key <= (int)Key.NumPad9)
                {
                    var key = (int)e.Key - (int)Key.NumPad1;
                    if (key > model.RecentFileList.Count - 1)
                    {
                        return;
                    }
                    model.RibbonCommands.OpenRecentCommand.Execute(model.RecentFileList[key]);
                    mainMenu.IsSubmenuOpen = false;
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

        private void BaseMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void RecentFileItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is ImgUtilViewModel model && (sender as MenuItem).DataContext is RecentFile recentFile)
                {
                    mainMenu.IsSubmenuOpen = false;
                    model.RibbonCommands.OpenRecentCommand.Execute(recentFile);
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
