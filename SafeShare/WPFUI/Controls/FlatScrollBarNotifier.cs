namespace SafeShare.WPFUI.Controls
{
    public delegate void ModeChanged(bool autoHide);

    public interface ISlideScrollBarNotifier
    {
        event ModeChanged ModeChanged;

        void SetMode(bool autoHide);
    }
}