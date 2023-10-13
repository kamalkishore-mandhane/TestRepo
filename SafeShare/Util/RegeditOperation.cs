using Microsoft.Win32;
using SafeShare.WPFUI.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SafeShare.Util
{
    internal class RegeditOperation
    {
        private const int EnglishLangID = 1033;
        private const string WzPolicyKey = @"Software\WinZip Computing\WinZip\Policies";
        private const string Wzxat = @"Software\WinZip Computing\WinZip";
        private const string WzAdminConfigKey = @"SOFTWARE\WinZip Computing\WinZip\admcfgpdf";
        private const string ExeBitsValueKey = "ExeBits";
        private const string WinZipLangsRegistryKey = @"SOFTWARE\WinZip Computing\WinZip\Langs";
        private const string WinZipLangsValueKey = "InstalledUILangID";
        private const string WzDisableShareAKey = "DisableShareA";
        private const string WzSuiteKey = @"SOFTWARE\WinZip Computing\WinZip\Suite";
        private const string WzSubsLastCheck = "slc";
        private const string WzSubsStatus = "sbs";
        private const string WzSubsDeaddate = "sed";
        private const string WzPwdPolicyLengthKey = "passwordlength";
        private const string WzPwdPolicyReqLowerKey = "passwordreqlower";
        private const string WzPwdPolicyReqUpperKey = "passwordrequpper";
        private const string WzPwdPolicyReqNumKey = "passwordreqnumber";
        private const string WzPwdPolicyReqSymKey = "passwordreqsymbol";

        public const string WzKey = @"SOFTWARE\WinZip Computing\WinZip\WinZip";
        public const string WzAddDesktopIconKey = "AddDesktopIcon";
        public const string WzEnterpriseKey = "Enterprise";
        public const string WzDisableCloudKey = "DisableCloudServices";
        public const string WzxatKey = "x-at";
        public const string WzAddStartMenuKey = "AddStartMenu";
        public const string WzAddFileAssociationKey = "AddFileAssociation";
        public const string WzPdfFmKey = @"SOFTWARE\WinZip Computing\PdfExpress\fm";
        public const string WzFileExtOpenWithProgId = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\OpenWithProgids";
        public const string WzFileExtUserChoice = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\UserChoice";

        public const string WzSafeShareSubKey = @"SOFTWARE\WinZip Computing\SafeShare\SafeShare";
        public const string WzEmailServicesKey = @"SOFTWARE\WinZip Computing\Common\Email\Services";
        public const string WzEmailAccountsKey = @"SOFTWARE\WinZip Computing\Common\Email\Accounts";
        public const string WzEmailShareKey = @"SOFTWARE\WinZip Computing\Common\Email\Share";
        public const string WzDefaultCloudSpidKey = "DefaultShareServiceSpid";
        public const string WzDefaultCloudAuthIdKey = "DefaultShareServiceAuthId";
        public const string WzDefaultCloudNickNameKey = "DefaultShareServiceNickName";
        public const string WzNoInternalEmailerKey = "NoInternalEmailer";
        public const string WzUseMapiKey = "UseMapi";
        public const string WzDefaultAccountKey = "DefaultAccount";
        public const string WzDefaultEmailService = "DefaultEmailService";
        public const string WzWxfBaseKey = @"SOFTWARE\WinZip Computing\WinZip\WXF";
        public const string WzWxfOutlookContactKey = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzAddrocts";
        public const string WzWxfGoogleContactKey = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzAddrgcts";
        public const string WzWxfYahooContactKey = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzAddrycts";

        private const string WzUseAltExternKey = @"Software\WinZip Computing\WinZip\fm";
        private const string WzAltAssocKey = @"Software\WinZip Computing\WinZip\AltAssoc";

        private const string WINZIP_INSTALL_REG_PATH = "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\";
        private const string WzDisableFileExpiration = @"DisableFileExpiration";
        private const string WzDisableDIPUSKey = "DisableDIPUS";
        private const string WzDisableRatingsKey = "DisableRatings";

        private const string WzSurveyRootKey = @"Software\WinZip Computing\Survey";
        private const string WzSurveyFreqKey = "freq";
        private const string WzSurveyDaysKey = "days";
        private const string WzSurveyCountKey = "count";
        private const string WzSurveyLastKey = "last";
        public const string DefaultLastDate = "1/1/1901";
        public const string LastDateFormat = "MM/dd/yyyy";

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

        public static int GetConversionSettingRegistryIntValue(string regKeyStr, string keyName)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, false))
            {
                if (regKey != null)
                {
                    return (int)regKey.GetValue(keyName, -1);
                }
            }
            return -1;
        }

        public static string GetConversionSettingRegistryStringValue(string regKeyStr, string keyName)
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

        public static void SetConversionSettingRegistryValue(string regKeyStr, string keyName, object value)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, true))
            {
                if (regKey != null)
                {
                    regKey.SetValue(keyName, value);
                }
            }
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

        public static string[] GetCurrentUserRegistrySubKeyNames(string regKeyStr)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(regKeyStr, false))
            {
                return regKey?.GetSubKeyNames();
            }
        }

        public static string GetLocalMachineRegistryStringValue(string regKeyStr, string keyName)
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(regKeyStr, false))
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

        public static bool IsAppletEnabled()
        {
            bool ret = true;
#if WZ_APPX
            string value = UWPHelper.GetOemPoliciesValue(WzDisableShareAKey);
            if (!string.IsNullOrEmpty(value))
            {
                return value == "0";
            }
#endif
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    ret = (regKey.GetValue(WzDisableShareAKey) as string) == "0";
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

        public static string LangIDToShortName(int langId)
        {
            switch (langId)
            {
                case 1031:
                    return "DE";    // German
                case 1033:
                    return "EN";    // English
                case 1034:          // Traditional Spanish
                case 3082:          // Modern Spanish
                case 2058:
                    return "ES";    // Spanish or Mexican
                case 1036:
                    return "FR";    // French
                case 1040:
                    return "IT";    // Italian
                case 1041:
                    return "JP";    // Japanese
                case 1043:
                    return "NL";    // Dutch
                case 1046:
                    return "BP";    // Brazilian Portuguese
                case 2052:
                    return "ZH";    // Chinese Simplified
                default:
                    return "EN";
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
                else
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

        public static PasswordPolicy GetPasswordPolicy()
        {
            var policy = new PasswordPolicy();
            policy.MinLength = PasswordGenerater.DefaultPwdMinLength;
            using (var regKey = Registry.CurrentUser.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    policy.MinLength = int.Parse(regKey.GetValue(WzPwdPolicyLengthKey, PasswordGenerater.DefaultPwdMinLength.ToString()) as string);
                    policy.ReqLowerCharacter = regKey.GetValue(WzPwdPolicyReqLowerKey, "0") as string == "1";
                    policy.ReqUpperCharacter = regKey.GetValue(WzPwdPolicyReqUpperKey, "0") as string == "1";
                    policy.ReqNumericCharacter = regKey.GetValue(WzPwdPolicyReqNumKey, "0") as string == "1";
                    policy.ReqSymbolCharacter = regKey.GetValue(WzPwdPolicyReqSymKey, "0") as string == "1";
                }
            }
            return policy;
        }

        public static bool IsDisableFileExpiration()
        {
            var disableFileExpiration = string.Empty;
#if WZ_APPX
            disableFileExpiration = UWPHelper.GetOemPoliciesValue(WzDisableFileExpiration);
#endif
            if (string.IsNullOrEmpty(disableFileExpiration))
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
                {
                    if (regKey != null)
                    {
                        return (regKey.GetValue(WzDisableFileExpiration) as string) == "1";
                    }
                }
            }
            else
            {
                return disableFileExpiration == "1";
            }

            return false;
        }

        public static bool IsUseAltExten()
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzUseAltExternKey, false))
            {
                if (regKey != null)
                {
                    var value = regKey.GetValue("UseAltExten") as string;
                    return value == "1";
                }
                return false;
            }
        }

        public static string GetAlternateExtension()
        {
            using (var regkey = Registry.LocalMachine.OpenSubKey(WzAltAssocKey, false))
            {
                if (regkey != null)
                {
                    return regkey.GetValue("AltAssoc1") as string;
                }
            }
            return string.Empty;
        }

        public static string GetRegKey64(UIntPtr inHive, String inKeyName, string inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, inPropertyName, RegSAM.WOW64_64Key);
        }

        public static string GetRegKey32(UIntPtr inHive, string inKeyName, string inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, inPropertyName, RegSAM.WOW64_32Key);
        }

        private static string GetRegKey(UIntPtr inHive, string inKeyName, string inPropertyName, RegSAM in32or64key)
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

        public enum RegWow64Options
        {
            None = 0,
            KEY_WOW64_64KEY = 0x0100,
            KEY_WOW64_32KEY = 0x0200
        }

        public enum RegistryRights
        {
            ReadKey = 131097,
            WriteKey = 131078
        }

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int RegOpenKeyEx(Microsoft.Win32.RegistryHive hKey, string lpSubKey, uint ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("Advapi32.dll")]
        internal static extern int RegCloseKey(IntPtr hKey);

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, ref uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

        public static string GetWinZipExecutePath()
        {
            string winzipExeBit = WinZipExtBits();
            string winZipInstallRegKey = WINZIP_INSTALL_REG_PATH + (winzipExeBit == "64" ? "winzip64.exe" : "winzip32.exe");
            return GetWinZipExecutePath(winZipInstallRegKey, (int)RegistryRights.ReadKey | (int)((winzipExeBit == "64" ? RegWow64Options.KEY_WOW64_64KEY : RegWow64Options.KEY_WOW64_32KEY)));
        }

        public static string GetInstallFolder()
        {
            string winzipPath = GetWinZipExecutePath();
            return Path.GetDirectoryName(winzipPath);
        }

        public static string WinZipExtBits()
        {
            return IsWinZip64Exe() ? "64" : "32";
        }

        private static string GetWinZipExecutePath(string winZipInstallKey, int samDesired)
        {
            IntPtr key;
            if (RegOpenKeyEx(RegistryHive.LocalMachine, winZipInstallKey, 0, samDesired, out key) == 0)
            {
                uint lptype = 0;
                uint pathByteLength = 520;
                System.Text.StringBuilder path = new System.Text.StringBuilder(260);
                RegQueryValueEx(key, null, 0, ref lptype, path, ref pathByteLength);
                RegCloseKey(key);
                return path.ToString();
            }
            return null;
        }

        private static bool IsZipShareDisabled()
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
            {
                if (regKey != null)
                {
                    var value = regKey.GetValue(WzDisableCloudKey, "0") as string;
                    if (value == "ALL" || value.ToLower().Contains("zipshare"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsRatingSurveyDisabled()
        {
            if (GetIsEnterprise())
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey(WzPolicyKey, false))
                {
                    if (regKey != null)
                    {
                        return (regKey.GetValue(WzDisableRatingsKey, "0") as string) != "0";
                    }
                }
            }
            return false;
        }

        public static RegistryKey GetSurveyOprationKey(string opreation = "")
        {
            opreation = string.IsNullOrEmpty(opreation) ? SurveyXmlDownloadHelper.SurveyOperationName : opreation;
            var operationKey = Path.Combine(WzSurveyRootKey, opreation);

            var regKey = Registry.CurrentUser.OpenSubKey(operationKey, true);
            if (regKey != null)
            {
                return regKey;
            }
            else
            {
                var newkey = Registry.CurrentUser.CreateSubKey(operationKey, true);
                if (newkey != null)
                {
                    return newkey;
                }
            }
            return null;
        }

        public static void SetSurveyFreq(int freq, string opreation)
        {
            using (var regKey = GetSurveyOprationKey(opreation))
            {
                if (regKey != null)
                {
                    regKey.SetValue(WzSurveyFreqKey, freq, RegistryValueKind.DWord);
                }
            }
        }

        public static int GetSurveyFreq()
        {
            using (var regKey = GetSurveyOprationKey())
            {
                int freq = SurveyXmlDownloadHelper.SurveyXmlFreqInterval;
                if (regKey != null && regKey.GetValueNames().Contains(WzSurveyFreqKey))
                {
                    freq = (int)regKey.GetValue(WzSurveyFreqKey, SurveyXmlDownloadHelper.SurveyXmlFreqInterval);
                }
                else
                {
                    SetSurveyFreq(SurveyXmlDownloadHelper.SurveyXmlFreqInterval, SurveyXmlDownloadHelper.SurveyOperationName);
                }
                return freq;
            }
        }

        public static void SetSurveyDays(int days, string opreation)
        {
            using (var regKey = GetSurveyOprationKey(opreation))
            {
                if (regKey != null)
                {
                    regKey.SetValue(WzSurveyDaysKey, days, RegistryValueKind.DWord);
                }
            }
        }

        public static int GetSurveyDays()
        {
            using (var regKey = GetSurveyOprationKey())
            {
                int days = SurveyXmlDownloadHelper.SurveyXmlDaysInterval;
                if (regKey != null && regKey.GetValueNames().Contains(WzSurveyDaysKey))
                {
                    days = (int)regKey.GetValue(WzSurveyDaysKey, SurveyXmlDownloadHelper.SurveyXmlDaysInterval);
                }
                else
                {
                    SetSurveyDays(SurveyXmlDownloadHelper.SurveyXmlDaysInterval, SurveyXmlDownloadHelper.SurveyOperationName);
                }
                return days;
            }
        }

        public static void SetSurveyCount(int count)
        {
            if (IsRatingSurveyDisabled())
            {
                return;
            }

            using (var regKey = GetSurveyOprationKey())
            {
                if (regKey != null)
                {
                    regKey.SetValue(WzSurveyCountKey, count, RegistryValueKind.DWord);
                }
            }
        }

        public static int GetSurveyCount()
        {
            using (var regKey = GetSurveyOprationKey())
            {
                int count = 0;
                if (regKey != null)
                {
                    count = (int)regKey.GetValue(WzSurveyCountKey, 0);
                }
                return count;
            }
        }

        public static void SetSurveyLastDate()
        {
            if (IsRatingSurveyDisabled())
            {
                return;
            }

            var current = DateTime.Now.ToString(LastDateFormat);
            using (var regKey = GetSurveyOprationKey())
            {
                if (regKey != null)
                {
                    regKey.SetValue(WzSurveyLastKey, current, RegistryValueKind.String);
                }
            }
        }

        public static DateTime GetSurveyLastDate()
        {
            using (var regKey = GetSurveyOprationKey())
            {
                var date = DateTime.Parse(DefaultLastDate);
                if (regKey != null)
                {
                    bool praseSuccess = DateTime.TryParseExact(regKey.GetValue(WzSurveyLastKey, DefaultLastDate) as string,
                       LastDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                    if (!praseSuccess || date > DateTime.Now)
                    {
                        date = DateTime.Parse(DefaultLastDate);
                    }
                }

                return date;
            }
        }
    }
}