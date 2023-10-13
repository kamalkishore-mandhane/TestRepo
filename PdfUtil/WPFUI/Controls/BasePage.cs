﻿using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PdfUtil.WPFUI.Controls
{
    public class BasePage : Page
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
