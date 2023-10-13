using Microsoft.Win32;
using PdfUtil.WPFUI.Model;
using System.Windows;

namespace PdfUtil.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for CommentRectangle.xaml
    /// </summary>
    public partial class CommentRectangle : BaseUserControl
    {
        public CommentRectangle(CommentItem commentItem)
        {
            DataContext = commentItem;
            InitializeComponent();
        }

        private void CommentRectangleCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void CommentRectangleCtrl_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }
    }
}
