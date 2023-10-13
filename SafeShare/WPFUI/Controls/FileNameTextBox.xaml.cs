using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;

namespace SafeShare.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for FileNameTextBox.xaml
    /// </summary>
    public partial class FileNameTextBox : UserControl, INotifyPropertyChanged
    {
        private const int _fileNameMaxLen = 251;
        private readonly char[] _invalidChar = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        public event EventHandler ValueChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty TextFileNameProperty = DependencyProperty.Register("NameContent", typeof(string), typeof(FileNameTextBox), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty TipsInfoNameProperty = DependencyProperty.Register("TipsInfo", typeof(string), typeof(FileNameTextBox), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty TipsIsOpenProperty = DependencyProperty.Register("TipsIsOpen", typeof(bool), typeof(FileNameTextBox), new FrameworkPropertyMetadata(OnValuePropertyChanged) { BindsTwoWayByDefault = true });

        private static FieldInfo _menuDropAlignmentField;

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fileNameTextBox = (FileNameTextBox)d;
            fileNameTextBox.TextBox.Text = fileNameTextBox.NameContent;
            fileNameTextBox.LabelInfo.Content = fileNameTextBox.TipsInfo;
            fileNameTextBox.PopFileNameTextBox.IsOpen = fileNameTextBox.TipsIsOpen;
        }

        public FileNameTextBox()
        {
            InitializeComponent();
            DisableSystemMenuPopupAlignment();
        }

        /// <summary>
        /// Disable the system's menu pop-up orientation setting(HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows\MenuDropAlignment)
        /// Cancel the effect on the application's Popup pop-up orientation
        /// </summary>
        private void DisableSystemMenuPopupAlignment()
        {
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            System.Diagnostics.Debug.Assert(_menuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment)
            {
                _menuDropAlignmentField?.SetValue(null, false);
            }
        }

        [BindableAttribute(true)]
        public string NameContent
        {
            get { return (string)GetValue(TextFileNameProperty); }
            set
            {
                var oldValue = NameContent;
                SetValue(TextFileNameProperty, value);
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
        public bool TipsIsOpen
        {
            get { return (bool)GetValue(TipsIsOpenProperty); }
            set
            {
                var oldValue = TipsIsOpen;
                SetValue(TipsIsOpenProperty, value);
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
        public string TipsInfo
        {
            get { return (string)GetValue(TipsInfoNameProperty); }
            set
            {
                var oldValue = TipsInfo;
                SetValue(TipsInfoNameProperty, value);
                if (value != oldValue)
                {
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, null);
                    }
                }
            }
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NameContent = TextBox.Text;
            var fielName = string.Join(string.Empty, TextBox.Text.Split(_invalidChar));
            if (string.Compare(fielName, TextBox.Text, false) != 0)
            {
                PopFileNameTextBox.IsOpen = true;
                LabelInfo.Content = Properties.Resources.ILLEGAL_CHAR_MSG;
                TextBox.Text = fielName;
                TextBox.Select(TextBox.Text.Length, 0);
                e.Handled = true;
            }

            if (TextBox.Text.Length > _fileNameMaxLen)
            {
                PopFileNameTextBox.IsOpen = true;
                LabelInfo.Content = Properties.Resources.FILE_NAME_TOO_LONG;
                e.Handled = true;
            }
            else if (IsContainsIllegalChar(TextBox.Text))
            {
                PopFileNameTextBox.IsOpen = true;
                LabelInfo.Content = Properties.Resources.ILLEGAL_CHAR_MSG;
                e.Handled = true;
            }
            else
            {
                PopFileNameTextBox.IsOpen = false;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsContainsIllegalChar(e.Text))
            {
                PopFileNameTextBox.IsOpen = true;
                LabelInfo.Content = Properties.Resources.ILLEGAL_CHAR_MSG;
                e.Handled = true;
            }
            else if (TextBox.Text.Length > _fileNameMaxLen)
            {
                PopFileNameTextBox.IsOpen = true;
                LabelInfo.Content = Properties.Resources.FILE_NAME_TOO_LONG;
                e.Handled = true;
            }
            else
            {
                PopFileNameTextBox.IsOpen = false;
            }
        }

        private bool IsContainsIllegalChar(string str)
        {
            foreach (var item in _invalidChar)
            {
                if (str.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}