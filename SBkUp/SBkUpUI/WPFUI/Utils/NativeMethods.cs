using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SBkUpUI.WPFUI.Utils
{
    public static class NativeMethods
    {
        public const int GW_OWNER = 4;

        public const int GWL_STYLE = -16;
        public const int WS_MINIMIZEBOX = 0x20000; //minimize button
        public const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        public const uint EVENT_MODIFY_STATE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int WM_SETICON = 0x0080;
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int WM_COMMAND = 0x0111;

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        #region Wait message pump
        public const int PM_REMOVE = 1;
        public const int QS_ALLINPUT = 0x1cff;
        public const int WM_QUIT = 0x12;
        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYLAST = 0x109;
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSELAST = 0x020E;
        public const int WAIT_TIMEOUT = 258;

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        public enum RegSAM
        {
            QueryValue = 0x0001,
            SetValue = 0x0002,
            CreateSubKey = 0x0004,
            EnumerateSubKeys = 0x0008,
            Notify = 0x0010,
            CreateLink = 0x0020,
            WOW64_32Key = 0x0200,
            WOW64_64Key = 0x0100,
            WOW64_Res = 0x0300,
            Read = 0x00020019,
            Write = 0x00020006,
            Execute = 0x00020019,
            AllAccess = 0x000f003f
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr DispatchMessage([In] ref MSG msg);

        [DllImport("user32.dll")]
        public static extern int MsgWaitForMultipleObjectsEx(int count, [In] IntPtr[] handles, int milliseconds, int waitMask, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PeekMessage([Out] out MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int exitCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool TranslateMessage([In, Out] ref MSG msg);
        #endregion

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppId);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetCurrentProcessExplicitAppUserModelID([Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegCloseKey(int hKey);

        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
        public static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

        public static string GetRegKey(UIntPtr inHive, string inKeyName, string inPropertyName, RegSAM in32or64key)
        {
            int hkey = 0;
            string value = string.Empty;
            try
            {
                uint lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                if (0 == lResult)
                {
                    uint lpType = 0;
                    uint lpcbData = 1024;
                    StringBuilder stringBuilder = new StringBuilder((int)lpcbData);
                    RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, stringBuilder, ref lpcbData);
                    value = stringBuilder.ToString();
                }
                return value;
            }
            finally
            {
                if (0 != hkey)
                {
                    RegCloseKey(hkey);
                }
            }
        }

        public static string GetRegKey64(UIntPtr inHive, String inKeyName, string inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, inPropertyName, RegSAM.WOW64_64Key);
        }

        public static string GetRegKey32(UIntPtr inHive, string inKeyName, string inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, inPropertyName, RegSAM.WOW64_32Key);
        }
    }


    public class MainWindowFinder
    {
        IntPtr bestHandle;
        int processId;

        public IntPtr FindMainWindow(int processId)
        {
            bestHandle = (IntPtr)0;
            this.processId = processId;

            NativeMethods.EnumThreadWindowsCallback callback = new NativeMethods.EnumThreadWindowsCallback(this.EnumWindowsCallback);
            NativeMethods.EnumWindows(callback, IntPtr.Zero);

            GC.KeepAlive(callback);
            return bestHandle;
        }

        bool IsMainWindow(IntPtr handle)
        {

            if (NativeMethods.GetWindow(new HandleRef(this, handle), NativeMethods.GW_OWNER) != (IntPtr)0 || !NativeMethods.IsWindowVisible(new HandleRef(this, handle).Handle))
            {
                return false;
            }

            return true;
        }

        bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            int processId;
            NativeMethods.GetWindowThreadProcessId(new HandleRef(this, handle), out processId);
            if (processId == this.processId)
            {
                if (IsMainWindow(handle))
                {
                    bestHandle = handle;
                    return false;
                }
            }
            return true;
        }
    }
}
