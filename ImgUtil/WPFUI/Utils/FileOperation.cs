using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ImgUtil.WPFUI.Utils
{
    public class FileOperation : IDisposable
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        public FileOperation()
        {
            MakeGlobalTempDir();
        }

        static FileOperation()
        {
            Instance = new FileOperation();
        }

        ~FileOperation()
        {
            Dispose(false);
        }

        private string _globalTempDir = string.Empty;

        public static FileOperation Instance { get; private set; }

        public string GlobalTempDir => _globalTempDir;

        private bool MakeGlobalTempDir()
        {
            string tempPath = Path.GetTempPath();

            if (tempPath == null || tempPath == string.Empty)
            {
                return false;
            }

            _globalTempDir = CreateTempFolder(tempPath);

            if (_globalTempDir == null || _globalTempDir == string.Empty)
            {
                return false;
            }

            return true;
        }

        private bool DeleteGlobalTempDir()
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

        public string CreateTempFolder(string tempPrefix = null)
        {
            for (int i = 0; i < 100; i++)
            {
                string folderName = string.Format(@"wzimg{0:x4}", LOWORD(DateTime.Now.Ticks) + i);
                string tempFolder = Path.Combine(tempPrefix ?? GlobalTempDir, folderName);

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

        private ushort LOWORD(long value)
        {
            return (ushort)(value & 0xFFFF);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            DeleteGlobalTempDir();
        }

        public static bool FileIsLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        // for local files
        public static WzCloudItem4[] FilterUnreadableFiles(in WzCloudItem4[] files, ref int count, IntPtr intPtr)
        {
            List<WzCloudItem4> goodFiles = new List<WzCloudItem4>();
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
                    if (count == 1)
                    {
                        bool handled = HandleFileException(ex, intPtr);
                    }
                }

            }
            count = goodFiles.Count;
            return goodFiles.ToArray();
        }

        public static WzCloudItem4[] FilterUnreadableFilesWithExceptionThrown(in WzCloudItem4[] files, ref int count, IntPtr intPtr)
        {
            List<WzCloudItem4> goodFiles = new List<WzCloudItem4>();
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
                catch
                {
                    if (count == 1)
                    {
                        throw;
                    }
                }

            }
            count = goodFiles.Count;
            return goodFiles.ToArray();
        }

        public static void FilterUnreadableFiles(List<string> sourceFiles, IntPtr intPtr)
        {
            List<WzCloudItem4> files = new List<WzCloudItem4>(sourceFiles.Count);
            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var tempItem = new WzCloudItem4();
                tempItem.itemId = sourceFiles[i];
                files.Add(tempItem);
            }

            var count = files.Count;
            var tempItems = FilterUnreadableFiles(files.ToArray(), ref count, intPtr);

            sourceFiles.Clear();
            for (int i = 0; i < tempItems.Length; i++)
            {
                sourceFiles.Add(tempItems[i].itemId);
            }
        }

        public static void FilterUnreadableFiles(List<WzCloudItem4> sourceFiles, IntPtr intPtr)
        {
            var count = sourceFiles.Count;
            var tempItems = FilterUnreadableFiles(sourceFiles.ToArray(), ref count, intPtr);

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
