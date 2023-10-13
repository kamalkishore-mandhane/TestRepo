using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SBkUpUI.WPFUI.Utils
{
    public static partial class WinZipMethods
    {
        private static readonly object _lock = new object();

        public static Dictionary<WzSvcProviderIDs, string> CloudServiceNameMap = new Dictionary<WzSvcProviderIDs, string>()
        {
            { WzSvcProviderIDs.SPID_CLOUD_DROPBOX, "Dropbox" },
            { WzSvcProviderIDs.SPID_CLOUD_BOX, "Box" },
            { WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC, "SugarSync" },
            { WzSvcProviderIDs.SPID_CLOUD_CLOUDME, "CloudMe" },
            { WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3, "AmazonS3" },
            { WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE, "ZipShare" },
            { WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE, "MediaFire" },
            { WzSvcProviderIDs.SPID_CLOUD_OFFICE365, "Office365" },
            { WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK, "SwiftStack" },
            { WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD, "GoogleCloud" },
            { WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD, "IBMCloud" },
            { WzSvcProviderIDs.SPID_CLOUD_RACKSPACE, "Rackspace" },
            { WzSvcProviderIDs.SPID_CLOUD_OPENSTACK, "Openstack" },
            { WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD, "AlibabaCloud" },
            { WzSvcProviderIDs.SPID_CLOUD_WASABI, "Wasabi" },
            { WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE, "S3compatible" },
            { WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB, "AzureBlob" },
            { WzSvcProviderIDs.SPID_CLOUD_OVH, "OVH" },
            { WzSvcProviderIDs.SPID_CLOUD_IONOS, "IONOS" },
            { WzSvcProviderIDs.SPID_CLOUD_NASCLOUD, "NASCloud" },
            { WzSvcProviderIDs.SPID_CLOUD_WEBDAV, "WebDAV" },
            { WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT, "TeamsSharePoint" },
            { WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT, "SharePoint" },
            { WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE, "OneDrive" },
            { WzSvcProviderIDs.SPID_CLOUD_GOOGLE, "GoogleDrive" },
            { WzSvcProviderIDs.SPID_CLOUD_FTP, "FTP" }
        };

        public enum Encryption : int
        {
            CRYPT_NONE = 0,
            CRYPT_CLASSIC = 1,
            CRYPT_AES128 = 2,
            CRYPT_AES192 = 3,
            CRYPT_AES256 = 4
        }

        public static string EncryptionToString(Encryption encryption)
        {
            switch (encryption)
            {
                case Encryption.CRYPT_NONE:
                    return "none";
                case Encryption.CRYPT_CLASSIC:
                    return "zip2";
                case Encryption.CRYPT_AES128:
                    return "aes128";
                case Encryption.CRYPT_AES192:
                    return "aes192";
                case Encryption.CRYPT_AES256:
                    return "aes256";
                default:
                    return "none";
            }
        }

        public static Encryption StringToEncryption(string str)
        {
            str = str.ToLower();
            if (str == "none")
            {
                return Encryption.CRYPT_NONE;
            }
            if (str == "zip2")
            {
                return Encryption.CRYPT_CLASSIC;
            }
            if (str == "aes128")
            {
                return Encryption.CRYPT_AES128;
            }
            if (str == "aes192")
            {
                return Encryption.CRYPT_AES192;
            }
            if (str == "aes256")
            {
                return Encryption.CRYPT_AES256;
            }
            return Encryption.CRYPT_NONE;
        }

        public enum WINZIP_LICENSED_VERSIONS    // wlver
        {
            WLV_MIN_VALUE,
            WLV_STD_VERSION = WLV_MIN_VALUE,
            WLV_PRO_VERSION,
            WLV_TRIAL_VERSION,
            WLV_MAX_VALUE = WLV_TRIAL_VERSION
        };

        public enum WzSvcProviderIDs  // spid     provider IDs
        {
            SPID_UNKNOWN,               // unknown provider
            SPID_SAMPLE,                // sample in SDK (clone of SPID_IMAGE_RESIZE provider)
            SPID_IMAGE_RESIZE,          // image resize service provider
            SPID_DOC2PDF_TRANSFORM,     // doc to PDF provider
            SPID_IMG2JPEG_TRANSFORM,    // image to JPEG provider
            SPID_WATERMARK_TRANSFORM,   // watermarking provider
            SPID_CLOUD_DROPBOX,         // Cloud storage services
            SPID_CLOUD_BOX,
            SPID_CLOUD_SKYDRIVE,
            SPID_CLOUD_ONEDRIVE = SPID_CLOUD_SKYDRIVE, //lint !e488 !e849 has same enumerator value
            SPID_CLOUD_GOOGLE,
            SPID_CLOUD_SUGARSYNC,
            SPID_CLOUD_CLOUDME,
            SPID_CLOUD_FTP,
            SPID_CLOUD_AMAZON_S3,
            SPID_CLOUD_YOUSENDIT,
            SPID_CLOUD_AZURE,
            SPID_CLOUD_FACEBOOK,
            SPID_CLOUD_TWITTER,
            SPID_CLOUD_LINKEDIN,
            SPID_CLOUD_EMAIL,
            SPID_CLOUD_SHAREPOINT,
            SPID_CLOUD_ZIPSHARE,
            SPID_NOTIFY_GTALK,
            SPID_NOTIFY_FACEBOOK,
            SPID_NOTIFY_YAHOOMESSAGE,
            SPID_SOCIALMEDIA_LINKEDIN,
            SPID_SOCIALMEDIA_FACEBOOK,
            SPID_SOCIALMEDIA_TWITTER,
            SPID_CLOUD_MEDIAFIRE,
            SPID_NOTIFY_JABBER,
            SPID_LOCAL_DRIVE,           // Local drive or mapped drive
            SPID_LOCAL_LIBRARY,         // User Library
            SPID_LOCAL_NETWORK,         // Network share
            SPID_LOCAL_FAVORITES,       // User Favorites
            SPID_LOCAL_HOMEGROUP,       // User Homegroup
            SPID_CLOUD_HIGHTAIL,
            SPID_SOCIALMEDIA_YOUTUBE,
            SPID_NOTIFY_TWITTER,
            SPID_CLOUD_OFFICE365,
            SPID_RECIPIENT_GOOGLECONTACTS,
            SPID_RECIPIENT_OUTLOOKCONTACTS,
            SPID_RECIPIENT_YAHOOCONTACTS,
            SPID_ENLARGE_REDUCE_IMAGE,  // enlarge/reduce photo size service provider
            SPID_NOTIFY_OFFICE365,
            SPID_RECIPIENT_LDAPCONTACTS,
            SPID_LOCAL_PORTABLE_DEVICE,
            SPID_RECIPIENT_EXCHANGECONTACTS,
            SPID_RECIPIENT_OUTLOOKPOCONTACTS,
            SPID_RECIPIENT_SLACKCONTACTS,
            SPID_NOTIFY_SLACK,
            SPID_CONVERTPHOTOS_TRANSFORM,
            SPID_REMOVEPERSONALDATA_TRANSFORM,
            SPID_CLOUD_SWIFTSTACK,
            SPID_CLOUD_GOOGLECLOUD,
            SPID_CLOUD_IBMCLOUD,
            SPID_CLOUD_RACKSPACE,
            SPID_CLOUD_OPENSTACK,
            SPID_CLOUD_ALIBABACLOUD,
            SPID_CLOUD_WASABI,
            SPID_CLOUD_S3COMPATIBLE,
            SPID_CLOUD_AZUREBLOB,
            SPID_CLOUD_WEBDAV,
            SPID_CLOUD_CENTURYLINK,
            SPID_CLOUD_OVH,
            SPID_CLOUD_IONOS,
            SPID_COMBINE_PDF_TRANSFORM,
            SPID_SIGN_PDF_TRANSFORM,
            SPID_NOTIFY_TEAMS,
            SPID_CLOUD_TEAMSSHAREPOINT,
            SPID_CLOUD_NASCLOUD,
            SPID_PDF2DOC_TRANSFORM,
            //  ^^^^^^^^ Add new sevices here ^^^^^^^^
            SPID_LAST_INSERT_BEFORE     // keeps lint happy
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WzProfile2
        {
            public WzSvcProviderIDs Id;                // Service ID

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
            public string name;       // User display name or nickname

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
            public string authId;  // User ID/e-mail, it could be for storing token
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        };

        public static DateTime SYSTEMTIMEToDateTime(SYSTEMTIME st)
        {
            return new DateTime(st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WzCloudItem4
        {
            public WzProfile2 profile;  // Profile that this item belongs to.

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
            public string name;         // Name of the File or Folder, no path.

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
            public string itemId;       // Item ID value used by the service

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
            public string description;  // User description of the item

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
            public string type;         /// File type as defined by the service

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
            public string parentId;     // Item's parent ID value used by the service

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
            public string uri;          // Shared URI, if the item is shared.

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string revision;     // Revision number of the file

            [MarshalAs(UnmanagedType.U1)]
            public bool isFolder;

            [MarshalAs(UnmanagedType.U1)]
            public bool isDownloadable; // Is item downloadable, NO for Google Doc types and folders

            public SYSTEMTIME created;

            public SYSTEMTIME modified;

            public Int64 length;        // Size of the file, 0 for folders

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 514)]
            public string path;         // Path of the File or Folder.
        }

        #region 32 API
        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryInit", ExactSpelling = true)]
        private static extern bool NecessaryInit32();

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "NecessaryUnInit", ExactSpelling = true)]
        private static extern void NecessaryUnInit32();

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateSession", ExactSpelling = true)]
        private static extern IntPtr CreateSession32(int serviceId, string accessPermissions, string additionalCMDParameters);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DestroySession", ExactSpelling = true)]
        private static extern void DestroySession32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "FileSelection", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool FileSelection32(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 defaultFolder, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), In, Out] WzCloudItem4[] selectedItems, out int num, bool multiSelect, bool local, bool network, bool cloud, bool allowFolderSelection, bool resurseChildFolders);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DownloadFromCloud", ExactSpelling = true)]
        private static extern bool DownloadFromCloud32(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite, ref int errorCode);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "UploadToCloud", ExactSpelling = true)]
        private static extern bool UploadToCloud32(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowNag", ExactSpelling = true)]
        private static extern bool ShowNag32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowUWPSubscription", ExactSpelling = true)]
        private static extern bool ShowUWPSubscription32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ImportFromScanner", ExactSpelling = true)]
        private static extern bool ImportFromScanner32(IntPtr hwnd, string destFolder);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ImportFromCamera", ExactSpelling = true)]
        private static extern bool ImportFromCamera32(IntPtr hwnd, string destFolder);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "SaveToZip", ExactSpelling = true)]
        private static extern bool SaveToZip32(IntPtr hwnd, WzCloudItem4[] zipItems, uint fileCount, string zipFilePath, bool isShowProgress, bool isShowCompleteDlg);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "DestinationFolderSelection", ExactSpelling = true)]
        private static extern bool DestinationFolderSelection32(IntPtr hwnd, string title, string defaultBtn, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem, bool isMultiSelect, bool isLocal, bool isNetwork, bool isCloud);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "UploadToCloud", ExactSpelling = true)]
        private static extern bool UploadToCloud32(IntPtr hwnd, WzCloudItem4[] cloudFolderItem, int num, string destFolderPath, bool isSilentMode, bool isOverWrite);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShareFile", ExactSpelling = true)]
        private static extern bool ShareFile32(IntPtr hwnd, string filePath);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "IsProviderSupport", ExactSpelling = true)]
        private static extern bool IsProviderSupport32(IntPtr hwnd, WzSvcProviderIDs spid);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowConversionSettings", ExactSpelling = true)]
        private static extern bool ShowConversionSettings32(IntPtr hwnd, int[] spids);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "SaveAsDialog", ExactSpelling = true)]
        private static extern bool SaveAsDialog32(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetLicenseStatus", ExactSpelling = true)]
        private static extern bool GetLicenseStatus32(IntPtr hwnd, ref int licenseType);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "CheckLicense", ExactSpelling = true)]
        private static extern bool CheckLicense32(IntPtr hwnd);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "PasswordDlg", ExactSpelling = true)]
        private static extern bool PasswordDlg32(IntPtr hwnd, bool showModes, ref Encryption method, StringBuilder password);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetAllowedEncryptionMethods", ExactSpelling = true)]
        private static extern bool GetAllowedEncryptionMethods32(IntPtr hwnd, ref int method);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "IsAlwaysEncrypt", ExactSpelling = true)]
        private static extern bool IsAlwaysEncrypt32(IntPtr hwnd, ref bool isAlwaysEncrypt);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "WzLibXProtectData", ExactSpelling = true)]
        private static extern bool ProtectData32(IntPtr hwnd, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "WzLibXUnprotectData", ExactSpelling = true)]
        private static extern bool UnprotectData32(IntPtr hwnd, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "WzWXFGetShortDescription", ExactSpelling = true)]
        private static extern bool WzWXFGetShortDescription32(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "WzWXFListItems", ExactSpelling = true)]
        private static extern bool WzWXFListItems32(IntPtr hwnd, bool isListRecurse, WzCloudItem4 parentFolder, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), In, Out] WzCloudItem4[] selectedItems, ref int num);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ExtractFromZip", ExactSpelling = true)]
        private static extern bool ExtractFromZip32(IntPtr hwnd, string zipPath, bool ovewWrite, bool showProgressMeter, WzCloudItem4 cloudFolder, string[] includeList, int listLength);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ConvertWzCloudItemToString", ExactSpelling = true)]
        private static extern bool ConvertWzCloudItemToString32(IntPtr hwnd, WzCloudItem4 item, StringBuilder data);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "ProcessCommand", ExactSpelling = true)]
        private static extern bool ProcessCommand32(IntPtr hwnd, StringBuilder program, StringBuilder argu);

        [DllImport("WzSharedServices32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetAuthNameAndCode", ExactSpelling = true)]
        private static extern bool GetAuthNameAndCode32(StringBuilder authName, StringBuilder autoCode);

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

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "FileSelection", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool FileSelection64(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 defaultFolder, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), In, Out] WzCloudItem4[] selectedItems, out int num, bool multiSelect, bool local, bool network, bool cloud, bool allowFolderSelection, bool resurseChildFolders);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DownloadFromCloud", ExactSpelling = true)]
        private static extern bool DownloadFromCloud64(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite, ref int errorCode);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "UploadToCloud", ExactSpelling = true)]
        private static extern bool UploadToCloud64(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowNag", ExactSpelling = true)]
        private static extern bool ShowNag64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowUWPSubscription", ExactSpelling = true)]
        private static extern bool ShowUWPSubscription64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ImportFromScanner", ExactSpelling = true)]
        private static extern bool ImportFromScanner64(IntPtr hwnd, string destFolder);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ImportFromCamera", ExactSpelling = true)]
        private static extern bool ImportFromCamera64(IntPtr hwnd, string destFolder);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "SaveToZip", ExactSpelling = true)]
        private static extern bool SaveToZip64(IntPtr hwnd, WzCloudItem4[] zipItems, uint fileCount, string zipFilePath, bool isShowProgress, bool isShowCompleteDlg);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "DestinationFolderSelection", ExactSpelling = true)]
        private static extern bool DestinationFolderSelection64(IntPtr hwnd, string title, string defaultBtn, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem, bool isMultiSelect, bool isLocal, bool isNetwork, bool isCloud);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "UploadToCloud", ExactSpelling = true)]
        private static extern bool UploadToCloud64(IntPtr hwnd, WzCloudItem4[] cloudFolderItem, int num, string destFolderPath, bool isSilentMode, bool isOverWrite);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShareFile", ExactSpelling = true)]
        private static extern bool ShareFile64(IntPtr hwnd, string filePath);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "IsProviderSupport", ExactSpelling = true)]
        private static extern bool IsProviderSupport64(IntPtr hwnd, WzSvcProviderIDs spid);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ShowConversionSettings", ExactSpelling = true)]
        private static extern bool ShowConversionSettings64(IntPtr hwnd, int[] spids);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "SaveAsDialog", ExactSpelling = true)]
        private static extern bool SaveAsDialog64(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetLicenseStatus", ExactSpelling = true)]
        private static extern bool GetLicenseStatus64(IntPtr hwnd, ref int licenseType);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "CheckLicense", ExactSpelling = true)]
        private static extern bool CheckLicense64(IntPtr hwnd);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "PasswordDlg", ExactSpelling = true)]
        private static extern bool PasswordDlg64(IntPtr hwnd, bool showModes, ref Encryption method, StringBuilder password);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetAllowedEncryptionMethods", ExactSpelling = true)]
        private static extern bool GetAllowedEncryptionMethods64(IntPtr hwnd, ref int method);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "IsAlwaysEncrypt", ExactSpelling = true)]
        private static extern bool IsAlwaysEncrypt64(IntPtr hwnd, ref bool isAlwaysEncrypt);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "WzLibXProtectData", ExactSpelling = true)]
        private static extern bool ProtectData64(IntPtr hwnd, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "WzLibXUnprotectData", ExactSpelling = true)]
        private static extern bool UnprotectData64(IntPtr hwnd, StringBuilder data, long bufferSize);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "WzWXFGetShortDescription", ExactSpelling = true)]
        private static extern bool WzWXFGetShortDescription64(IntPtr hwnd, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "WzWXFListItems", ExactSpelling = true)]
        private static extern bool WzWXFListItems64(IntPtr hwnd, bool isListRecurse, WzCloudItem4 parentFolder, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), In, Out] WzCloudItem4[] selectedItems, ref int num);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ExtractFromZip", ExactSpelling = true)]
        private static extern bool ExtractFromZip64(IntPtr hwnd, string zipPath, bool ovewWrite, bool showProgressMeter, WzCloudItem4 cloudFolder, string[] includeList, int listLength);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ConvertWzCloudItemToString", ExactSpelling = true)]
        private static extern bool ConvertWzCloudItemToString64(IntPtr hwnd, WzCloudItem4 item, StringBuilder data);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "ProcessCommand", ExactSpelling = true)]
        private static extern bool ProcessCommand64(IntPtr hwnd, StringBuilder program, StringBuilder argu);

        [DllImport("WzSharedServices64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetAuthNameAndCode", ExactSpelling = true)]
        private static extern bool GetAuthNameAndCode64(StringBuilder authName, StringBuilder autoCode);

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
                return NecessaryInit32();
            }
            else
            {
                return NecessaryInit64();
            }
        }

        public static void NecessaryUnInit()
        {
            if (Is32Bit)
            {
                NecessaryUnInit32();
            }
            else
            {
                NecessaryUnInit64();
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

        public static bool FileSelection(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 defaultFolder, WzCloudItem4[] selectedItems, out int count, bool multiSelect, bool local, bool network, bool cloud, bool allowFolderSelection, bool resurseChildFolders)
        {
            if (Is32Bit)
            {
                return WinZipMethods.FileSelection32(hwnd, title, defaultBtn, filter, defaultFolder, selectedItems, out count, multiSelect, local, network, cloud, allowFolderSelection, resurseChildFolders);
            }
            else
            {
                return WinZipMethods.FileSelection64(hwnd, title, defaultBtn, filter, defaultFolder, selectedItems, out count, multiSelect, local, network, cloud, allowFolderSelection, resurseChildFolders);
            }
        }

        public static bool DownloadFromCloud(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite, ref int errorCode)
        {
            if (Is32Bit)
            {
                return DownloadFromCloud32(hwnd, cloudItems, num, destFolderItem, isSilentMode, isOverWrite, ref errorCode);
            }
            else
            {
                return DownloadFromCloud64(hwnd, cloudItems, num, destFolderItem, isSilentMode, isOverWrite, ref errorCode);
            }
        }

        public static bool UploadToCloud(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, WzCloudItem4 destFolderItem, bool isSilentMode, bool isOverWrite)
        {
            if (Is32Bit)
            {
                return UploadToCloud32(hwnd, cloudItems, num, destFolderItem, isSilentMode, isOverWrite);
            }
            else
            {
                return UploadToCloud64(hwnd, cloudItems, num, destFolderItem, isSilentMode, isOverWrite);
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

        public static bool ImportFromScanner(IntPtr hwnd, string destFolder)
        {
            if (Is32Bit)
            {
                return ImportFromScanner32(hwnd, destFolder);
            }
            else
            {
                return ImportFromScanner64(hwnd, destFolder);
            }
        }

        public static bool ImportFromCamera(IntPtr hwnd, string destFolder)
        {
            if (Is32Bit)
            {
                return ImportFromCamera32(hwnd, destFolder);
            }
            else
            {
                return ImportFromCamera64(hwnd, destFolder);
            }
        }

        public static bool SaveToZip(IntPtr hwnd, WzCloudItem4[] zipItems, uint fileCount, string zipFilePath, bool isShowProgress, bool isShowCompleteDlg)
        {
            if (Is32Bit)
            {
                return SaveToZip32(hwnd, zipItems, fileCount, zipFilePath, isShowProgress, isShowCompleteDlg);
            }
            else
            {
                return SaveToZip64(hwnd, zipItems, fileCount, zipFilePath, isShowProgress, isShowCompleteDlg);
            }
        }

        public static bool DestinationFolderSelection(IntPtr hwnd, string title, string defaultBtn, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem, bool isMultiSelect, bool isLocal, bool isNetwork, bool isCloud)
        {
            if (Is32Bit)
            {
                return DestinationFolderSelection32(hwnd, title, defaultBtn, cloudFolderItem, ref selectedItem, isMultiSelect, isLocal, isNetwork, isCloud);
            }
            else
            {
                return DestinationFolderSelection64(hwnd, title, defaultBtn, cloudFolderItem, ref selectedItem, isMultiSelect, isLocal, isNetwork, isCloud);
            }
        }

        public static bool UploadToCloud(IntPtr hwnd, WzCloudItem4[] cloudItems, int num, string destFolderPath, bool isSilentMode, bool isOverWrite)
        {
            if (Is32Bit)
            {
                return UploadToCloud32(hwnd, cloudItems, num, destFolderPath, isSilentMode, isOverWrite);
            }
            else
            {
                return UploadToCloud64(hwnd, cloudItems, num, destFolderPath, isSilentMode, isOverWrite);
            }
        }
        public static bool ShareFile(IntPtr hwnd, string filePath)
        {
            if (Is32Bit)
            {
                return ShareFile32(hwnd, filePath);
            }
            else
            {
                return ShareFile64(hwnd, filePath);
            }
        }

        public static bool IsProviderSupport(IntPtr hwnd, WzSvcProviderIDs spid)
        {
            if (Is32Bit)
            {
                return IsProviderSupport32(hwnd, spid);
            }
            else
            {
                return IsProviderSupport64(hwnd, spid);
            }
        }

        public static bool ShowConversionSettings(IntPtr hwnd, int[] spids)
        {
            if (Is32Bit)
            {
                return ShowConversionSettings32(hwnd, spids);
            }
            else
            {
                return ShowConversionSettings64(hwnd, spids);
            }
        }


        public static bool SaveAsDialog(IntPtr hwnd, string title, string defaultBtn, string filter, WzCloudItem4 cloudFolderItem, ref WzCloudItem4 selectedItem)
        {
            if (Is32Bit)
            {
                return SaveAsDialog32(hwnd, title, defaultBtn, filter, cloudFolderItem, ref selectedItem);
            }
            else
            {
                return SaveAsDialog64(hwnd, title, defaultBtn, filter, cloudFolderItem, ref selectedItem);
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

        public static bool PasswordDlg(IntPtr hwnd, bool showModes, ref Encryption method, out string password)
        {
            var buff = new StringBuilder(128);
            bool ret = false;

            if (Is32Bit)
            {
                ret = PasswordDlg32(hwnd, showModes, ref method, buff);
            }
            else
            {
                ret = PasswordDlg64(hwnd, showModes, ref method, buff);
            }
            password = ret ? buff.ToString() : null;
            return ret;
        }

        public static bool AllowedEncrypt(IntPtr hwnd)
        {
            int allowedEncrypt = 0;
            if (Is32Bit)
            {
                GetAllowedEncryptionMethods32(hwnd, ref allowedEncrypt);
            }
            else
            {
                GetAllowedEncryptionMethods64(hwnd, ref allowedEncrypt);
            }
            return allowedEncrypt != 0;
        }

        public static bool IsAlwaysEncrypt(IntPtr hwnd)
        {
            bool isAlwaysEncrypt = false;
            if (Is32Bit)
            {
                IsAlwaysEncrypt32(hwnd, ref isAlwaysEncrypt);
            }
            else
            {
                IsAlwaysEncrypt64(hwnd, ref isAlwaysEncrypt);
            }
            return isAlwaysEncrypt;
        }

        public static bool ProtectData(IntPtr hwnd, ref string data)
        {
            var buff = new StringBuilder(4096);
            buff.Append(data);

            if (Is32Bit)
            {
                ProtectData32(hwnd, buff, buff.Capacity);
            }
            else
            {
                ProtectData64(hwnd, buff, buff.Capacity);
            }
            data = buff.ToString();
            return true;
        }

        public static bool UnprotectData(IntPtr hwnd, ref string data)
        {
            var buff = new StringBuilder(data);
            buff.Capacity = 4096;

            if (Is32Bit)
            {
                UnprotectData32(hwnd, buff, buff.Capacity);
            }
            else
            {
                UnprotectData64(hwnd, buff, buff.Capacity);
            }
            data = buff.ToString();
            return true;
        }

        public static string GetShortDescription(IntPtr hwnd, WinZipMethods.WzSvcProviderIDs spid)
        {
            var buff = new StringBuilder(((int)spid).ToString());
            buff.Capacity = 4096;

            if (Is32Bit)
            {
                WzWXFGetShortDescription32(hwnd, buff);
            }
            else
            {
                WzWXFGetShortDescription64(hwnd, buff);
            }
            return buff.ToString();
        }

        public static bool ListItems(IntPtr hwnd, WzCloudItem4 parentFolder, ref List<WinZipMethods.WzCloudItem4> resultList)
        {
            lock(_lock)
            {
                int count = 2048;
                var childrenItems = new WzCloudItem4[count];
                var fOK = false;

                if (Is32Bit)
                {
                    fOK = WzWXFListItems32(hwnd, false, parentFolder, childrenItems, ref count);
                }
                else
                {
                    fOK = WzWXFListItems64(hwnd, false, parentFolder, childrenItems, ref count);
                }

                if (fOK)
                {
                    resultList.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        resultList.Add(childrenItems[i]);
                    }
                }

                return fOK;
            }
        }

        public static bool ExtractFromZip(IntPtr hwnd, string zipPath, bool ovewWrite, bool showProgressMeter, WzCloudItem4 cloudFolder)
        {
            if (Is32Bit)
            {
                return ExtractFromZip32(hwnd, zipPath, ovewWrite, showProgressMeter, cloudFolder, null, 0);
            }
            else
            {
                return ExtractFromZip64(hwnd, zipPath, ovewWrite, showProgressMeter, cloudFolder, null, 0);
            }
        }

        public static string CloudItemToString(IntPtr hwnd, WzCloudItem4 item)
        {
            var buff = new StringBuilder(4096);

            if (Is32Bit)
            {
                ConvertWzCloudItemToString32(hwnd, item, buff);
            }
            else
            {
                ConvertWzCloudItemToString64(hwnd, item, buff);
            }
            return buff.ToString();
        }

        public static bool ProcessCommand(IntPtr hwnd, ref string program,ref string argu)
        {
            var programBuff = new StringBuilder(4096);
            programBuff.Append(program);
            var arguBuff = new StringBuilder(4096);
            arguBuff.Append(argu);

            if (Is32Bit)
            {
                ProcessCommand32(hwnd, programBuff, arguBuff);
            }
            else
            {
                ProcessCommand64(hwnd, programBuff, arguBuff);
            }
            program = programBuff.ToString();
            argu = arguBuff.ToString();
            return true;
        }

        public static bool GetAuthNameAndCode(out string authName, out string authCode)
        {
            var buffName = new StringBuilder(4096);
            var buffCode = new StringBuilder(4096);
            if (Is32Bit)
            {
                GetAuthNameAndCode32(buffName, buffCode);
            }
            else
            {
                GetAuthNameAndCode64(buffName, buffCode);
            }
            authName = buffName.ToString();
            authCode = buffCode.ToString();
            return true;
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
