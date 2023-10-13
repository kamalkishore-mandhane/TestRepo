using PdfUtil.WPFUI.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

using RECT = PdfUtil.WPFUI.Utils.NativeMethods.RECT;
using POINT = PdfUtil.WPFUI.Utils.NativeMethods.POINT;

namespace PdfUtil.WPFUI.Controls
{
    public class ColorDialogEx : ColorDialog
    {
        private bool _isBackgroundColor;
        private bool _isRemoveBackgroudColor;

        private Panel _radioButtonPanel = new Panel();
        private RadioButton _setBackgroundColor = new RadioButton() { Text = Properties.Resources.COLOR_DIALOG_SETBACKGROUNDCOLOR };
        private RadioButton _removeBackgroundColor = new RadioButton() { Text = Properties.Resources.COLOR_DIALOG_REMOVEBACKGROUNDCOLOR };

        private string OKText = Properties.Resources.COLOR_DIALOG_OK;
        private string CancelText = Properties.Resources.COLOR_DIALOG_CANCEL;
        private string SolidText = Properties.Resources.COLOR_DIALOG_SOLID;

        public bool IsRemoveBackgroudColor
        {
            get { return _isRemoveBackgroudColor; }
        }

        public ColorDialogEx(Color currentColor, bool isBackgroundColor = false, int[] customColor = null)
        {
            this.FullOpen = true;
            this.Color = currentColor;
            this.CustomColors = customColor;

            _isBackgroundColor = isBackgroundColor;
            _radioButtonPanel.Controls.Add(_setBackgroundColor);
            _radioButtonPanel.Controls.Add(_removeBackgroundColor);
        }

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == WM_INITDIALOG)
            {
                // Hide the define custom button
                HideControl(hWnd, CUSTOM_DEFINE_BUTTON, true);

                // Change inappropriate text
                SetControlText(hWnd, OK_BUTTON, OKText);
                SetControlText(hWnd, CANCEL_BUTTON, CancelText);
                SetControlText(hWnd, SOLID_TEXT, SolidText);

                if (_isBackgroundColor)
                {
                    _setBackgroundColor.Checked = true;
                    _setBackgroundColor.AutoSize = true;
                    _setBackgroundColor.Location = new Point(2, 0);
                    _removeBackgroundColor.AutoSize = true;
                    _removeBackgroundColor.Location = new Point(2, _setBackgroundColor.Height);

                    // Calculate radioButtonPanel size and parent window moving size
                    RECT rectCustomGrid = GetControlRect(hWnd, CUSTOM_GRID);
                    RECT rectCustomButton = GetControlRect(hWnd, CUSTOM_DEFINE_BUTTON);
                    RECT rectOKButton = GetControlRect(hWnd, OK_BUTTON);
                    int radioPanelWidth = rectCustomButton.right - rectCustomButton.left;
                    int radioPanelHeight = _setBackgroundColor.Height * 2;
                    int moveDownLength = (rectCustomButton.top - rectCustomGrid.bottom) * 2 + radioPanelHeight - (rectOKButton.top - rectCustomGrid.bottom);

                    // Resize the parent window
                    RECT rect = GetWindowRect(hWnd);
                    NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, rect.right - rect.left, rect.bottom - rect.top + moveDownLength, SWP_NOMOVE | SWP_NOZORDER);

                    // Move OK button, Cancel button and AddCustomColor button
                    MoveControl(hWnd, OK_BUTTON, 0, moveDownLength);
                    MoveControl(hWnd, CANCEL_BUTTON, 0, moveDownLength);
                    MoveControl(hWnd, CUSTOM_ADD_BUTTON, 0, moveDownLength);

                    // Add radioButtonPanel to parent window
                    NativeMethods.SetParent(_radioButtonPanel.Handle, hWnd);
                    POINT pointRadioPanel = new POINT() { X = rectCustomButton.left, Y = rectCustomButton.top - 2 };
                    NativeMethods.ScreenToClient(hWnd, ref pointRadioPanel);
                    NativeMethods.SetWindowPos(_radioButtonPanel.Handle, NativeMethods.GetDlgItem(hWnd, CUSTOM_GRID),
                        pointRadioPanel.X, pointRadioPanel.Y, radioPanelWidth, radioPanelHeight, 0);
                }
            }
            else if (msg == WM_SHOWWINDOW)
            {
                // Center the dialog on the parent window
                RECT rectParent = GetWindowRect(NativeMethods.GetParent(hWnd));
                RECT rectCurrent = GetWindowRect(hWnd);

                int x = rectParent.left + ((rectParent.right - rectParent.left) - (rectCurrent.right - rectCurrent.left)) / 2;
                int y = rectParent.top + ((rectParent.bottom - rectParent.top) - (rectCurrent.bottom - rectCurrent.top)) / 2;
                NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
            }
            else if (msg == WM_COMMAND)
            {
                if (wparam.ToInt32() == IDOK)
                {
                    // When click OK button, get the radio button status. 
                    if (_isBackgroundColor)
                    {
                        _isRemoveBackgroudColor = _removeBackgroundColor.Checked;
                    }
                }
            }

            return base.HookProc(hWnd, msg, wparam, lparam);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_radioButtonPanel != null)
                {
                    _radioButtonPanel.Dispose();
                }

                if (_setBackgroundColor != null)
                {
                    _setBackgroundColor.Dispose();
                }

                if (_removeBackgroundColor != null)
                {
                    _removeBackgroundColor.Dispose();
                }

                _radioButtonPanel = null;
                _setBackgroundColor = null;
                _removeBackgroundColor = null;
            }
        }

        private void SetControlText(IntPtr hWndParent, int idCtrl, string text)
        {
            IntPtr hWnd = NativeMethods.GetDlgItem(hWndParent, idCtrl);
            if (hWnd == null)
            {
                return;
            }
            NativeMethods.SetWindowText(hWnd, text);
        }

        private void MoveControl(IntPtr hWndParent, int idCtrl, int xIncrement, int yIncrement)
        {
            IntPtr hWnd = NativeMethods.GetDlgItem(hWndParent, idCtrl);
            if (hWnd == null)
            {
                return;
            }
            RECT rect = new RECT();
            NativeMethods.GetWindowRect(hWnd, out rect);
            POINT point = new POINT();
            point.X = rect.left;
            point.Y = rect.top;
            NativeMethods.ScreenToClient(hWndParent, ref point);
            NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, point.X + xIncrement, point.Y + yIncrement, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private RECT GetControlRect(IntPtr hWndParent, int idCtrl)
        {
            IntPtr hWnd = NativeMethods.GetDlgItem(hWndParent, idCtrl);
            RECT rect = new RECT();
            if (hWnd == null)
            {
                return rect;
            }
            NativeMethods.GetWindowRect(hWnd, out rect);
            return rect;
        }

        private RECT GetWindowRect(IntPtr hWndParent)
        {
            RECT rect = new RECT();
            if (hWndParent == null)
            {
                return rect;
            }
            NativeMethods.GetWindowRect(hWndParent, out rect);
            return rect;
        }

        private void HideControl(IntPtr hWndParent, int idCtrl, bool hideAndDisable = true)
        {
            IntPtr hWnd = NativeMethods.GetDlgItem(hWndParent, idCtrl);
            if (hWnd == null)
            {
                return;
            }
            NativeMethods.ShowWindow(hWnd, hideAndDisable ? SW_HIDE : SW_SHOW);
            NativeMethods.EnableWindow(hWnd, !hideAndDisable);
        }

        public DialogResult ShowWindow()
        {
            bool isMainWindowLoaded = System.Windows.Application.Current?.MainWindow.IsLoaded ?? false;
            bool oldEnableStatus = false;
            if (isMainWindowLoaded)
            {
                oldEnableStatus = System.Windows.Application.Current.MainWindow.IsEnabled;
                System.Windows.Application.Current.MainWindow.IsEnabled = false;
            }

            var ret = ShowDialog();

            if (isMainWindowLoaded)
            {
                System.Windows.Application.Current.MainWindow.IsEnabled = oldEnableStatus;
                System.Windows.Application.Current.MainWindow.Focus();
            }

            return ret;
        }

        public static bool IsChoosePresetColor(ColorDialogEx dialog)
        {
            foreach (var customColor in dialog.CustomColors)
            {
                if (ToInt(dialog.Color) == customColor)
                {
                    return false;
                }
            }
            return true;
        }

        private static int ToInt(Color c)
        {
            return c.R + c.G * 0x100 + c.B * 0x10000;
        }

        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const int WM_COMMAND = 0x111;
        private const int WM_INITDIALOG = 0x110;
        private const int WM_SHOWWINDOW = 0x18;
        private const int IDOK = 1;

        private const int OK_BUTTON = 0x0001;
        private const int CANCEL_BUTTON = 0x0002;
        private const int CUSTOM_GRID = 0x02d1;
        private const int CUSTOM_DEFINE_BUTTON = 0x02cf;
        private const int CUSTOM_ADD_BUTTON = 0x02c8;
        private const int SOLID_TEXT = 0x02db;
    }
}
