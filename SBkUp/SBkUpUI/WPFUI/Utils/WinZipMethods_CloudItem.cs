using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SBkUpUI.WPFUI.Utils
{
    static partial class WinZipMethods
    {
        public static bool SelectFolder(IntPtr parent, ref WzCloudItem4 folder)
        {
            string title = Properties.Resources.DESTINATION_FOLDER_TITLE;
            string defaultBtn = Properties.Resources.DESTINATION_FOLDER_BUTTON;
            var cloudItem = folder;
            folder = InitWzCloudItem();
            folder.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;
            return DestinationFolderSelection(parent, title, defaultBtn, cloudItem, ref folder, false, false, true, true);
        }

        public static bool SelectRestoreFolder(IntPtr parent, in WzCloudItem4 defaultFolder, ref WzCloudItem4 folder)
        {
            string title = Properties.Resources.RESTORE_TEXT;
            string defaultBtn = Properties.Resources.RESTORE_TEXT;
            folder = InitWzCloudItem();
            folder.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;
            return DestinationFolderSelection(parent, title, defaultBtn, defaultFolder, ref folder, false, false, true, true);
        }

        public static WzCloudItem4 InitWzCloudItem()
        {
            var profile = new WzProfile2() { authId = null, Id = WzSvcProviderIDs.SPID_LOCAL_DRIVE, name = null };

            var systemCreatedTime = new SYSTEMTIME()
            {
                wYear = 0,
                wMonth = 0,
                wDay = 0,
                wDayOfWeek = 0,
                wHour = 0,
                wMinute = 0,
                wSecond = 0,
                wMilliseconds = 0
            };

            var systemModifiedTime = new SYSTEMTIME()
            {
                wYear = 0,
                wMonth = 0,
                wDay = 0,
                wDayOfWeek = 0,
                wHour = 0,
                wMinute = 0,
                wSecond = 0,
                wMilliseconds = 0
            };

            var item = new WzCloudItem4()
            {
                profile = profile,
                itemId = null,
                parentId = null,
                name = null,
                path = null,
                length = 0,
                isFolder = false,
                isDownloadable = false,
                created = systemCreatedTime,
                modified = systemModifiedTime
            };

            return item;
        }

        public static WzCloudItem4 InitCloudItemFromPath(string localFilePath)
        {
            var fileInfo = new FileInfo(localFilePath);
            bool isDirectory = (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
            DateTime createTime = File.GetCreationTime(localFilePath);
            DateTime modifiedTime = File.GetLastWriteTime(localFilePath);
            var profile = new WzProfile2() { authId = "30", Id = WzSvcProviderIDs.SPID_LOCAL_DRIVE, name = null };

            var systemCreatedTime = new SYSTEMTIME()
            {
                wYear = (ushort)createTime.Year,
                wMonth = (ushort)createTime.Month,
                wDay = (ushort)createTime.Day,
                wDayOfWeek = (ushort)createTime.DayOfWeek,
                wHour = (ushort)createTime.Hour,
                wMinute = (ushort)createTime.Minute,
                wSecond = (ushort)createTime.Second,
                wMilliseconds = (ushort)createTime.Millisecond
            };

            var systemModifiedTime = new SYSTEMTIME()
            {
                wYear = (ushort)modifiedTime.Year,
                wMonth = (ushort)modifiedTime.Month,
                wDay = (ushort)modifiedTime.Day,
                wDayOfWeek = (ushort)modifiedTime.DayOfWeek,
                wHour = (ushort)modifiedTime.Hour,
                wMinute = (ushort)modifiedTime.Minute,
                wSecond = (ushort)modifiedTime.Second,
                wMilliseconds = (ushort)modifiedTime.Millisecond
            };

            var item = new WzCloudItem4()
            {
                profile = profile,
                itemId = localFilePath,
                parentId = fileInfo.Directory == null ? localFilePath.Substring(0, localFilePath.LastIndexOf(Path.DirectorySeparatorChar)) : fileInfo.Directory.FullName,
                name = fileInfo.Name,
                path = localFilePath,
                length = isDirectory ? 0 : fileInfo.Length,
                isFolder = isDirectory,
                isDownloadable = !isDirectory,
                created = systemCreatedTime,
                modified = systemModifiedTime
            };

            return item;
        }

        public static bool IsCloudItem(WzSvcProviderIDs id)
        {
            if (id == WzSvcProviderIDs.SPID_CLOUD_DROPBOX
                || id == WzSvcProviderIDs.SPID_CLOUD_BOX
                || id == WzSvcProviderIDs.SPID_CLOUD_SKYDRIVE
                || id == WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE
                || id == WzSvcProviderIDs.SPID_CLOUD_GOOGLE
                || id == WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC
                || id == WzSvcProviderIDs.SPID_CLOUD_CLOUDME
                || id == WzSvcProviderIDs.SPID_CLOUD_FTP
                || id == WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3
                || id == WzSvcProviderIDs.SPID_CLOUD_YOUSENDIT
                || id == WzSvcProviderIDs.SPID_CLOUD_AZURE
                || id == WzSvcProviderIDs.SPID_CLOUD_FACEBOOK
                || id == WzSvcProviderIDs.SPID_CLOUD_TWITTER
                || id == WzSvcProviderIDs.SPID_CLOUD_LINKEDIN
                || id == WzSvcProviderIDs.SPID_CLOUD_EMAIL
                || id == WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT
                || id == WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE
                || id == WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE
                || id == WzSvcProviderIDs.SPID_CLOUD_HIGHTAIL
                || id == WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK
                || id == WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_RACKSPACE
                || id == WzSvcProviderIDs.SPID_CLOUD_OPENSTACK
                || id == WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD
                || id == WzSvcProviderIDs.SPID_CLOUD_WASABI
                || id == WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE
                || id == WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB
                || id == WzSvcProviderIDs.SPID_CLOUD_WEBDAV
                || id == WzSvcProviderIDs.SPID_CLOUD_CENTURYLINK
                || id == WzSvcProviderIDs.SPID_CLOUD_OVH
                || id == WzSvcProviderIDs.SPID_CLOUD_IONOS
                || id == WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT
                || id == WzSvcProviderIDs.SPID_CLOUD_NASCLOUD)
            {
                return true;
            }

            return false;
        }
    }
}
