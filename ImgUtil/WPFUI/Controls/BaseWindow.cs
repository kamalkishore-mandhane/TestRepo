using Microsoft.Win32;
using System;
using System.Windows;

namespace ImgUtil.WPFUI.Controls
{
    public class BaseWindow : Window
    {
        protected void UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        protected void SetHighContrastTheme(bool highContrast)
        {
            this.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (highContrast)
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/WinZipColorTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }

        protected bool BaseShowWindow()
        {
            bool oldEnableStatus = false;
            if (Owner != null)
            {
                oldEnableStatus = Owner.IsEnabled;
                Owner.IsEnabled = false;
            }

            ShowDialog();

            if (Owner != null)
            {
                Owner.IsEnabled = oldEnableStatus;
                Owner.Focus();
            }

            return DialogResult == true;
        }
    }
}
