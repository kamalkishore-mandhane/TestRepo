using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using SBkUpUI.Utils;
using SBkUpUI.WPFUI.Utils;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SBkUp
{
    class RegeditOperation
    {
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

        public static string GetRegKey(UIntPtr inHive, string inKeyName, string inPropertyName, RegSAM in32or64key)
        {
            int hkey = 0;
            string value = string.Empty;
            try
            {
                uint lResult = NativeMethods.RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                if (0 == lResult)
                {
                    uint lpType = 0;
                    uint lpcbData = 1024;
                    StringBuilder stringBuilder = new StringBuilder((int)lpcbData);
                    NativeMethods.RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, stringBuilder, ref lpcbData);
                    value = stringBuilder.ToString();
                }
                return value;
            }
            finally
            {
                if (0 != hkey)
                {
                    NativeMethods.RegCloseKey(hkey);
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

        private const string WzKey = @"SOFTWARE\WinZip Computing\WinZip\WinZip";
        private const string ExeBitsValueKey = "ExeBits";
        private const string WinZipLangsRegistryKey = @"SOFTWARE\WinZip Computing\WinZip\Langs";
        private const string WinZipLangsValueKey = "InstalledUILangID";
        private const int EnglishLangID = 1033;
        private const string WzDisableSbkupUtilKey = "DisableSbkupUtil";
        private const string WzPolicyKey = @"Software\WinZip Computing\WinZip\Policies";

        public static bool IsWinZip64Exe()
        {
            bool isWinZip64Exe = false;
            string exeBits = string.Empty;
            // Look in HKCU first for ExeBits
            exeBits = GetRegKey64(HKEY_CURRENT_USER, WzKey, ExeBitsValueKey);
            if (string.IsNullOrEmpty(exeBits))
            {
                exeBits = GetRegKey32(HKEY_CURRENT_USER, WzKey, ExeBitsValueKey);
            }

            if (string.IsNullOrEmpty(exeBits))
            {
                // Look in HKCM first for ExeBits
                exeBits = GetRegKey64(HKEY_LOCAL_MACHINE, WzKey, ExeBitsValueKey);
                if (string.IsNullOrEmpty(exeBits))
                {
                    exeBits = GetRegKey32(HKEY_LOCAL_MACHINE, WzKey, ExeBitsValueKey);
                }
            }

            if (exeBits == "64")
                isWinZip64Exe = true;
            return isWinZip64Exe;
        }

        public static bool IsAppletEnabled()
        {
            bool ret = false;
#if WZ_APPX
            string value = UWPHelper.GetOemPoliciesValue(WzDisableSbkupUtilKey);
            if (!string.IsNullOrEmpty(value))
            {
                return value == "0";
            }
#endif
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    ret = (regKey.GetValue(WzDisableSbkupUtilKey) as string) == "0";
                }
            }
            return ret;
        }

        public static int GetDefaultUILangId(UIntPtr inHive)
        {
            int langId = 0;
            var value = string.Empty;
            if (IsWinZip64Exe())
            {
                value = GetRegKey64(inHive, WinZipLangsRegistryKey, WinZipLangsValueKey);
            }
            else
            {
                value = GetRegKey32(inHive, WinZipLangsRegistryKey, WinZipLangsValueKey);
            }
            if (!string.IsNullOrEmpty(value))
            {
                int.TryParse(value, out langId);
            }
            return langId;
        }

        public static bool IsInstalledLanguage(string languageId)
        {
            bool isInstall = false;
            var langName = string.Empty;
            if (IsWinZip64Exe())
            {
                langName = GetRegKey64(HKEY_LOCAL_MACHINE, WinZipLangsRegistryKey, languageId);
            }
            else
            {
                langName = GetRegKey32(HKEY_LOCAL_MACHINE, WinZipLangsRegistryKey, languageId);
            }
            if (!string.IsNullOrEmpty(langName))
            {
                isInstall = true;
            }
            return isInstall;
        }

        public static int GetWinZipInstalledUILangID()
        {
            int langId = 0;
            bool fIsRequestedLangInstalled = false;

            // Look in HKCU first for a LangID
            langId = GetDefaultUILangId(HKEY_CURRENT_USER);
            if (langId != 0)
            {
                fIsRequestedLangInstalled = IsInstalledLanguage(langId.ToString());
                if (!fIsRequestedLangInstalled)
                    langId = 0;
            }

            // If not found, look in HKLM
            if (langId == 0)
            {
                langId = GetDefaultUILangId(HKEY_LOCAL_MACHINE);
                if (langId != 0)
                {
                    fIsRequestedLangInstalled = IsInstalledLanguage(langId.ToString());
                    if (!fIsRequestedLangInstalled)
                        langId = 0;
                }
            }

            // If still no valid language check the current users language
            // before we fallback to ENUS
            if (langId == 0)
            {
                langId = Thread.CurrentThread.CurrentUICulture.LCID; // Get the current UI language
                if (langId != EnglishLangID)
                {
                    // Make sure the language is installed
                    fIsRequestedLangInstalled = IsInstalledLanguage(langId.ToString());
                    if (!fIsRequestedLangInstalled)
                        langId = 0;
                }
            }

            // If no language is set then fallback to English US
            if (langId == 0)
            {
                langId = EnglishLangID;
            }
            else
            {
                // Make sure the langId is a valid culture identifier
                try
                {
                    CultureInfo.GetCultureInfo(langId);
                }
                catch
                {
                    langId = EnglishLangID;
                }
            }
            return langId;
        }
    }
}
