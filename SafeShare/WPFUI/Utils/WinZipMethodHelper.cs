using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SafeShare.WPFUI.Utils
{
    internal class WinZipMethodHelper
    {
        private static readonly string[] ZipFileTypes = new[]
                                                        {
                                                            ".7z", ".b64", ".bhx", ".bz", ".bz2", ".cab", ".gz", ".hqx", ".lha",
                                                            ".lzh", ".mim", ".rar", ".tar", ".taz", ".tbz", ".tbz2", ".tgz", ".txz",
                                                            ".tz", ".uu", ".uue", ".vmdk", ".xxe", ".xz", ".z", ".zip", ".zipx"
                                                        };

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
                parentId = fileInfo.Directory.FullName,
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

        public static WzCloudItem4 GetOpenPickerDefaultFolder()
        {
            WzCloudItem4 defaultFolder = InitWzCloudItem();

            defaultFolder.itemId = SafeShareSettings.Instance.RecordOpenPickerPath;
            defaultFolder.profile.authId = SafeShareSettings.Instance.RecordOpenPickerAuthId;

            return defaultFolder;
        }

        public static void SetOpenPickerDefaultFolder(WzCloudItem4 item)
        {
            string folderPath = item.itemId;

            if (IsCloudItem(item.profile.Id))
            {
                folderPath = item.parentId;
            }
            else if (IsLocalPortableDeviceItem(item.profile.Id))
            {
                folderPath = item.path;
            }
            else if (!item.isFolder)
            {
                try
                {
                    folderPath = Path.GetDirectoryName(folderPath);
                }
                catch (PathTooLongException)
                {
                    return;
                }
            }

            SafeShareSettings.Instance.RecordOpenPickerPath = folderPath;
            SafeShareSettings.Instance.RecordOpenPickerAuthId = item.profile.authId;
        }

        public static void SetSavePickerDefaultFolder(WzCloudItem4 item)
        {
            string folderPath = item.itemId;
            if (IsCloudItem(item.profile.Id))
            {
                folderPath = item.parentId;
            }
            else if (IsLocalPortableDeviceItem(item.profile.Id))
            {
                folderPath = item.path;
            }
            else
            {
                if (item.itemId == string.Empty || item.itemId == null)
                {
                    // Upload entry doesn't have id
                    folderPath = Path.Combine(item.parentId, item.name);
                }

                if (!item.isFolder)
                {
                    try
                    {
                        folderPath = Path.GetDirectoryName(folderPath);
                    }
                    catch (PathTooLongException)
                    {
                        return;
                    }
                }
            }

            SafeShareSettings.Instance.RecordSavePickerPath = folderPath;
            SafeShareSettings.Instance.RecordSavePickerAuthId = item.profile.authId;
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
                || id == WzSvcProviderIDs.SPID_CLOUD_OFFICE365
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

        public static bool IsLocalPortableDeviceItem(WzSvcProviderIDs id)
        {
            return id == WzSvcProviderIDs.SPID_LOCAL_PORTABLE_DEVICE;
        }

        public static bool DownloadCloudItems(IntPtr windowHandle, ref WzCloudItem4[] selectedItems)
        {
            var downloadFolder = FileOperation.CreateTempFolder(FileOperation.GlobalTempDir);
            var folderItem = InitCloudItemFromPath(downloadFolder);
            int count = selectedItems.Count();
            int downloadErrorCode = 0;

            var ret = WinzipMethods.DownloadFromCloud(windowHandle, selectedItems, count, folderItem, false, false, ref downloadErrorCode);
            if (!ret)
            {
                // Download file failed, return the error code
                Directory.Delete(downloadFolder, true);
                return false;
            }

            var downloadItems = new List<WzCloudItem4>();
            var direcrory = new DirectoryInfo(downloadFolder);
            foreach (var folder in direcrory.GetDirectories())
            {
                downloadItems.Add(InitCloudItemFromPath(folder.FullName));
            }
            foreach (var file in direcrory.GetFiles())
            {
                downloadItems.Add(InitCloudItemFromPath(file.FullName));
            }
            selectedItems = downloadItems.ToArray();

            return true;
        }

        public static void GetCloudItemsByFolder(string folderPath, List<WzCloudItem4> cloudItemList, bool isRecurse = false)
        {
            var direcrory = new DirectoryInfo(folderPath);
            foreach (var file in direcrory.GetFiles())
            {
                cloudItemList.Add(InitCloudItemFromPath(file.FullName));
            }
            foreach (var folder in direcrory.GetDirectories())
            {
                if (isRecurse)
                {
                    GetCloudItemsByFolder(folder.FullName, cloudItemList, isRecurse);
                }
                else
                {
                    cloudItemList.Add(InitCloudItemFromPath(folder.FullName));
                }
            }
        }

        public static bool ValidateZipFile(string filename)
        {
            string fileType = Path.GetExtension(FileOperation.RemoveInvalidCharactersInFileName(filename));

            return (Array.IndexOf(ZipFileTypes, fileType.ToLower()) > -1) ? true : FileOperation.IsAltZipExtensionFile(filename);
        }
    }
}