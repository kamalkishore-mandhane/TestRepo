using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace SBkUpUI.WPFUI.Utils
{
    class Util
    {
        private static readonly string[] ReservedNames = new[] { "AUX", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "CON",
                                                          "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "NUL", "PRN" };

        public static string GetWinZipPath()
        {
            var process = Process.GetCurrentProcess();
            return Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), WinZipMethods.Is32Bit ? "WinZip32.exe" : "WinZip64.exe");
        }

        public static string RunJobArguments(JobItem jobItem, IntPtr owner)
        {
            return string.Format("/autorunjobfile sbkupHwnd:{0} \"{1}\"", owner.ToInt32(), jobItem.FilePath);
        }

        public static string GetDisplayName(WinZipMethods.WzCloudItem4 item)
        {
            string displayName = item.itemId;
            if (WinZipMethods.IsCloudItem(item.profile.Id))
            {
                displayName = item.path;
            }
            else
            {
                if (!string.IsNullOrEmpty(item.path))
                {
                    displayName = item.path;
                }
            }

            if (displayName.Last() == Path.DirectorySeparatorChar)
            {
                displayName = displayName.Substring(0, displayName.Length - 1);
            }
            return displayName;
        }

        public static string GetTrimmedString(string textSource, double maxWidth, TextBlock block, int protectNum = 0)
        {
            return GetTrimmedString(textSource, maxWidth, protectNum, block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch, block.FontSize);
        }

        public static string GetTrimmedString(string textSource, double maxWidth, System.Windows.Controls.TextBox box, int protectNum = 0)
        {
            return GetTrimmedString(textSource, maxWidth, protectNum, box.FontFamily, box.FontStyle, box.FontWeight, box.FontStretch, box.FontSize);
        }

        public static string GetTrimmedString(string textSource, double maxWidth, int protectNum, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            if (!textSource.Contains(Path.DirectorySeparatorChar))
            {
                return textSource;
            }

            var pretext = textSource.Substring(0, protectNum);
            textSource = textSource.Substring(protectNum);
            string filename = Path.GetFileName(textSource);
            var directory = textSource.Substring(0, textSource.LastIndexOf(Path.DirectorySeparatorChar));
            bool widthOK = false;
            bool changedWidth = false;
            bool isFilenameWidth = false;
            bool isFilenameOK = false;
            bool isDirectoryOK = false;
            string finalPathShow ="";

            do
            {
                var formattedText = new FormattedText(
                pretext + directory + "\\...\\" + filename,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);

                var formattedFilename = new FormattedText(
                pretext + "\\...\\" + filename,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);

                var formattedDirectory = new FormattedText(
                pretext + directory + "\\...",
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);

                widthOK = formattedText.Width < (maxWidth - 20);
                isFilenameOK = formattedFilename.Width < (maxWidth - 20);
                isDirectoryOK = formattedDirectory.Width < (maxWidth - 20);

                if (!isFilenameOK && isDirectoryOK)
                {
                    changedWidth = true;
                    filename = filename.Substring(0, filename.Length/2);
                    isFilenameWidth = true;
                }
                else if(isFilenameOK && !isDirectoryOK)
                {
                    changedWidth = true;
                    if (directory.Contains(Path.DirectorySeparatorChar))
                    {
                        directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
                    }
                    else
                    {
                        finalPathShow = string.Format("{0}...\\{1}", pretext, filename);
                        return isFilenameOK ? finalPathShow + "..." : finalPathShow;
                    }
                }
                else if (!isFilenameOK && !isDirectoryOK)
                {
                    changedWidth = true;
                    filename = filename.Substring(0, filename.Length / 2);
                    isFilenameWidth = true;
                    if (directory.Contains(Path.DirectorySeparatorChar))
                    {
                        directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
                    }
                    else
                    {
                        finalPathShow = string.Format("{0}...\\{1}", pretext, filename);
                        return isFilenameOK ? finalPathShow + "..." : finalPathShow;
                    }
                }
                else if (!widthOK)
                {
                    changedWidth = true;
                    if (directory.Contains(Path.DirectorySeparatorChar))
                    {
                        directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
                    }
                    else
                    {
                        finalPathShow = string.Format("{0}...\\{1}", pretext, filename);
                        return isFilenameOK ? finalPathShow + "..." : finalPathShow;
                    }
                }
            } while (!widthOK);

            if (!changedWidth)
            {
                return pretext + textSource;
            }

            finalPathShow = string.Format("{0}{1}...\\{2}", pretext, directory, filename);
            return isFilenameWidth ? finalPathShow + "..." : finalPathShow;
        }

        public static string GetRestoreFolderStorageText(in WinZipMethods.WzCloudItem4 item)
        {
            var startIndex = item.path.IndexOf(Path.DirectorySeparatorChar);
            var res = item.path.Substring(startIndex);
            if (res.Length > 1 && res.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                res = res.Substring(1, res.Length - 1);
            }
            if (res.Length > 1 && res.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                res = res.Substring(0, res.Length - 1);
            }
            return res;
        }

        public static void GetRestoreFolderRecordText(ref WinZipMethods.WzCloudItem4 item)
        {
            var res = item.path;
            if (!res.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                res = res + Path.DirectorySeparatorChar.ToString();
            }
            item.path = WinZipMethods.GetShortDescription(IntPtr.Zero, item.profile.Id) + (res.Length == 1 ? string.Empty : Path.DirectorySeparatorChar.ToString()) + res;
        }

        public static string GetFolderName(string str)
        {

            if (str.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                str = str.Substring(0, str.Length - 1);
            }

            var lastIndex = str.LastIndexOf(Path.DirectorySeparatorChar);

            if (lastIndex >= 0)
            {
                str = str.Substring(lastIndex + 1);
            }
            return str;
        }

        public static string GetFolderPath(string str)
        {
            var firstIndex = str.IndexOf(Path.DirectorySeparatorChar);
            var lastIndex = str.LastIndexOf(Path.DirectorySeparatorChar);

            if (firstIndex > 0 && firstIndex + 1 < lastIndex)
            {
                return str.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            }
            return str;
        }

        public static string GetFolderPathFromID(string id)
        {
            if (!id.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return id + Path.DirectorySeparatorChar.ToString();
            }
            return id;
        }

        public static string GetIDFromFolderPath(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path.Substring(0, path.Length - 1);
            }
            return path;
        }

        private static List<string> tempFolder = new List<string>();

        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            tempFolder.Add(tempDirectory);
            return tempDirectory;
        }

        public static void CleanTempFolder()
        {
            foreach (var item in tempFolder)
            {
                Directory.Delete(item, true);
            }
        }

        public static bool FileIsInJobFolder(string file)
        { 
            return Path.GetDirectoryName(file) == Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinZip Backups"); ;
        }

        public static FileInfo[] GetAllSwjfFiles(string folder)
        {
            var dirInfo = new DirectoryInfo(folder);
            return dirInfo.GetFiles("*" + Swjf.Extension);
        }

        public static Swjf GetCannedSwjf(CannedSwjf canned)
        {
            var swjf = new Swjf();
            var path = Util.GetCannedPath(canned);
            swjf.backupFolder = WinZipMethods.InitCloudItemFromPath(path);
            swjf.storeFolder = WinZipMethods.InitCloudItemFromPath(Path.Combine(Util.GetCannedPath(CannedSwjf.Documents), Properties.Resources.MY_WINZIP_FILES));
            swjf.zipName = Util.GetCannedZipName(canned);
            swjf.isCanned = true;
            if (canned == CannedSwjf.Documents)
            {
                swjf.excludeFilter = "*\\" + Properties.Resources.MY_WINZIP_FILES + "\\*";
            }
            return swjf;
        }

        public static List<CannedSwjf> GetMissingCannedSwjf(ObservableCollection<JobItem> jobs)
        {
            var list = new List<CannedSwjf>();
            bool found = true;
            for (int i = 0; ((CannedSwjf)i) != CannedSwjf.End; i++)
            {
                found = false;
                foreach (var item in jobs)
                {
                    if (item.FilePath == Path.Combine(ViewModel.SBkUpViewModel.JobFolder, Util.GetCannedSwjfName((CannedSwjf)i)))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    list.Add((CannedSwjf)i);
                }
            }
            return list;
        }

        public static string GetCannedPath(CannedSwjf canned)
        {
            switch (canned)
            {
                case CannedSwjf.Documents:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case CannedSwjf.Desktop:
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                case CannedSwjf.Favorites:
                    return Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
                case CannedSwjf.Pictures:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                default:
                    return string.Empty;
            }
        }

        public static string GetCannedZipName(CannedSwjf canned)
        {
            switch (canned)
            {
                case CannedSwjf.Documents:
                    return "My Documents.zipx";
                case CannedSwjf.Desktop:
                    return "My Desktop.zipx";
                case CannedSwjf.Favorites:
                    return "My Favorites.zipx";
                case CannedSwjf.Pictures:
                    return "Pictures Library.zipx";
                default:
                    return string.Empty;
            }
        }

        public static string GetCannedSwjfName(CannedSwjf canned)
        {
            switch (canned)
            {
                case CannedSwjf.Documents:
                    return "Documents" + Swjf.Extension;
                case CannedSwjf.Desktop:
                    return "Desktop" + Swjf.Extension;
                case CannedSwjf.Favorites:
                    return "Favorites" + Swjf.Extension;
                case CannedSwjf.Pictures:
                    return "Pictures" + Swjf.Extension;
                default:
                    return string.Empty;
            }
        }

        public static string GetCannedJobDisPlayName(string fileName)
        {
            if (fileName == "Documents")
            {
                return Properties.Resources.DOCUMENTS;
            }
            if (fileName == "Desktop")
            {
                return Properties.Resources.DESKTOP;
            }
            if (fileName == "Favorites")
            {
                return Properties.Resources.FAVORITES;
            }
            if (fileName == "Pictures")
            {
                return Properties.Resources.PICTURES;
            }
            return fileName;
        }

        public static bool IsReservedName(string name)
        {
            int pos = name.IndexOf('.');
            if (pos >= 0)
            {
                name = name.Substring(0, pos);
            }

            return Array.BinarySearch(ReservedNames, name, StringComparer.OrdinalIgnoreCase) >= 0;
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }
    }
}
