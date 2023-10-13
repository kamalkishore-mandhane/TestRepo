using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DupFF.WPFUI.Utils
{
    public static partial class WinZipMethods
    {
        private static readonly object _lock = new object();
        private static bool _busy = false;

        public enum WINZIP_LICENSED_VERSIONS    // wlver
        {
            WLV_MIN_VALUE,
            WLV_STD_VERSION = WLV_MIN_VALUE,
            WLV_PRO_VERSION,
            WLV_TRIAL_VERSION,
            WLV_MAX_VALUE = WLV_TRIAL_VERSION
        };

        public enum BGTUICategory
        {
            Unknown = 0,
            Cleaner,
            Deduplicator,
            Organizer
        };

        #region 32 API
        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryInit", ExactSpelling = true)]
        private static extern bool NecessaryInit32();

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryUnInit", ExactSpelling = true)]
        private static extern void NecessaryUnInit32();

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateSession", ExactSpelling = true)]
        private static extern IntPtr CreateSession32(int serviceId, string accessPermissions, string additionalCMDParameters);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DestroySession", ExactSpelling = true)]
        private static extern void DestroySession32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowNag", ExactSpelling = true)]
        private static extern bool ShowNag32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowUWPSubscription", ExactSpelling = true)]
        private static extern bool ShowUWPSubscription32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetLicenseStatus", ExactSpelling = true)]
        private static extern bool GetLicenseStatus32(IntPtr hwnd, ref int licenseType);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "CheckLicense", ExactSpelling = true)]
        private static extern bool CheckLicense32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetBGToolInfos", ExactSpelling = true)]
        private static extern bool GetBGToolInfos32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetBGToolInfos", ExactSpelling = true)]
        private static extern bool SetBGToolInfos32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateBGTool", ExactSpelling = true)]
        private static extern bool CreateBGTool32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "RunBGTool", ExactSpelling = true)]
        private static extern bool RunBGTool32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ModifyBGTool", ExactSpelling = true)]
        private static extern bool ModifyBGTool32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "FilterBGTool", ExactSpelling = true)]
        private static extern bool FilterBGTool32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DeleteBGTools", ExactSpelling = true)]
        private static extern bool DeleteBGTools32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "UndoDeleteBGTools", ExactSpelling = true)]
        private static extern bool UndoDeleteBGTools32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "RefreshBGToolsStatus", ExactSpelling = true)]
        private static extern bool RefreshBGToolsStatus32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DestroyBGToolInfos", ExactSpelling = true)]
        private static extern bool DestroyBGToolInfos32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetBGTRNInfos", ExactSpelling = true)]
        private static extern bool GetBGTRNInfos32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "BGTTakeAction", ExactSpelling = true)]
        private static extern bool BGTTakeAction32(IntPtr hwnd, StringBuilder guid);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DismissBGTRN", ExactSpelling = true)]
        private static extern bool DismissBGTRN32(IntPtr hwnd, StringBuilder guid);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetTrialPeriodInfo", ExactSpelling = true)]
        private static extern bool GetTrialPeriodInfo32(IntPtr hwnd, ref int nagIndex, ref int trialDaysRemaining, ref bool isAlreadyRegistered, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "LogAppletFeatureEvent", ExactSpelling = true)]
        private static extern bool LogAppletFeatureEvent32(string eventName, string eventProperties);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "IsInGracePeriod", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool IsInGracePeriod32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetGracePeriodInfo", ExactSpelling = true)]
        private static extern bool GetGracePeriodInfo32(IntPtr hwnd, ref int gracePeriodIndex, ref int graceDaysRemaining, StringBuilder useEmail, long bufferSize);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowGracePeriodDialog", ExactSpelling = true)]
        private static extern bool ShowGracePeriodDialog32(IntPtr hwnd, bool fForceShow);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceIsSignedIn", ExactSpelling = true)]
        private static extern bool GraceIsSignedIn32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceSignIn", ExactSpelling = true)]
        private static extern bool GraceSignIn32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceSignOut", ExactSpelling = true)]
        private static extern bool GraceSignOut32(IntPtr hwnd);
        #endregion

        #region 64 API
        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryInit", ExactSpelling = true)]
        private static extern bool NecessaryInit64();

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryUnInit", ExactSpelling = true)]
        private static extern void NecessaryUnInit64();

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateSession", ExactSpelling = true)]
        private static extern IntPtr CreateSession64(int serviceId, string accessPermissions, string additionalCMDParameters);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DestroySession", ExactSpelling = true)]
        private static extern void DestroySession64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowNag", ExactSpelling = true)]
        private static extern bool ShowNag64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowUWPSubscription", ExactSpelling = true)]
        private static extern bool ShowUWPSubscription64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetLicenseStatus", ExactSpelling = true)]
        private static extern bool GetLicenseStatus64(IntPtr hwnd, ref int licenseType);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "CheckLicense", ExactSpelling = true)]
        private static extern bool CheckLicense64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetBGToolInfos", ExactSpelling = true)]
        private static extern bool GetBGToolInfos64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "SetBGToolInfos", ExactSpelling = true)]
        private static extern bool SetBGToolInfos64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateBGTool", ExactSpelling = true)]
        private static extern bool CreateBGTool64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "RunBGTool", ExactSpelling = true)]
        private static extern bool RunBGTool64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ModifyBGTool", ExactSpelling = true)]
        private static extern bool ModifyBGTool64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "FilterBGTool", ExactSpelling = true)]
        private static extern bool FilterBGTool64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DeleteBGTools", ExactSpelling = true)]
        private static extern bool DeleteBGTools64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "UndoDeleteBGTools", ExactSpelling = true)]
        private static extern bool UndoDeleteBGTools64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "RefreshBGToolsStatus", ExactSpelling = true)]
        private static extern bool RefreshBGToolsStatus64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DestroyBGToolInfos", ExactSpelling = true)]
        private static extern bool DestroyBGToolInfos64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetBGTRNInfos", ExactSpelling = true)]
        private static extern bool GetBGTRNInfos64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "BGTTakeAction", ExactSpelling = true)]
        private static extern bool BGTTakeAction64(IntPtr hwnd, StringBuilder guid);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DismissBGTRN", ExactSpelling = true)]
        private static extern bool DismissBGTRN64(IntPtr hwnd, StringBuilder guid);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetTrialPeriodInfo", ExactSpelling = true)]
        private static extern bool GetTrialPeriodInfo64(IntPtr hwnd, ref int nagIndex, ref int trialDaysRemaining, ref bool isAlreadyRegistered, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "LogAppletFeatureEvent", ExactSpelling = true)]
        private static extern bool LogAppletFeatureEvent64(string eventName, string eventProperties);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "IsInGracePeriod", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool IsInGracePeriod64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetGracePeriodInfo", ExactSpelling = true)]
        private static extern bool GetGracePeriodInfo64(IntPtr hwnd, ref int gracePeriodIndex, ref int graceDaysRemaining, StringBuilder useEmail, long bufferSize);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowGracePeriodDialog", ExactSpelling = true)]
        private static extern bool ShowGracePeriodDialog64(IntPtr hwnd, bool fForceShow);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceIsSignedIn", ExactSpelling = true)]
        private static extern bool GraceIsSignedIn64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceSignIn", ExactSpelling = true)]
        private static extern bool GraceSignIn64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GraceSignOut", ExactSpelling = true)]
        private static extern bool GraceSignOut64(IntPtr hwnd);
        #endregion

        internal static bool Is32Bit
        {
            get
            {
                return IntPtr.Size == 4;
            }
        }

        public static IntPtr WinzipSharedServiceHandle
        {
            get;
            set;
        }

        public static bool NecessaryInit()
        {
            if (Is32Bit)
            {
                return WinZipMethods.NecessaryInit32();
            }
            else
            {
                return WinZipMethods.NecessaryInit64();
            }
        }

        public static void NecessaryUnInit()
        {
            if (Is32Bit)
            {
                WinZipMethods.NecessaryUnInit32();
            }
            else
            {
                WinZipMethods.NecessaryUnInit64();
            }
        }

        public static void CreateSession(int serviceId, string accessPermissions, string additionalCMDParameters)
        {
            if (Is32Bit)
            {
                WinzipSharedServiceHandle = WinZipMethods.CreateSession32(serviceId, accessPermissions, additionalCMDParameters);
            }
            else
            {
                WinzipSharedServiceHandle = WinZipMethods.CreateSession64(serviceId, accessPermissions, additionalCMDParameters);
            }
        }

        public static void DestroySession(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                WinZipMethods.DestroySession32(hwnd);
            }
            else
            {
                WinZipMethods.DestroySession64(hwnd);
            }
        }

        public static bool ShowNag(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return ShowNag32(hwnd);
            }
            else
            {
                return ShowNag64(hwnd);
            }
        }

        public static bool ShowUWPSubscription(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return ShowUWPSubscription32(hwnd);
            }
            else
            {
                return ShowUWPSubscription64(hwnd);
            }
        }

        public static bool GetLicenseStatus(IntPtr hwnd, ref int licenseType)
        {
            if (Is32Bit)
            {
                return GetLicenseStatus32(hwnd, ref licenseType);
            }
            else
            {
                return GetLicenseStatus64(hwnd, ref licenseType);
            }
        }

        public static bool CheckLicense(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return CheckLicense32(hwnd);
            }
            else
            {
                return CheckLicense64(hwnd);
            }
        }

        public static string GetBGToolInfos(IntPtr hwnd, BGTUICategory bGTUI)
        {
            _busy = true;
            StringBuilder str = new StringBuilder(((int)bGTUI).ToString(), 4096);
            if (Is32Bit)
            {
                GetBGToolInfos32(hwnd, str);
            }
            else
            {
                GetBGToolInfos64(hwnd, str);
            }
            _busy = false;
            return str.ToString();
        }

        public static string SetBGToolInfos(IntPtr hwnd, string data)
        {
            lock(_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(data, 4096);
                if (Is32Bit)
                {
                    SetBGToolInfos32(hwnd, str);
                }
                else
                {
                    SetBGToolInfos64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string CreateBGTool(IntPtr hwnd)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(4096);
                if (Is32Bit)
                {
                    CreateBGTool32(hwnd, str);
                }
                else
                {
                    CreateBGTool64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string RunBGTool(IntPtr hwnd, int index)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(index.ToString(), 4096);
                if (Is32Bit)
                {
                    RunBGTool32(hwnd, str);
                }
                else
                {
                    RunBGTool64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }

        }

        public static string ModifyBGTool(IntPtr hwnd, int index)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(index.ToString(), 4096);
                if (Is32Bit)
                {
                    ModifyBGTool32(hwnd, str);
                }
                else
                {
                    ModifyBGTool64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string FilterBGTool(IntPtr hwnd, int index)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(index.ToString(), 4096);
                if (Is32Bit)
                {
                    FilterBGTool32(hwnd, str);
                }
                else
                {
                    FilterBGTool64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string DeleteBGTools(IntPtr hwnd, int index)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(index.ToString(), 4096);
                if (Is32Bit)
                {
                    DeleteBGTools32(hwnd, str);
                }
                else
                {
                    DeleteBGTools64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string UndoDeleteBGTools(IntPtr hwnd, int index)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(index.ToString(), 4096);
                if (Is32Bit)
                {
                    UndoDeleteBGTools32(hwnd, str);
                }
                else
                {
                    UndoDeleteBGTools64(hwnd, str);
                }
                _busy = false;
                return str.ToString();
            }
        }

        public static string RefreshBGToolsStatus(IntPtr hwnd)
        {
            if (!_busy)
            {
                lock (_lock)
                {
                    _busy = true;
                    StringBuilder str = new StringBuilder(string.Empty, 4096);
                    if (Is32Bit)
                    {
                        RefreshBGToolsStatus32(hwnd, str);
                    }
                    else
                    {
                        RefreshBGToolsStatus64(hwnd, str);
                    }
                    _busy = false;
                    return str.ToString();
                }
            }
            return string.Empty;
        }

        public static void DestroyBGToolInfos(IntPtr hwnd)
        {
            lock (_lock)
            {
                _busy = true;
                if (Is32Bit)
                {
                    DestroyBGToolInfos32(hwnd);
                }
                else
                {
                    DestroyBGToolInfos64(hwnd);
                }
                _busy = false;
                return;
            }
        }

        public static string GetBGTRNInfos(IntPtr hwnd, BGTUICategory bGTUI)
        {
            if (!_busy)
            {
                lock (_lock)
                {
                    _busy = true;
                    StringBuilder str = new StringBuilder(((int)bGTUI).ToString(), 4096);
                    if (Is32Bit)
                    {
                        GetBGTRNInfos32(hwnd, str);
                    }
                    else
                    {
                        GetBGTRNInfos64(hwnd, str);
                    }
                    _busy = false;
                    return str.ToString();
                }
            }
            return null;
        }

        public static bool BGTTakeAction(IntPtr hwnd, string guid)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(guid, 4096);
                var res = false;
                if (Is32Bit)
                {
                    res = BGTTakeAction32(hwnd, str);
                }
                else
                {
                    res = BGTTakeAction64(hwnd, str);
                }
                _busy = false;
                return res;
            }
        }

        public static bool DismissBGTRN(IntPtr hwnd, string guid)
        {
            lock (_lock)
            {
                _busy = true;
                StringBuilder str = new StringBuilder(guid, 4096);
                var res = false;
                if (Is32Bit)
                {
                    res = DismissBGTRN32(hwnd, str);
                }
                else
                {
                    res = DismissBGTRN64(hwnd, str);
                }
                _busy = false;
                return res;
            }
        }

        public static bool GetTrialPeriodInfo(IntPtr hwnd, ref int nagIndex, ref int trialDaysRemaining, ref bool isAlreadyRegistered, ref string buyNowUrl)
        {
            var buff = new StringBuilder(buyNowUrl);
            buff.Capacity = 1024;
            bool ret;
            if (Is32Bit)
            {
                ret = GetTrialPeriodInfo32(hwnd, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, buff, buff.Capacity);
            }
            else
            {
                ret = GetTrialPeriodInfo64(hwnd, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, buff, buff.Capacity);
            }

            buyNowUrl = buff.ToString();

            return ret;
        }

        public static void LogAppletFeatureEvent(string eventName, string eventProperties)
        {
            if (!string.IsNullOrEmpty(eventName))
            {
                if (Is32Bit)
                {
                    LogAppletFeatureEvent32(eventName, eventProperties);
                }
                else
                {
                    LogAppletFeatureEvent64(eventName, eventProperties);
                }
            }
        }

        public static bool IsInGracePeriod(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return IsInGracePeriod32(hwnd);
            }
            else
            {
                return IsInGracePeriod64(hwnd);
            }
        }

        public static bool GetGracePeriodInfo(IntPtr hwnd, ref int gracePeriodIndex, ref int graceDaysRemaining, ref string userEmail)
        {
            var bufferSize = 1024;
            var buffEmail = new StringBuilder(bufferSize);

            bool ret;
            if (Is32Bit)
            {
                ret = GetGracePeriodInfo32(hwnd, ref gracePeriodIndex, ref graceDaysRemaining, buffEmail, bufferSize);
            }
            else
            {
                ret = GetGracePeriodInfo64(hwnd, ref gracePeriodIndex, ref graceDaysRemaining, buffEmail, bufferSize);
            }

            userEmail = buffEmail.ToString();

            return ret;
        }

        public static bool ShowGracePeriodDialog(IntPtr hwnd, bool fForceShow)
        {
            if (Is32Bit)
            {
                return ShowGracePeriodDialog32(hwnd, fForceShow);
            }
            else
            {
                return ShowGracePeriodDialog64(hwnd, fForceShow);
            }
        }

        public static bool GraceIsSignedIn(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return GraceIsSignedIn32(hwnd);
            }
            else
            {
                return GraceIsSignedIn64(hwnd);
            }
        }

        public static bool GraceSignIn(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return GraceSignIn32(hwnd);
            }
            else
            {
                return GraceSignIn64(hwnd);
            }
        }

        public static bool RocSignOut(IntPtr hwnd)
        {
            if (Is32Bit)
            {
                return GraceSignOut32(hwnd);
            }
            else
            {
                return GraceSignOut64(hwnd);
            }
        }
    }
}

