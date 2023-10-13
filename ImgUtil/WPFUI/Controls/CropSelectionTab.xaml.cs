using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for CropSelectionTab.xaml
    /// </summary>
    public partial class CropSelectionTab : BaseControl
    {
        ImgUtilView _view;

        public CropSelectionTab(ImgUtilView view)
        {
            InitializeComponent();
            _view = view;
        }

        public bool IsMaintainRatio => MaintainRationCheckBox.IsChecked == true;

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (_view != null)
            {
                _view.RemoveCroppingAdorner();
            }
        }

        private void CropButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (_view != null)
            {
                _view.DoCrop();

                TrackHelper.LogImgToolsEvent("crop", IsMaintainRatio? "keep" : "free");
            }
        }

        public void SetEnableCrop(bool isEnable)
        {
            CropButton.IsEnabled = isEnable;
        }

        public void SetPos(double x, double y, Rect boundaryRect)
        {
            const int gap = 3;

            var top = y - ActualHeight - gap;
            if (top < boundaryRect.Top)
            {
                top = boundaryRect.Top - gap;
            }

            var left = x - ActualWidth / 2;
            if (left < boundaryRect.Left)
            {
                left = boundaryRect.Left + gap;
            }

            if (left + ActualWidth > boundaryRect.Right)
            {
                left = boundaryRect.Right - gap - ActualWidth;
            }

            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        private void CropSelectionTab_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void CropSelectionTab_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
