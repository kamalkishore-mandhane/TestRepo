using System;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Input;

namespace SBkUpUI.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for DatePicker.xaml
    /// </summary>
    public partial class DatePicker : BaseUserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime), typeof(DatePicker), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = (DatePicker)d;
            datePicker.wfDatePicker.Value = datePicker.Value;
            datePicker.dateTextBox.Text = datePicker.Value.ToString("d");
        }

        public DatePicker()
        {
            InitializeComponent();
            wfDatePicker.ValueChanged += WfDatePicker_ValueChanged;
        }

        private void WfDatePicker_ValueChanged(object sender, EventArgs e)
        {
            Value = (sender as DateTimePicker).Value;
        }

        public DateTime Value
        {
            get { return (DateTime)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(
            IntPtr hWnd, // handle to destination window
            Int32 msg, // message
            Int32 wParam, // first message parameter
            Int32 lParam // second message parameter
            );

        private const Int32 WM_SYSKEYDOWN = 0x104;
        private const Int32 WM_KILLFOCUS = 0x8;

        //method to call dropdown
        private void DateTime_Click(object sender, EventArgs e)
        {
            PostMessage(wfDatePicker.Handle, WM_SYSKEYDOWN, (int)Keys.Down, 0);
        }

        private void PickerButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime_Click(null, null);
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void DatePicker_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void DatePicker_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (pickerButton.IsFocused)
                    {
                        PostMessage(wfDatePicker.Handle, WM_KILLFOCUS, 0, 0);
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(pickerButton), null);
                        Keyboard.Focus(View.SBkUpView.MainWindow);
                        e.Handled = true;
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
