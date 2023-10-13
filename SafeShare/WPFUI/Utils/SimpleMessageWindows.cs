using SafeShare.WPFUI.Controls;

namespace SafeShare.WPFUI.Utils
{
    internal static class SimpleMessageWindows
    {
        public static bool DisplayWarningConfirmationMessage(string content)
        {
            return SimpleMessageBox.Show(content, true);
        }

        public static bool DisplayCloseAppWarningMessage()
        {
            return SimpleMessageBox.Show(string.Empty, false);
        }

        public static bool DisplayWarningMessage(string content)
        {
            return SimpleMessageBox.Show(content, false);
        }
    }
}