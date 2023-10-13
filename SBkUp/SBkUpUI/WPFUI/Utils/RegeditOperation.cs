using Microsoft.Win32;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SBkUpUI.Utils;

namespace SBkUpUI.WPFUI.Utils
{
    class RegeditOperation
    {
        private const string WzKey = @"SOFTWARE\WinZip Computing\WinZip\WinZip";
        private const string WzAdminConfigKey = @"SOFTWARE\WinZip Computing\WinZip\admcfgimage";
        private const string Wzxat = @"Software\WinZip Computing\WinZip";
        private const string ExeBitsValueKey = "ExeBits";
        private const string VersionValueKey = "Version";
        private const string WinZipLangsRegistryKey = @"SOFTWARE\WinZip Computing\WinZip\Langs";
        private const string WinZipLangsValueKey = "InstalledUILangID";
        private const int EnglishLangID = 1033;
        private const string WzPolicyKey = @"Software\WinZip Computing\WinZip\Policies";
        private const string WzDisableImgUtilKey = "DisableImgUtil";
        private const string WzDisableShareA = "DisableShareA";
        public const string WzAddDesktopIconKey = "AddDesktopIcon";
        public const string WzEnterpriseKey = "Enterprise";
        public const string WzxatKey = "x-at";
        public const string WzAddStartMenuKey = "AddStartMenu";
        public const string WzAddFileAssociationKey = "AddFileAssociation";
        public const string WzImageFmKey = @"SOFTWARE\WinZip Computing\ImageManager\fm";
        public const string WzFileExtOpenWithProgId = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\OpenWithProgids";
        public const string WzFileExtUserChoice = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\UserChoice";
        private const string WzSuiteKey = @"SOFTWARE\WinZip Computing\WinZip\Suite";
        private const string WzSubsLastCheck = "slc";
        private const string WzSubsStatus = "sbs";
        private const string WzSubsDeaddate = "sed";

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

        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern uint RegCloseKey(int hKey);

        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

        public static bool IsWinZip64Exe()
        {
            bool isWinZip64Exe = false;
            string exeBits = string.Empty;
            // Look in HKCU first for ExeBits
            exeBits = NativeMethods.GetRegKey64(NativeMethods.HKEY_CURRENT_USER, WzKey, ExeBitsValueKey);
            if (string.IsNullOrEmpty(exeBits))
            {
                exeBits = NativeMethods.GetRegKey32(NativeMethods.HKEY_CURRENT_USER, WzKey, ExeBitsValueKey);
            }

            if (string.IsNullOrEmpty(exeBits))
            {
                // Look in HKCM first for ExeBits
                exeBits = NativeMethods.GetRegKey64(NativeMethods.HKEY_LOCAL_MACHINE, WzKey, ExeBitsValueKey);
                if (string.IsNullOrEmpty(exeBits))
                {
                    exeBits = NativeMethods.GetRegKey32(NativeMethods.HKEY_LOCAL_MACHINE, WzKey, ExeBitsValueKey);
                }
            }

            if (exeBits == "64")
                isWinZip64Exe = true;
            return isWinZip64Exe;
        }

        public static string GetWinZipVersion()
        {
            string winZipVersion = NativeMethods.GetRegKey64(NativeMethods.HKEY_LOCAL_MACHINE, WzKey, VersionValueKey);
            if (string.IsNullOrEmpty(winZipVersion))
            {
                winZipVersion = NativeMethods.GetRegKey32(NativeMethods.HKEY_LOCAL_MACHINE, WzKey, VersionValueKey);
            }

            return winZipVersion;
        }

        public static bool IsAppletEnabled()
        {
            bool ret = false;
#if WZ_APPX
            string value = UWPHelper.GetOemPoliciesValue(WzDisableImgUtilKey);
            if (!string.IsNullOrEmpty(value))
            {
                return value == "0";
            }
#endif
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    ret = (regKey.GetValue(WzDisableImgUtilKey) as string) == "0";
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
                value = NativeMethods.GetRegKey64(inHive, WinZipLangsRegistryKey, WinZipLangsValueKey);
            }
            else
            {
                value = NativeMethods.GetRegKey32(inHive, WinZipLangsRegistryKey, WinZipLangsValueKey);
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
                langName = NativeMethods.GetRegKey64(NativeMethods.HKEY_LOCAL_MACHINE, WinZipLangsRegistryKey, languageId);
            }
            else
            {
                langName = NativeMethods.GetRegKey32(NativeMethods.HKEY_LOCAL_MACHINE, WinZipLangsRegistryKey, languageId);
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
            langId = GetDefaultUILangId(NativeMethods.HKEY_CURRENT_USER);
            if (langId != 0)
            {
                fIsRequestedLangInstalled = IsInstalledLanguage(langId.ToString());
                if (!fIsRequestedLangInstalled)
                    langId = 0;
            }

            // If not found, look in HKLM
            if (langId == 0)
            {
                langId = GetDefaultUILangId(NativeMethods.HKEY_LOCAL_MACHINE);
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

        public static bool GetWinzipPolicyIntegrationRegistryValue(ref bool addDesktop, ref bool addStartMenu, ref int addFileAssoc)
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    addDesktop = regKey.GetValue(WzAddDesktopIconKey, "0") as string == "1";
                    addStartMenu = regKey.GetValue(WzAddStartMenuKey, "0") as string == "1";
                    addFileAssoc = int.Parse(regKey.GetValue(WzAddFileAssociationKey, "0") as string);

                    return true;
                }
            }
            return false;
        }

