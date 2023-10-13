using SafeShare.Properties;
using SafeShare.Util;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SafeShare.WPFUI.Utils
{
    public static class FileOperation
    {
        public const int FileAccessDeniedErrorCode = -2147024891;

        private static string _globalTempDir = string.Empty;
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        public static string GlobalTempDir
        {
            get
            {
                return _globalTempDir;
            }
            private set
            {
                _globalTempDir = value;
            }
        }

        public static bool MakeGlobalTempDir()
        {
            string tempPath = Path.GetTempPath();

            if (tempPath == null || tempPath == string.Empty)
            {
                return false;
            }

            GlobalTempDir = CreateTempFolder(tempPath);

            if (GlobalTempDir == null || GlobalTempDir == string.Empty)
            {
                return false;
            }

            return true;
        }

        public static bool DeleteGlobalTempDir()
        {
            if (string.IsNullOrEmpty(_globalTempDir) || !Directory.Exists(_globalTempDir))
            {
                return true;
            }

            try
            {
                var dir = new DirectoryInfo(_globalTempDir);
                dir.Delete(true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string CreateTempFolder(string tempPrefix)
        {
            for (int i = 0; i < 100; i++)
            {
                string folderName = string.Format(@"wzsf{0:x4}", LOWORD(DateTime.Now.Ticks) + i);
                string tempFolder = Path.Combine(tempPrefix, folderName);

                try
                {
                    Directory.CreateDirectory(tempFolder);
                    return tempFolder;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return string.Empty;
        }

        public static ushort LOWORD(long value)
        {
            return (ushort)(value & 0xFFFF);
        }

        public static bool FileIsReadOnly(IntPtr WindowHandle, string FileName)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                if (File.Exists(FileName))
                {
                    var fileInfo = new FileInfo(FileName);
                    if (fileInfo.IsReadOnly)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public static void ForceDeleteFolderRecursive(string folderPath)
        {
            // When folder contains readonly files, delete it directly will throw UnauthorizedAccessException.
            // To prevent this, set file's attributes to FileAttributes.Normal before the delete.
            var directory = new DirectoryInfo(folderPath) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*"))
            {
                info.Attributes = FileAttributes.Normal;
            }

            try
            {
                directory.Delete(true);
            }
            catch
            {
                // catch the exception when delete directory
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            // If target folder does not exist, create it first
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // Create all of the sub directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            // Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                EDPHelper.FileCopy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static string RenameFileName(string filename)
        {
            var ext = Path.GetExtension(filename);
            var prevIndex = 1;
            var fileNameBody = filename.Substring(0, filename.Length - ext.Length);
            Match match = Regex.Match(fileNameBody, "(.*)\\((\\d+)\\)");

            if (match.Value.Length == fileNameBody.Length && match.Groups.Count > 2)
            {
                fileNameBody = match.Groups[1].Value;
                prevIndex = int.Parse(match.Groups[2].Value);
            }
            var newFileName = string.Format("{0}({1}){2}", fileNameBody, prevIndex + 1, ext);
            return newFileName;
        }

        public static bool FileIsLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        public static bool HandleFileException(Exception ex, string fileName = "")
        {
            if (ex is PathTooLongException)
            {
                SimpleMessageWindows.DisplayWarningConfirmationMessage(Resources.WARNING_PATH_TOO_LONG);
            }
            else if (ex is UnauthorizedAccessException)
            {
                string errorMsg = ex.Message;
                if (ex.HResult == FileAccessDeniedErrorCode)
                {
                    errorMsg = string.Format(Resources.FILE_ACCESS_DENIED, fileName);
                }

                SimpleMessageWindows.DisplayWarningConfirmationMessage(errorMsg);
            }
            else if (ex is IOException ioEx && FileIsLocked(ioEx))
            {
                SimpleMessageWindows.DisplayWarningConfirmationMessage(string.Format(Resources.WARNING_FILE_IN_USE, Path.GetFileName(fileName)));
            }
            else
            {
                return false;
            }

            return true;
        }

        public static void CopyFileWithNotExistDestDir(string srcFile, string destFile)
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(destFile));
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            EDPHelper.FileCopy(srcFile, destFile);
        }

        public static string RemoveInvalidCharactersInFileName(string filename)
        {
            string name = filename;
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(
                string.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(regexSearch)));

            name = r.Replace(name, string.Empty);

            return name;
        }

        public static bool IsAltZipExtensionFile(string fileName)
        {
            if (RegeditOperation.IsUseAltExten())
            {
                string altAssoc = RegeditOperation.GetAlternateExtension();
                if (!string.IsNullOrEmpty(altAssoc) && string.Equals(Path.GetExtension(fileName), "." + altAssoc, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}