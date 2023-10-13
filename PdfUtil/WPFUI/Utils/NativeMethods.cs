using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfUtil.WPFUI.Utils
{
    static partial class NativeMethods
    {
        public const uint GA_ROOT = 2;
        public const int WM_USER = 0x0400;
        public const int GW_OWNER = 4;
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public const uint EVENT_MODIFY_STATE = 0x0002;

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int WM_SETICON = 0x0080;
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;
        public const int WM_COMMAND = 0x0111;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        [Flags]
        public enum TASKDIALOG_FLAGS
        {
            TDF_ENABLE_HYPERLINKS = 0x0001,
            TDF_USE_HICON_MAIN = 0x0002,
            TDF_USE_HICON_FOOTER = 0x0004,
            TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,
            TDF_USE_COMMAND_LINKS = 0x0010,
            TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,
            TDF_EXPAND_FOOTER_AREA = 0x0040,
            TDF_EXPANDED_BY_DEFAULT = 0x0080,
            TDF_VERIFICATION_FLAG_CHECKED = 0x0100,
            TDF_SHOW_PROGRESS_BAR = 0x0200,
            TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,
            TDF_CALLBACK_TIMER = 0x0800,
            TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,
            TDF_RTL_LAYOUT = 0x2000,
            TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,
            TDF_CAN_BE_MINIMIZED = 0x8000
        }

        public enum TASKDIALOG_ELEMENTS
        {
            TDE_CONTENT,
            TDE_EXPANDED_INFORMATION,
            TDE_FOOTER,
            TDE_MAIN_INSTRUCTION
        }

        public enum TASKDIALOG_ICON_ELEMENTS
        {
            TDIE_ICON_MAIN,
            TDIE_ICON_FOOTER
        }

        public enum TASKDIALOG_MESSAGES : uint
        {
            // Spec is not clear on what this is for.
            ////TDM_NAVIGATE_PAGE = WM_USER + 101,
            TDM_CLICK_BUTTON = WM_USER + 102,              // wParam = Button ID
            TDM_SET_MARQUEE_PROGRESS_BAR = WM_USER + 103,  // wParam = 0 (nonMarque) wParam != 0 (Marquee)
            TDM_SET_PROGRESS_BAR_STATE = WM_USER + 104,    // wParam = new progress state
            TDM_SET_PROGRESS_BAR_RANGE = WM_USER + 105,    // lParam = MAKELPARAM(nMinRange, nMaxRange)
            TDM_SET_PROGRESS_BAR_POS = WM_USER + 106,      // wParam = new position
            TDM_SET_PROGRESS_BAR_MARQUEE = WM_USER + 107,  // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
            TDM_SET_ELEMENT_TEXT = WM_USER + 108,          // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            TDM_CLICK_RADIO_BUTTON = WM_USER + 110,        // wParam = Radio Button ID
            TDM_ENABLE_BUTTON = WM_USER + 111,             // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
            TDM_ENABLE_RADIO_BUTTON = WM_USER + 112,       // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
            TDM_CLICK_VERIFICATION = WM_USER + 113,        // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
            TDM_UPDATE_ELEMENT_TEXT = WM_USER + 114,       // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = WM_USER + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
            TDM_UPDATE_ICON = WM_USER + 116                // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct TASKDIALOGCONFIG
        {
            public int size;
            public IntPtr parent;
            public IntPtr instance;
            public TASKDIALOG_FLAGS flags;
            public TASKDIALOG_COMMON_BUTTON_FLAGS commonButtonFlags;
            public string title;
            public IntPtr mainIcon;
            public string mainInstruction;
            public string content;
            public int buttonCount;
            public IntPtr buttons;
            public int defaultButton;
            public int radioButtonCount;
            public IntPtr radioButtons;
            public int defaultRadioButton;
            public string verificationText;
            public string expandedInformation;
            public string expandedControlText;
            public string collapsedControlText;
            public IntPtr footerIcon;
            public string footer;
            public TaskDialogCallbackProc callback;
            public IntPtr callbackData;
            public int width;
        }

        public enum TASKDIALOG_NOTIFICATIONS : uint
        {
            TDN_CREATED = 0,
            TDN_NAVIGATED = 1,
            TDN_BUTTON_CLICKED = 2,             // wParam = Button ID
            TDN_HYPERLINK_CLICKED = 3,          // lParam = (LPCWSTR)pszHREF
            TDN_TIMER = 4,                      // wParam = Milliseconds since dialog created or timer reset
            TDN_DESTROYED = 5,
            TDN_RADIO_BUTTON_CLICKED = 6,       // wParam = Radio Button ID
            TDN_DIALOG_CONSTRUCTED = 7,
            TDN_VERIFICATION_CLICKED = 8,       // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
            TDN_HELP = 9,
            TDN_EXPANDO_BUTTON_CLICKED = 10     // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
        }

        public delegate int TaskDialogCallbackProc(IntPtr hwnd, TASKDIALOG_NOTIFICATIONS msg, IntPtr wParam, IntPtr lParam, IntPtr refData);

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [DllImport("comctl32.dll")]
        public static extern int TaskDialogIndirect(ref TASKDIALOGCONFIG taskConfig, out int button, out int radioButton, out bool verificationFlagChecked);

        public const int SHGFI_USEFILEATTRIBUTES = 0x00000010;
        public const int SHGFI_LARGEICON = 0x00000000;
        public const int SHGFI_SMALLICON = 0x00000001;
        public const int SHGFI_ICON = 0x00000100;
        public const int SHGFI_DISPLAYNAME = 0x00000200;
        public const int SHGFI_SYSICONINDEX = 0x00004000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
            public IntPtr icon;
            public int iconIndex;
            public uint attributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string displayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string typeName;
        };

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string path, int fileAttributes, out SHFILEINFO fileInfo, int fileInfoSize, int flags);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr icon);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int type;
            public int width;
            public int height;
            public int widthBytes;
            public short planes;
            public short bitsPixel;
            public IntPtr bits;
        }

        [Flags]
        public enum AssocF : uint
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_FixedProgId = 0x800,
            IsProtocol = 0x1000,
            InitForFile = 0x2000,
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            SupportedUriProtocols,
            Max,
        }

        [DllImport("Gdi32.dll")]
        public static extern int GetObject(IntPtr hbm, int buffer, out BITMAP bitmap);

        [DllImport("Gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DeleteObject(IntPtr obj);

        [DllImport("comctl32")]
        public extern static int ImageList_GetIconSize(IntPtr himl, ref int cx, ref int cy);

        [DllImport("comctl32")]
        public extern static IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageListHandle(int iImageList, ref Guid riid, ref IntPtr handle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public const int VK_LMENU = 0xA4;
        public const int KEYEVENTF_EXTENDEDKEY = 0x1;
        public const int KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenEvent(uint lpEventAttributes, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool SetEvent(IntPtr hEvent);

        public const int CSIDL_COMMON_PROGRAMS = 0X0017;            // All Users\Start Menu\Programs
        public const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019;    // All Users\Desktop

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        public const int SHCNE_ASSOCCHANGED = 0x8000000;
        public const int SHCNF_FLUSH = 0x1000;

        [DllImport("Shell32.dll")]
        public static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppId);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetCurrentProcessExplicitAppUserModelID([Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID);

        public static string AssocQueryString(AssocStr association, string extension)
        {
            // get association information
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = AssocQueryString(AssocF.None, association, extension, null, null, ref length);
            if (ret != S_FALSE)
            {
                return string.Empty;
            }

            var sb = new StringBuilder((int)length);
            ret = AssocQueryString(AssocF.None, association, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                return string.Empty;
            }

            return sb.ToString();
        }

        #region Wait message pump
        public const int PM_REMOVE = 1;
        public const int QS_ALLINPUT = 0x1cff;
        public const int WM_QUIT = 0x12;
        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYLAST = 0x109;
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSELAST = 0x020E;

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

        #region Side by side load COM interface
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ACTCTX
        {
            public UInt32 cbSize;
            public UInt32 dwFlags;
            public string lpSource;
            // public UInt16 wProcessorArchitecture;
            // public UInt16 wLangId;
            // the not Used is for 32 bit and 64 bit work.
            //   The total size forwProcessorArchitecture and wLangId is 64 in x64 and 32 in x86
            //      which is the same behavier for IntPtr
            public IntPtr notUsed;
            public string lpAssemblyDirectory;
            public IntPtr lpResourceName;
            public string lpApplicationName;
            public IntPtr hModule;
        }
        public const uint ACTCTX_FLAG_PROCESSOR_ARCHITECTURE_VALID = 0x001;
        public const uint ACTCTX_FLAG_LANGID_VALID = 0x002;
        public const uint ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0x004;
        public const uint ACTCTX_FLAG_RESOURCE_NAME_VALID = 0x008;
        public const uint ACTCTX_FLAG_SET_PROCESS_DEFAULT = 0x010;
        public const uint ACTCTX_FLAG_APPLICATION_NAME_VALID = 0x020;
        public const uint ACTCTX_FLAG_HMODULE_VALID = 0x080;

        public const UInt16 RT_MANIFEST = 24;
        public const UInt16 CREATEPROCESS_MANIFEST_RESOURCE_ID = 1;
        public const UInt16 ISOLATIONAWARE_MANIFEST_RESOURCE_ID = 2;
        public const UInt16 ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID = 3;


        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint CLSCTX_LOCAL_SERVER = 4;

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateActCtxW(ref ACTCTX pActCtx);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeactivateActCtx(int dwFlags, IntPtr lpCookie);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern void ReleaseActCtx(IntPtr hActCtx);

        [DllImport("ole32.Dll")]
        static public extern uint CoCreateInstance(ref Guid clsid,
           [MarshalAs(UnmanagedType.IUnknown)] object inner,
           uint context,
           ref Guid uuid,
           [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

        public static bool CreateComObject(Guid classGuid, Guid interfaceGuid, out object instance, out string sErr)
        {

            instance = null;

            uint hResult = CoCreateInstance(ref classGuid, null,
                           CLSCTX_INPROC_SERVER, ref interfaceGuid, out instance);


            // Some error codes. See 'winerror.h for more, and use the following to convert the debug value to Hex: http://www.rapidtables.com/convert/number/decimal-to-hex.htm

            const uint S_OK = 0x00000000;       //Operation successful
            const uint E_NOTIMPL = 0x80004001;       //Not implemented
            const uint E_NOINTERFACE = 0x80004002;       //No such interface supported
            const uint E_POINTER = 0x80004003;       //Pointer that is not valid
            const uint E_ABORT = 0x80004004;       //Operation aborted
            const uint E_FAIL = 0x80004005;       //Unspecified failure
            const uint E_UNEXPECTED = 0x8000FFFF;       //Unexpected failure
            const uint E_ACCESSDENIED = 0x80070005;       //General access denied error
            const uint E_HANDLE = 0x80070006;       //Handle that is not valid
            const uint E_OUTOFMEMORY = 0x8007000E;       //Failed to allocate necessary memory
            const uint E_INVALIDARG = 0x80070057;       //One or more arguments are not valid

            const uint E_CLASSNOTREG = 0x80040154;      // Class not registered

            sErr = "";
            switch (hResult)
            {
                case S_OK:
                    sErr = "";
                    break;
                case E_NOTIMPL:
                    sErr = "E_NOTIMPL: Not implemented";
                    break;
                case E_NOINTERFACE:
                    sErr = "E_NOINTERFACE: No such interface supported";
                    break;
                case E_POINTER:
                    sErr = "E_POINTER: Pointer that is not valid";
                    break;
                case E_ABORT:
                    sErr = "E_ABORT: Operation aborted";
                    break;
                case E_FAIL:
                    sErr = "E_FAIL: Unspecified failure";
                    break;
                case E_UNEXPECTED:
                    sErr = "E_UNEXPECTED: Unexpected failure";
                    break;
                case E_ACCESSDENIED:
                    sErr = "E_ACCESSDENIED: General access denied error";
                    break;
                case E_HANDLE:
                    sErr = "E_HANDLE: Handle that is not valid";
                    break;
                case E_OUTOFMEMORY:
                    sErr = "E_OUTOFMEMORY: Failed to allocate necessary memory";
                    break;
                case E_INVALIDARG:
                    sErr = "E_INVALIDARG: One or more arguments are not valid";
                    break;

                case E_CLASSNOTREG:
                    sErr = "E_CLASSNOTREG: Class not registered";
                    break;
            }

            return hResult == 0;

        }
        #endregion

        #region Reg key operation

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

        [Flags]
        public enum EFileAccess : uint
        {
            //
            // Standart Section
            //

            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,     // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,        // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,       // file & pipe
            FILE_ADD_FILE = 0x0002,         // directory
            FILE_APPEND_DATA = 0x0004,      // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,          // file & directory
            FILE_WRITE_EA = 0x0010,         // file & directory
            FILE_EXECUTE = 0x0020,          // file
            FILE_TRAVERSE = 0x0020,         // directory
            FILE_DELETE_CHILD = 0x0040,     // directory
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            ///
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access.
            /// Otherwise, other processes cannot open the object if they request read access.
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access.
            /// Otherwise, other processes cannot open the object if they request write access.
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access.
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always.
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always.
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000,
            FlagWriteThrough = 0x80000000
        }

        [DllImport("advapi32.dll")]
        public static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

        [DllImport("advapi32.dll")]
        public static extern uint RegCloseKey(int hKey);

        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
        public static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

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
        #endregion
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
