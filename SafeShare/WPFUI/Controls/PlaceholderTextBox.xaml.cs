using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SafeShare.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for PlaceholderTextBox.xaml
    /// </summary>
    public partial class PlaceholderTextBox : UserControl
    {
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderTextBox));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PlaceholderTextBox));
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(500));

        public PlaceholderTextBox()
        {
            InitializeComponent();
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value);}
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = ContentText.Text;
        }
    }
}
