using System;
using System.Windows.Forms;

using PdfUtil.WPFUI.Utils;

namespace PdfUtil.WPFUI.Controls
{
    class WzFontDialog : FontDialog
    {
        private const int WM_INITDIALOG = 0x0110;
        private const int STRIKEOUT = 1040;
        private const int UNDERLINE = 1041;
        private const int SCRIPT = 1140;

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == WM_INITDIALOG)
            {
                DisableItem(hWnd, STRIKEOUT);
                DisableItem(hWnd, UNDERLINE);
                DisableItem(hWnd, SCRIPT);
            }
            return base.HookProc(hWnd, msg, wparam, lparam);
        }

        private void DisableItem(IntPtr hWnd, int item)
        {
            IntPtr hItem = NativeMethods.GetDlgItem(hWnd, item);
            if (hItem != IntPtr.Zero)
            {
                NativeMethods.EnableWindow(hItem, false);
            }
        }
    }
}
