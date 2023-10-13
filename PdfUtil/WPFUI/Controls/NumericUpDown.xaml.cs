using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : BaseUserControl
    {
        public event EventHandler ValueChanged;
        public static readonly DependencyProperty NumericUpDownValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty NumericUpDownMinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty NumericUpDownMaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(365));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.TextBox.Text = numericUpDown.Value.ToString();
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }

        [BindableAttribute(true)]
        public int Value
        {
            get { return (int)GetValue(NumericUpDownValueProperty); }
            set
            {
                var oldValue = Value;
                SetValue(NumericUpDownValueProperty, value);
                if (value != oldValue)
                {
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, null);
                    }
                }
            }
        }

        [BindableAttribute(true)]
        public int MaxValue
        {
            get { return (int)GetValue(NumericUpDownMaxValueProperty); }
            set { SetValue(NumericUpDownMaxValueProperty, value); }
        }

        [BindableAttribute(true)]
        public int MinValue
        {
            get { return (int)GetValue(NumericUpDownMinValueProperty); }
            set { SetValue(NumericUpDownMinValueProperty, value); }
        }

        private void ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (Value < MaxValue)
            {
                Value += 1;
            }
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > MinValue)
            {
                Value -= 1;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonUP, new object[] { true });
            }

            if (e.Key == Key.Down)
            {
                ButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonDown, new object[] { true });
            }
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonUP, new object[] { false });
            }

            if (e.Key == Key.Down)
            {
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ButtonDown, new object[] { false });
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var value = 0;
            if (IsTextAllowed(TextBox.Text, ref value))
            {
                Value = value;
            }
            else
            {
                TextBox.TextChanged -= TextBox_TextChanged;
                TextBox.Text = Value.ToString();
                TextBox.TextChanged += TextBox_TextChanged;
                TextBox.SelectAll();
            }
        }

        private bool IsTextAllowed(string inputText, ref int value)
        {
            foreach (char c in inputText.ToCharArray())
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            if (inputText.StartsWith("0"))
            {
                return false;
            }

            if (int.TryParse(inputText, out value))
            {
                if (value >= MinValue && value <= MaxValue)
                {
                    return true;
                }
            }
            return false;
        }

        private void NumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void NumericUpDown_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
