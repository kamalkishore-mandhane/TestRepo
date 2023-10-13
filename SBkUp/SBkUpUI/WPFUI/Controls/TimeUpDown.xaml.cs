using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reflection;
using Microsoft.Win32;

namespace SBkUpUI.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for TimeUpDown.xaml
    /// </summary>
    public partial class TimeUpDown : BaseUserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime), typeof(TimeUpDown), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeUpDown = (TimeUpDown)d;
            timeUpDown.TextBox.Text = timeUpDown.Value.ToLongTimeString();
        }

        public TimeUpDown()
        {
            InitializeComponent();
        }

        public DateTime Value
        {
            get { return (DateTime)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            Value = Value.AddMinutes(30.0);
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            Value = Value.AddMinutes(-30.0);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                ButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonUP, new object[] { true });
                e.Handled = true;
            }


            if (e.Key == Key.Down)
            {
                ButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonDown, new object[] { true });
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonUP, new object[] { false });
                e.Handled = true;
            }

            if (e.Key == Key.Down)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonDown, new object[] { false });
                e.Handled = true;
            }
        }

        private void TimeUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void TimeUpDown_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
