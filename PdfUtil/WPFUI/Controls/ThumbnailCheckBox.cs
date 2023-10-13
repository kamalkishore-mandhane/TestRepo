using System.Windows.Controls;
using System.Windows.Input;

namespace PdfUtil.WPFUI.Controls
{
    class ThumbnailCheckBox : CheckBox
    {
        public ThumbnailCheckBox()
        {
            Focusable = false;
            ClickMode = ClickMode.Press;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        }

        protected override void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.Tap)
            {
                e.Handled = true;
            }
            else
            {
                base.OnStylusSystemGesture(e);
            }
        }
    }
}

