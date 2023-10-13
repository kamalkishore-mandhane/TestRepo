using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for CropDetailBox.xaml
    /// </summary>
    public partial class CropDetailBox : BaseControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CropDetailBox()
        {
            InitializeComponent();
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public void RefreshDetail(double height, double width)
        {
            WidthTextBlock.Text = "W : " + width + " px";
            HeightTextBlock.Text = "H : " + height + " px";
        }

        public void SetPos(double x, double y, Rect boundaryRect)
        {
            const int gap = 10;

            var left = x + gap;
            if (left + ActualWidth > boundaryRect.Right)
            {
                left = boundaryRect.Right - ActualWidth;
            }

            var top = y + gap;
            if (top + ActualHeight > boundaryRect.Bottom)
            {
                top = boundaryRect.Bottom - ActualHeight;
            }

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }

        private void CropDetails_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void CropDetails_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
