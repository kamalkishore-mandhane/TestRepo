using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SafeShare.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public event EventHandler ValueChanged;

        public static readonly DependencyProperty NumericUpDownValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty NumericUpDownMinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty NumericUpDownMaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(365));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.NumTextBox.Text = numericUpDown.Value.ToString();
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

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > MinValue)
            {
                Value -= 1;
            }
        }

        private void ButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (Value < MaxValue)
            {
                Value += 1;
            }
        }

        private void NumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNumeric((sender as TextBox).Text))
            {
                var value = Convert.ToInt32((sender as TextBox).Text);
                if (MinValue <= value && value <= MaxValue)
                {
                    Value = value;
                }
                else
                {
                    if (value > MaxValue)
                    {
                        value = MaxValue;
                    }

                    if (value < MinValue)
                    {
                        value = MinValue;
                    }
                }

                (sender as TextBox).Text = value.ToString();
            }
        }

        private bool IsNumeric(string input)
        {
            if (input.Equals(string.Empty))
            {
                return false;
            }

            foreach (char ch in input)
            {
                if (ch < '0' || ch > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private void NumTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumeric(e.Text))
            {
                e.Handled = true;
            }
        }
    }
}