using Aspose.Imaging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ImgUtil.WPFUI.Utils
{
    public static partial class NativeMethods
    {
        public const int WM_COMMAND = 0x0111;
        public const uint GA_ROOT = 2;
        public const int GW_OWNER = 4;
        public static Dictionary<FileFormat, string> SaveAsPickerFilterDic = new Dictionary<FileFormat, string>()
        {
            { FileFormat.Bmp,      Properties.Resources.FILE_FORMAT_BMP},
            { FileFormat.Gif,      Properties.Resources.FILE_FORMAT_GIF},
            { FileFormat.Jpeg2000, Properties.Resources.FILE_FORMAT_JPG2000},
            { FileFormat.Jpeg,     Properties.Resources.FILE_FORMAT_JPEG},
            { FileFormat.Png,      Properties.Resources.FILE_FORMAT_PNG},
            { FileFormat.Psd,      Properties.Resources.FILE_FORMAT_PSD},
            { FileFormat.Svg,      Properties.Resources.FILE_FORMAT_SVG},
            { FileFormat.Tiff,     Properties.Resources.FILE_FORMAT_TIF},
            { FileFormat.Webp,     Properties.Resources.FILE_FORMAT_WEBP}
        };

        public const int GWL_STYLE = -16;
        public const int WS_MINIMIZEBOX = 0x20000; //minimize button
        public const int WS_MAXIMIZEBOX = 0x10000; //maximize button

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

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

        public const int PM_REMOVE = 1;
        public const int QS_ALLINPUT = 0x1cff;
        public const int WM_QUIT = 0x12;
        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYLAST = 0x109;
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSELAST = 0x020E;

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

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenEvent(uint lpEventAttributes, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("comctl32.dll")]
        public static extern int TaskDialogIndirect(ref TASKDIALOGCONFIG taskConfig, out int button, out int radioButton, out bool verificationFlagChecked);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PeekMessage([Out] out MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int exitCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool TranslateMessage([In, Out] ref MSG msg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr DispatchMessage([In] ref MSG msg);

        [DllImport("user32.dll")]
        public static extern int MsgWaitForMultipleObjectsEx(int count, [In] IntPtr[] handles, int milliseconds, int waitMask, int flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        public const int CSIDL_COMMON_PROGRAMS = 0X0017;            // All Users\Start Menu\Programs
        public const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019;    // All Users\Desktop

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

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

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        public const int SHCNE_ASSOCCHANGED = 0x08000000;

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppId);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetCurrentProcessExplicitAppUserModelID([Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID);

        [DllImport("UxTheme.dll")]
        public static extern int BeginPanningFeedback(IntPtr hwnd);

        [DllImport("UxTheme.dll")]
        public static extern int EndPanningFeedback(IntPtr hwnd, bool animateBack);

        [DllImport("UxTheme.dll")]
        public static extern int UpdatePanningFeedback(IntPtr hwnd, long offsetX, long offsetY, bool inInertia);

        public const int VK_LMENU = 0xA4;
        public const int KEYEVENTF_EXTENDEDKEY = 0x1;
        public const int KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

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
            public IntPtr callback;
            public IntPtr callbackData;
            public int width;
        }

        public enum MessageId
        {
            ID_REFRESH_UXNAG_BANNER = 13022,
        }

        #region Reg key operation

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

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
        public enum OPEN_AS_INFO_FLAGS
        {
            OAIF_ALLOW_REGISTRATION = 0x00000001,   // Show "Always" checkbox
            OAIF_REGISTER_EXT = 0x00000002,   // Perform registration when user hits OK
            OAIF_EXEC = 0x00000004,   // Exec file after registering
            OAIF_FORCE_REGISTRATION = 0x00000008,   // Force the checkbox to be registration
            OAIF_HIDE_REGISTRATION = 0x00000020,   // Vista+: Hide the "always use this file" checkbox
            OAIF_URL_PROTOCOL = 0x00000040,   // Vista+: cszFile is actually a URI scheme; show handlers for that scheme
            OAIF_FILE_IS_URI = 0x00000080    // Win8+: The location pointed to by the pcszFile parameter is given as a URI
        }

        public struct OPENASINFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string cszFile;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string cszClass;

            [MarshalAs(UnmanagedType.I4)]
            public OPEN_AS_INFO_FLAGS oaifInFlags;
        }

        [DllImport("shell32.dll", EntryPoint = "SHOpenWithDialog", CharSet = CharSet.Unicode)]
        public static extern int SHOpenWithDialog(IntPtr hWndParent, ref OPENASINFO openAsInfo);

        public const uint SW_NORMAL = 1;
        public const uint SEE_MASK_INVOKEIDLIST = 12;

        [Serializable]
        public struct ShellExecuteInfo
        {
            public int Size;
            public uint Mask;
            public IntPtr hwnd;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public uint Show;
            public IntPtr InstApp;
            public IntPtr IDList;
            public string Class;
            public IntPtr hkeyClass;
            public uint HotKey;
            public IntPtr Icon;
            public IntPtr Monitor;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

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

        #region File Icon

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

        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IImageList
        {
            [PreserveSig]
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

            [PreserveSig]
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);

            [PreserveSig]
            int SetOverlayImage(int iImage, int iOverlay);

            [PreserveSig]
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

            [PreserveSig]
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(int i);

            [PreserveSig]
            int GetIcon(int i, int flags, ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

            [PreserveSig]
            int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int Clone(ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(int i, ref NativeMethods.RECT prc);

            [PreserveSig]
            int GetIconSize(ref int cx, ref int cy);

            [PreserveSig]
            int SetIconSize(int cx, int cy);

            [PreserveSig]
            int GetImageCount(ref int pi);

            [PreserveSig]
            int SetImageCount(int uNewCount);

            [PreserveSig]
            int SetBkColor(int clrBk, ref int pclr);

            [PreserveSig]
            int GetBkColor(ref int pclr);

            [PreserveSig]
            int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(IntPtr hwndLock, int x, int y);

            [PreserveSig]
            int DragLeave(IntPtr hwndLock);

            [PreserveSig]
            int DragMove(int x, int y);

            [PreserveSig]
            int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int DragShowNolock(int fShow);

            [PreserveSig]
            int GetDragImage(ref NativeMethods.POINT ppt, ref NativeMethods.POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(int i, ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(int iOverlay, ref int piIndex);
        };

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string path, int fileAttributes, out SHFILEINFO fileInfo, int fileInfoSize, int flags);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr icon);

        [DllImport("comctl32")]
        public extern static int ImageList_GetIconSize(IntPtr himl, ref int cx, ref int cy);

        [DllImport("comctl32")]
        public extern static IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageListHandle(int iImageList, ref Guid riid, ref IntPtr handle);

        [DllImport("Gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DeleteObject(IntPtr obj);

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
