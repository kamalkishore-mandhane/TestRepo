using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace PdfUtil.WPFUI.Utils
{
    public static class FileOperation
    {
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
                string folderName = string.Format(@"wzpu{0:x4}", LOWORD(DateTime.Now.Ticks) + i);
                string tempFolder = Path.Combine(tempPrefix, folderName);

                try
                {
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                        return tempFolder;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return string.Empty;
        }

        public static string GenerateUniqueFileName(string folder, string nameFormat)
        {
            for (int i = 0; i < 100; i++)
            {
                string fileName = string.Format(nameFormat, LOWORD(DateTime.Now.Ticks) + i);
                string filePath = Path.Combine(folder, fileName);

                if (!File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return string.Empty;
        }

        public static string GenerateUniqueName(string nameFormat)
        {
            return string.Format(nameFormat, LOWORD(DateTime.Now.Ticks));
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
                        FlatMessageWindows.DisplayWarningMessage(WindowHandle, Properties.Resources.PDF_IS_READ_ONLY);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool FileIsReadOnly(string FileName)
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

        public static bool FileIsLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        // for local files
        public static WzCloudItem4[] FilterUnreadableFiles(in WzCloudItem4[] files, ref int count, IntPtr intPtr, Action action = null)
        {
            List<WzCloudItem4> goodFiles = new List<WzCloudItem4>();
            Exception exception = null;
            for (var i = 0; i < count; i++)
            {
                try
                {
                    if (Directory.Exists(files[i].itemId))
                    {
                        goodFiles.Add(files[i]);
                    }
                    else
                    {
                        using (Stream stream = new FileStream(files[i].itemId, FileMode.Open, FileAccess.Read))
                        {
                            goodFiles.Add(files[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (exception == null)
                    {
                        exception = ex;
                    }
                }

            }

            if (goodFiles.Count == 0 && exception != null)
            {
                if (action != null)
                {
                    action();
                }

                HandleFileException(exception, intPtr);
            }

            count = goodFiles.Count;
            return goodFiles.ToArray();
        }

        public static void FilterUnreadableFiles(List<string> sourceFiles, IntPtr intPtr, Action action = null)
        {
            List<WzCloudItem4> files = new List<WzCloudItem4>(sourceFiles.Count);
            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var tempItem = new WzCloudItem4();
                tempItem.itemId = sourceFiles[i];
                files.Add(tempItem);
            }

            var count = files.Count;
            var tempItems = FilterUnreadableFiles(files.ToArray(), ref count, intPtr, action);

            sourceFiles.Clear();
            for (int i = 0; i < tempItems.Length; i++)
            {
                sourceFiles.Add(tempItems[i].itemId);
            }
        }

        public static void FilterUnreadableFiles(List<WzCloudItem4> sourceFiles, IntPtr intPtr, Action action = null)
        {
            var count = sourceFiles.Count;
            var tempItems = FilterUnreadableFiles(sourceFiles.ToArray(), ref count, intPtr, action);

            sourceFiles.Clear();
            for (int i = 0; i < tempItems.Length; i++)
            {
                sourceFiles.Add(tempItems[i]);
            }
        }

        public static bool HandleFileException(Exception ex, IntPtr intPtr)
        {
            if (ex is PathTooLongException)
            {
                FlatMessageWindows.DisplayWarningMessage(intPtr, Properties.Resources.WARNING_PATH_TOO_LONG);
            }
            else if (ex is UnauthorizedAccessException)
            {
                FlatMessageWindows.DisplayWarningMessage(intPtr, Properties.Resources.WARNING_ACCESS_DENY);
            }
            else if (ex is IOException ioEx && FileIsLocked(ioEx))
            {
                FlatMessageWindows.DisplayWarningMessage(intPtr, Properties.Resources.FILE_IN_USE_WARNING);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
