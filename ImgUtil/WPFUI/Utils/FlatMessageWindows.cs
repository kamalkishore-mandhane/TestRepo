using ImgUtil.WPFUI.Controls;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgUtil.WPFUI.Utils
{
    static class FlatMessageWindows
    {
        public static bool DisplayWarningMessage(IntPtr owner, string message)
        {
            ImageSource warning = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Warning.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return FlatMessageBox.ShowDialog(owner, message, warning, Properties.Resources.IDENTIFYKEY_OK, null, false);
        }

        public static bool DisplayInformationMessage(IntPtr owner, string message)
        {
            ImageSource info = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Information.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return FlatMessageBox.ShowDialog(owner, message, info, Properties.Resources.IDENTIFYKEY_OK, null, false);
        }

        public static bool DisplayConfirmationMessage(IntPtr owner, string message)
        {
            ImageSource question = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Question.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return FlatMessageBox.ShowDialog(owner, message, question);
        }

        public static bool DisplayErrorMessage(IntPtr owner, string message)
        {
            ImageSource error = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return FlatMessageBox.ShowDialog(owner, message, error, Properties.Resources.IDENTIFYKEY_OK, null, false);
        }
    }
}
