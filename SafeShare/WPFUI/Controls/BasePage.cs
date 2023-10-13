using Microsoft.Win32;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SafeShare.WPFUI.Controls
{
    public class BasePage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SafeShareView _mainWindow;

        public SafeShareView MainWindow
        {
            get
            {
                if (_mainWindow == null)
                {
                    _mainWindow = VisualTreeHelperUtils.FindAncestor<Window>(this) as SafeShareView;
                }

                return _mainWindow;
            }
        }

        public virtual void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MainWindow?.ExecuteCloseCommand(null, null);
            }
        }

        protected void UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        protected void SetHighContrastTheme(bool highContrast)
        {
            Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (highContrast)
            {
                Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/ColorTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }
    }
}
