using Aspose.Imaging;
using ImgUtil.WPFUI.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace ImgUtil.WPFUI
{
    static class ImageHelper
    {
        static ImageHelper()
        {
            var graphics = System.Drawing.Graphics.FromHwnd((IntPtr)0);
            CurrentDpi_X = graphics.DpiX;
            CurrentDpi_Y = graphics.DpiY;
            DesktopWindowHandle = NativeMethods.GetDesktopWindow();
        }

        public const string JpgExtension = ".jpg";
        public const string JpeExtension = ".jpe";
        public const string JpegExtension = ".jpeg";
        public const string JfifExtension = ".jfif";
        public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";
        public const string DibExtension = ".dib";
        public const string Jp2Extension = ".jp2";
        public const string TifExtension = ".tif";
        public const string TiffExtension = ".tiff";
        public const string PsdExtension = ".psd";
        public const string WebpExtension = ".webp";
        public const string GifExtension = ".gif";
        public const string SvgExtension = ".svg";

        public const string ImageProgId = "WinZip.ImageManager";
        public const string AppUserModeId = "AppUserModelID";

        public static double CurrentDpi_X { get; private set; }

        public static double CurrentDpi_Y { get; private set; }

        public static bool IsWin7 => Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;

        public static IntPtr DesktopWindowHandle { get; private set; }

        public static bool CheckTeamsBackgroundsFolderExist()
        {
            bool teamsExist = false;

            using (var baseKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\"))
            {
                if (baseKey != null)
                {
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        if (subKeyName.Contains("Teams"))
                        {
                            teamsExist = true;
                            break;
                        }
                    }
                }
            }

            bool specialFolderExist = false;
            string specialPath = GetTeamsBackgroundFolder();

            if (Directory.Exists(specialPath))
            {
                specialFolderExist = true;
            }

            return teamsExist && specialFolderExist;
        }

        public static string GetTeamsBackgroundFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Teams\\Backgrounds\\Uploads";
        }

        public static FileFormat GetImageRealFormatFromPath(string path)
        {
            FileFormat format = Image.GetFileFormat(path);
            if (format == FileFormat.Undefined && Aspose.PSD.Image.GetFileFormat(path) == Aspose.PSD.FileFormat.Psd)
            {
                format = FileFormat.Psd;
            }
            return format;
        }

        public static FileFormat GetImageFormatFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return FileFormat.Undefined;
            }

            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case BmpExtension:
                case DibExtension:
                    return FileFormat.Bmp;

                case GifExtension:
                    return FileFormat.Gif;

                case JpgExtension:
                case JpegExtension:
                case JpeExtension:
                case JfifExtension:
                    return FileFormat.Jpeg;

                case Jp2Extension:
                    return FileFormat.Jpeg2000;

                case PngExtension:
                    return FileFormat.Png;

                case PsdExtension:
                    return FileFormat.Psd;

                case TifExtension:
                case TiffExtension:
                    return FileFormat.Tiff;

                case WebpExtension:
                    return FileFormat.Webp;

                case SvgExtension:
                    return FileFormat.Svg;

                default:
                    return FileFormat.Undefined;
            }
        }

        public static string[] GetFileExtensionsFromFormat(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Bmp:
                    return new string[] { BmpExtension, DibExtension };

                case FileFormat.Gif:
                    return new string[] { GifExtension };

                case FileFormat.Jpeg:
                    return new string[] { JpgExtension, JpeExtension, JpegExtension, JfifExtension };

                case FileFormat.Jpeg2000:
                    return new string[] { Jp2Extension };

                case FileFormat.Png:
                    return new string[] { PngExtension };

                case FileFormat.Psd:
                    return new string[] { PsdExtension };

                case FileFormat.Svg:
                    return new string[] { SvgExtension };

                case FileFormat.Tiff:
                    return new string[] { TifExtension, TiffExtension };

                case FileFormat.Webp:
                    return new string[] { WebpExtension };

                default:
                    return new string[] { };
            }
        }

        public static string GetFormatStringFromFormat(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Bmp:
                    return "BMP";

                case FileFormat.Gif:
                    return "GIF";

                case FileFormat.Jpeg:
                    return "JPEG";

                case FileFormat.Jpeg2000:
                    return "JPEG2000";

                case FileFormat.Png:
                    return "PNG";

                case FileFormat.Psd:
                    return "PSD";

                case FileFormat.Svg:
                    return "SVG";

                case FileFormat.Tiff:
                    return "TIFF";

                case FileFormat.Webp:
                    return "WEBP";

                default:
                    return null;
            }
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

        public static string CloudItemToString(WzCloudItem4 item, string delimiter = "\t")
        {
            if (item.profile.Id == WzSvcProviderIDs.SPID_UNKNOWN)
            {
                return null;
            }
            else
            {
                var strList = new List<string>();
                strList.Add(((int)item.profile.Id).ToString());
                strList.Add(item.profile.authId);
                strList.Add(item.profile.name);
                strList.Add(item.name);
                strList.Add(item.itemId);
                strList.Add(item.description);
                strList.Add(item.type);
                strList.Add(item.parentId);
                strList.Add(item.uri);
                strList.Add(item.revision);
                strList.Add(item.isFolder ? "1" : "0");
                strList.Add(item.isDownloadable ? "1" : "0");

                string timeFormat = "%u/%u/%u %u:%u:%u.%u";
                var st = item.created;
                strList.Add(string.Format(timeFormat, st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds));
                st = item.modified;
                strList.Add(string.Format(timeFormat, st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds));

                strList.Add(((int)item.length).ToString());
                strList.Add(item.path);

                var sb = new StringBuilder();
                foreach (var str in strList)
                {
                    sb.Append(str);
                    sb.Append(delimiter);
                }

                return sb.ToString();
            }
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
            var defaultFolder = InitWzCloudItem();

            if (string.IsNullOrEmpty(ImgUtilSettings.Instance.RecordOpenPickerPath) || string.IsNullOrEmpty(ImgUtilSettings.Instance.RecordOpenPickerAuthId))
            {
                defaultFolder.itemId = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                defaultFolder.profile.authId = "30";
            }
            else
            {
                defaultFolder.itemId = ImgUtilSettings.Instance.RecordOpenPickerPath;
                defaultFolder.profile.authId = ImgUtilSettings.Instance.RecordOpenPickerAuthId;
            }

            return defaultFolder;
        }

        public static WzCloudItem4 GetSavePickerDefaultFolder()
        {
            WzCloudItem4 defaultFolder = InitWzCloudItem();

            defaultFolder.itemId = defaultFolder.parentId = ImgUtilSettings.Instance.RecordSavePickerPath;
            defaultFolder.profile.authId = ImgUtilSettings.Instance.RecordSavePickerAuthId;

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
                folderPath = Path.GetDirectoryName(folderPath);
            }

            ImgUtilSettings.Instance.RecordOpenPickerPath = folderPath;
            ImgUtilSettings.Instance.RecordOpenPickerAuthId = item.profile.authId;
        }

        public static void SetSavePickerDefaultFolder(WzCloudItem4 item)
        {
            string folderPath = item.parentId;

            ImgUtilSettings.Instance.RecordSavePickerPath = folderPath;
            ImgUtilSettings.Instance.RecordSavePickerAuthId = item.profile.authId;
        }

        public static System.Windows.Forms.DialogResult AskToModifyFileExtension(string formatString, string extension, bool fromShellExtension, bool isReadOnly)
        {
            List<TASKDIALOG_BUTTON> buttons = new List<TASKDIALOG_BUTTON>();
            string dialogTitle = Properties.Resources.TASKDLG_CHANGE_EXT_TITLE;
            string dialogContent = string.Format(Properties.Resources.TASKDLG_WRONG_EXTENSION, formatString, extension);
            if (fromShellExtension)
            {
                if (isReadOnly)
                {
                    dialogTitle = Properties.Resources.TASKDLG_WRONG_EXT_TITLE;
                    dialogContent = string.Format(Properties.Resources.TASKDLG_WRONG_EXTENSION_CANNOT_RENAMED, formatString, extension);
                }
                else
                {
                    buttons.Add(new TASKDIALOG_BUTTON()
                    {
                        id = (int)System.Windows.Forms.DialogResult.Yes,
                        text = Properties.Resources.TASKDLG_CHANGE_EXT_BUTTON + "\n" + Properties.Resources.TASKDLG_CHANGE_EXT_BUTTON_TIP
                    });
                }

                buttons.Add(new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.No,
                    text = Properties.Resources.TASKDLG_SKIP_BUTTON + "\n" + Properties.Resources.TASKDLG_SKIP_BUTTON_CONTENT
                });

                buttons.Add(new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.Cancel,
                    text = Properties.Resources.TASKDLG_CANCEL_BUTTON
                });
            }
            else
            {
                buttons.Add(new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.Yes,
                    text = Properties.Resources.TASKDLG_CHANGE_EXT_BUTTON + "\n" + Properties.Resources.TASKDLG_CHANGE_EXT_BUTTON_TIP
                });

                buttons.Add(new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.No,
                    text = Properties.Resources.TASKDLG_DONT_CHANGE_BUTTON + "\n" + Properties.Resources.TASKDLG_DONT_CHANGE_BUTTON_TIP
                });

                buttons.Add(new TASKDIALOG_BUTTON()
                {
                    id = (int)System.Windows.Forms.DialogResult.Cancel,
                    text = Properties.Resources.TASKDLG_CANCEL_BUTTON
                });
            }

            var taskDialog = new TaskDialog(Properties.Resources.IMAGE_UTILITY_TITLE, dialogTitle, dialogContent, null, buttons.ToArray(),
                fromShellExtension ? Properties.Resources.ImgUtil : System.Drawing.SystemIcons.Warning, 250);

            var result = taskDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);

            return result.dialogResult;
        }

        public static string ChangeFileExtension(string sourcePath, string destExtension)
        {
            var fileDirectory = Path.GetDirectoryName(sourcePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
            var fileFullPath = Path.ChangeExtension(sourcePath, destExtension);
            int count = 1;
            while (File.Exists(fileFullPath) && count < 1000)
            {
                fileFullPath = Path.Combine(fileDirectory, $"{fileNameWithoutExtension}({count++}){destExtension}");
            }
            return fileFullPath;
        }
    }
}
