using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using PdfUtil.WPFUI.Controls;
using PdfUtil.WPFUI.ViewModel;

namespace PdfUtil.WPFUI.View
{
    /// <summary>
    /// Interaction logic for WatermarkSettingView.xaml
    /// </summary>
    public partial class WatermarkSettingView : BaseWindow
    {
        private Cursor ColorPickerCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/color-pro.cur", UriKind.RelativeOrAbsolute)).Stream);
        public WatermarkSettingView(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public void InitDataContext()
        {
            var viewModel = new WatermarkSettingViewModel(this);
            DataContext = viewModel;
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        public IntPtr WindowHandle { get; private set; }

        public bool IsInsidePreviewImage { get; private set; }

        private void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as WatermarkSettingViewModel)?.ClickOpenImageButtonAction();
        }

        private void EyeDropperButton_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as ImageToggleButton)?.IsChecked ?? false;

            if (isChecked)
            {
                PreviewImage.Cursor = ColorPickerCursor;
                PreviewImage.MouseEnter += PreviewImage_MouseEnter;
                PreviewImage.MouseLeave += PreviewImage_MouseLeave;
                PreviewImage.MouseMove += PreviewImage_MouseMove;
                PreviewImage.MouseLeftButtonDown += PreviewImage_MouseLeftButtonDown;
            }
            else
            {
                PreviewImage.Cursor = Cursor;
                PreviewImage.MouseEnter -= PreviewImage_MouseEnter;
                PreviewImage.MouseLeave -= PreviewImage_MouseLeave;
                PreviewImage.MouseMove -= PreviewImage_MouseMove;
                PreviewImage.MouseLeftButtonDown -= PreviewImage_MouseLeftButtonDown;
            }
        }

        private void FontButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as WatermarkSettingViewModel)?.ClickFontButtonAction();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as WatermarkSettingViewModel)?.ClickOKButtonAction();
        }

        private void WatermarkSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHandle = new WindowInteropHelper(this).Handle;
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
            (DataContext as WatermarkSettingViewModel)?.InitResultWatermark();
        }

        private void WatermarkSettingsView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void PreviewImage_MouseEnter(object sender, MouseEventArgs e)
        {
            IsInsidePreviewImage = true;
        }

        private void PreviewImage_MouseLeave(object sender, MouseEventArgs e)
        {
            IsInsidePreviewImage = false;
        }

        private void PreviewImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (ColorPickerButton.IsChecked == true && IsInsidePreviewImage)
            {
                var point = e.GetPosition(PreviewImage);
                (DataContext as WatermarkSettingViewModel)?.PreviewImageMouseMoveAction(point);
            }
        }

        private void PreviewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ColorPickerButton.IsChecked == true && IsInsidePreviewImage)
            {
                (DataContext as WatermarkSettingViewModel)?.BackgroundTransparencyAction();
                PreviewImage.Cursor = Cursor;
                PreviewImage.MouseEnter -= PreviewImage_MouseEnter;
                PreviewImage.MouseLeave -= PreviewImage_MouseLeave;
                PreviewImage.MouseMove -= PreviewImage_MouseMove;
                PreviewImage.MouseLeftButtonDown -= PreviewImage_MouseLeftButtonDown;
            }
        }

        private void WatermarkSettingsViewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (ColorPickerButton.IsChecked == true)
                    {
                        (DataContext as WatermarkSettingViewModel)?.BackgroundTransparencyCancleAction();
                        PreviewImage.Cursor = Cursor;
                        PreviewImage.MouseEnter -= PreviewImage_MouseEnter;
                        PreviewImage.MouseLeave -= PreviewImage_MouseLeave;
                        PreviewImage.MouseMove -= PreviewImage_MouseMove;
                        PreviewImage.MouseLeftButtonDown -= PreviewImage_MouseLeftButtonDown;
                    }
                    else
                    {
                        this.Close();
                    }
                    e.Handled = true;
                    break;
                default:
                    return;
            }
        }

        private void Panel_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Panel panel && panel.IsKeyboardFocusWithin == true)
            {
                foreach (var child in panel.Children)
                {
                    if (child is RadioButton button && button.IsChecked == true)
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(button), button);
                    }
                }
            }
        }
    }
}
