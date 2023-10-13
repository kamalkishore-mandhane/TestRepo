using System.Windows.Controls;
using System.Windows.Input;

namespace SafeShare.WPFUI.Controls
{
    internal class SpaceCheckBox : CheckBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                IsChecked = !IsChecked;
            }
        }
    }
}