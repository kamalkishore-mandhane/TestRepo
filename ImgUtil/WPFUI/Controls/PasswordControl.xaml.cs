using Microsoft.Win32;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImgUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for PasswordControl.xaml
    /// </summary>
    public partial class PasswordControl : BaseControl, INotifyPropertyChanged
    {
        public static DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(PasswordControl), new PropertyMetadata(""));
        public event PropertyChangedEventHandler PropertyChanged;

        public PasswordControl()
        {
            InitializeComponent();
        }

        public string Password
        {
            get
            {
                return (string)GetValue(PasswordProperty);
            }
            set
            {
                SetValue(PasswordProperty, value);
            }
        }


        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PasswordControl_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }

            PasswordBox.Password = Password;
        }

        private void passwordControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void SetPasswordBoxSelection(int start, int length)
        {
            PasswordBox.GetType()?.GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                                 ?.Invoke(PasswordBox, new object[] { start, length });
        }

        public void Clear()
        {
            PasswordBox.Clear();
        }

        public void FocusPasswordBox()
        {
            PasswordBox.Focus();
        }

        private void ImgShowHide_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PasswordBox.IsVisible)
            {
                ShowPassword();
            }
            else
            {
                HidePassword();
            }
        }

        void ShowPassword()
        {
            ImgShowHide.Source = Imaging.CreateBitmapSourceFromHIcon(Properties.Resources.EyeballDisable.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Hidden;
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Focus();
            PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length;
        }

        void HidePassword()
        {
            ImgShowHide.Source = Imaging.CreateBitmapSourceFromHIcon(Properties.Resources.EyeballEnable.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PasswordTextBox.Visibility = Visibility.Hidden;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Focus();
            SetPasswordBoxSelection(PasswordBox.Password.Length, 0);
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!PasswordBox.Password.Equals(PasswordTextBox.Text))
            {
                PasswordBox.Password = PasswordTextBox.Text;
                SetPasswordBoxSelection(PasswordTextBox.SelectionStart, PasswordTextBox.SelectionLength);
            }
        }

        private void PasswordTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ImgShowHide.Source = Imaging.CreateBitmapSourceFromHIcon(Properties.Resources.EyeballEnable.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PasswordTextBox.Visibility = Visibility.Hidden;
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            SetPasswordBoxSelection(PasswordTextBox.SelectionStart, PasswordTextBox.SelectionLength);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!PasswordBox.Password.Equals(PasswordTextBox.Text))
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length;
            }

            Password = PasswordBox.Password;
        }
    }
}
