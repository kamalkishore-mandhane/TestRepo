using Microsoft.Win32;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for RotateWindow.xaml
    /// </summary>
    public enum DegreesSelected
    {
        None = -1,
        On90DegreesClockwise,
        On180Degrees,
        On270Clockwise,
    }

    public partial class RotateView : BaseWindow, INotifyPropertyChanged
    {
        private DegreesSelected _curDegreesSelected = DegreesSelected.On90DegreesClockwise;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public RotateView(PdfUtilView view)
        {
            InitializeComponent();
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        public DegreesSelected CurDegreesSelected
        {
            get { return _curDegreesSelected; }
            set
            {
                if (_curDegreesSelected != value)
                {
                    _curDegreesSelected = value;
                    OnPropertyChanged(nameof(CurDegreesSelected));
                }
            }
        }

        private void DegreesRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
            {
                return;
            }

            if (radioButton == On90DegreesBtn)
            {
                CurDegreesSelected = DegreesSelected.On90DegreesClockwise;
            }
            else if (radioButton == On180DegreesBtn)
            {
                CurDegreesSelected = DegreesSelected.On180Degrees;
            }
            else if (radioButton == On270DegreesBtn)
            {
                CurDegreesSelected = DegreesSelected.On270Clockwise;
            }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public bool ShowWindow()
        {
            return BaseShowWindow();
        }

        private void RotateWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
            else if (e.Key == Key.Enter)
            {
                DialogResult = true;
            }
        }

        private void RotateView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void RotateView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