        public static void SetAdminConfigRegistryStringValue(string keyName, string value)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(WzAdminConfigKey, true))
            {
                if (regKey != null)
                {
                    regKey.SetValue(keyName, value);
                }
                else
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(WzAdminConfigKey))
                    {
                        if (key != null)
                        {
                            key.SetValue(keyName, value);
                        }
                    }
                }
            }
        }

        public static void DeleteAdminConfigRegistryStringValue()
        {
            RemoveCurrentUserRegistryKey(WzAdminConfigKey);
        }

        public static string[] GetAdminConfigRegistryValueName()
        {
            return GetCurrentUserRegistryValueName(WzAdminConfigKey);
        }

        public static string GetAdminConfigRegistryStringValue(string keyName)
        {
            return GetCurrentUserRegistryStringValue(WzAdminConfigKey, keyName);
        }

        public static string[] GetCurrentUserRegistryValueName(string regKeyStr)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, false))
            {
                if (regKey != null)
                {
                    return regKey.GetValueNames();
                }
            }
            return null;
        }

        public static string GetCurrentUserRegistryStringValue(string regKeyStr, string keyName)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, false))
            {
                if (regKey != null)
                {
                    return regKey.GetValue(keyName, string.Empty) as string;
                }
            }
            return string.Empty;
        }

        public static void RemoveCurrentUserRegistryKey(string regKeyStr)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, false))
            {
                if (regKey != null)
                {
                    Registry.CurrentUser.DeleteSubKey(regKeyStr, false);
                }
            }
        }

        public static void RemoveCurrentUserRegistryKeyTree(string regKeyStr)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, true))
            {
                if (regKey != null)
                {
                    Registry.CurrentUser.DeleteSubKeyTree(regKeyStr);
                }
            }
        }

        public static void DeleteCurrentUserRegistryKeyValue(string key, string name)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(key, true))
            {
                if (regKey != null)
                {
                    regKey.DeleteValue(name, false);
                }
            }
        }

        public static void SetRootRegistryStringValue(string key, string name, string data)
        {
            using (var regKey = Registry.ClassesRoot.OpenSubKey(key, true))
            {
                if (regKey != null)
                {
                    regKey.SetValue(name, data);
                }
                {
                    using (var newkey = Registry.ClassesRoot.CreateSubKey(key))
                    {
                        if (newkey != null)
                        {
                            newkey.SetValue(name, data);
                        }
                    }
                }
            }
        }

        public static string GetRootRegistryStringValue(string key, string name)
        {
            using (var regKey = Registry.ClassesRoot.OpenSubKey(key, false))
            {
                if (regKey != null)
                {
                    return regKey.GetValue(name, string.Empty) as string;
                }
            }
            return string.Empty;
        }

        public static void DeleteRootRegistryKeyValue(string key, string name)
        {
            using (var regKey = Registry.ClassesRoot.OpenSubKey(key, true))
            {
                if (regKey != null)
                {
                    regKey.DeleteValue(name, false);
                }
            }
        }

        public static void RemoveRootRegistryKeyTree(string key)
        {
            using (var regKey = Registry.ClassesRoot.OpenSubKey(key, false))
            {
                if (regKey != null)
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(key);
                }
            }
        }

        public static void SetCurrentUserStringValue(string key, string name, string data)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(key, true))
            {
                if (regKey != null)
                {
                    regKey.SetValue(name, data);
                }
                {
                    using (var newkey = Registry.CurrentUser.CreateSubKey(key))
                    {
                        if (newkey != null)
                        {
                            newkey.SetValue(name, data);
                        }
                    }
                }
            }
        }

        public static void AddCurrentUserOpenWithProgidsValue(string key, string name)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(key, true))
            {
                if (regKey != null && regKey.GetValue(name) == null)
                {
                    regKey.SetValue(name, new byte[0], RegistryValueKind.Binary);
                }
            }
        }

        public static bool GetIsEnterprise()
        {
            bool IsEnterprise = false;
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    IsEnterprise = regKey.GetValue(WzEnterpriseKey, "0") as string == "1";
                }
            }
            return IsEnterprise;
        }

        public static string GetIsXAT()
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(Wzxat, false))
            {
                if (regKey != null)
                {
                    return regKey.GetValue(WzxatKey, "").ToString();
                }
            }
            return string.Empty;
        }

        public static byte[] GetSuiteSED()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(WzSuiteKey, false))
            {
                if (regKey != null)
                {
                    return (byte[])regKey.GetValue(WzSubsDeaddate);
                }
            }

            return null;
        }

        public static byte[] GetSuiteSBS()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(WzSuiteKey, false))
            {
                if (regKey != null)
                {
                    return (byte[])regKey.GetValue(WzSubsStatus);
                }
            }

            return null;
        }

        public static byte[] GetSuiteSLC()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(WzSuiteKey, false))
            {
                if (regKey != null)
                {
                    return (byte[])regKey.GetValue(WzSubsLastCheck);
                }
            }

            return null;
        }

        public static bool IsSafeShareEnabled()
        {
            bool ret = false;
#if WZ_APPX
            string value = UWPHelper.GetOemPoliciesValue(WzDisableShareA);
            if (!string.IsNullOrEmpty(value))
            {
                return value == "0";
            }
#endif
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    ret = (regKey.GetValue(WzDisableShareA) as string) == "0";
                }
            }
            return ret;
        }
    }
}
