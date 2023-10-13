using ImgUtil.WPFUI.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.Model
{
    public class RecentFile : INotifyPropertyChanged
    {
        private string _name;
        private uint _index;
        private long _fileSize;
        private DateTime? _fileModifiedDate;
        private BitmapSource _thumbnail;
        private bool _isThumbnailLoaded;
        private bool _isThumbnailLoading;

        static private BitmapSource _defaultThumbnail;

        // Parameterless constructor for serialize
        private RecentFile()
        {
        }

        public RecentFile(WzCloudItem4 item)
        {
            FileItem = item;
            RecentFileName = item.name;
            IsCloudItem = ImageHelper.IsCloudItem(item.profile.Id);
            IsLocalPortableDeviceItem = ImageHelper.IsLocalPortableDeviceItem(item.profile.Id);
            UpdateFileInfo();
        }

        public RecentFile(WzCloudItem4 item, BitmapSource bitmap)
        {
            FileItem = item;
            RecentFileName = item.name;
            IsCloudItem = ImageHelper.IsCloudItem(item.profile.Id);
            IsLocalPortableDeviceItem = ImageHelper.IsLocalPortableDeviceItem(item.profile.Id);
            UpdateFileInfo();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _thumbnail = ScaleThumbnail(bitmap);
                _isThumbnailLoaded = true;
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public uint RecentFileIndex
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    Notify(nameof(RecentFileIndex));
                }
            }
        }

        public string RecentFileName
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Notify(nameof(RecentFileName));
                }
            }
        }

        public string RecentFileCloudName => FileItem.profile.name;

        public string RecentFileFullName => IsCloudItem ? $"({RecentFileCloudName}) " + _name : _name;

        public bool IsCloudItem { get; set; }

        public bool IsLocalPortableDeviceItem { get; set; }

        public WzCloudItem4 FileItem { get; set; }

        public string RecentFileTooltip
        {
            get
            {
                if (IsCloudItem || IsLocalPortableDeviceItem)
                {
                    var providerDisplayName = WinzipMethods.GetProviderDisplayInfo(IntPtr.Zero, FileItem.profile);
                    if (string.IsNullOrEmpty(FileItem.path))
                    {
                        return string.Format("{0} ({1})", _name, providerDisplayName);
                    }
                    else
                    {
                        return string.Format("{0} ({1}: {2})", _name, providerDisplayName, FileItem.path);
                    }
                }
                else
                {
                    return string.Format("{0} ({1})", _name, FileItem.parentId);
                }
            }
        }

        public DateTime? FileModifiedDate
        {
            get
            {
                return _fileModifiedDate;
            }
            set
            {
                if (_fileModifiedDate != value)
                {
                    _fileModifiedDate = value;
                    Notify("FileModifiedDate");
                    Notify("FileModifiedDateString");
                }
            }
        }

        public string FileModifiedDateString
        {
            get
            {
                if (_fileModifiedDate != null)
                {
                    return ((DateTime)_fileModifiedDate).ToString("MMMM d, yyyy", System.Globalization.CultureInfo.CurrentCulture);
                }

                return string.Empty;
            }
        }

        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    Notify("FileSize");
                }
            }
        }

        public string FileNameWithoutExt
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_name);
            }
        }

        public string FileType
        {
            get
            {
                return Path.GetExtension(_name).Trim('.').ToUpper();
            }
        }

        public BitmapSource FileThumbnail
        {
            get
            {
                if (_thumbnail != null && (_isThumbnailLoaded || _isThumbnailLoading))
                {
                    return _thumbnail;
                }
                else
                {
                    _thumbnail = _defaultThumbnail;

                    if (!IsCloudItem && !IsLocalPortableDeviceItem && File.Exists(FileItem.itemId))
                    {
                        _isThumbnailLoading = true;
                        var watchThread = new Thread(LoadThumbnail);
                        watchThread.IsBackground = true;
                        watchThread.Start();
                    }
                    else
                    {
                        _isThumbnailLoaded = true;
                    }

                    return _thumbnail;
                }
            }
        }

        public void UpdateFileInfo()
        {
            if (!IsCloudItem && !IsLocalPortableDeviceItem)
            {
                string itemPath = !string.IsNullOrEmpty(FileItem.itemId) ?
                FileItem.itemId : Path.Combine(FileItem.parentId, FileItem.name);

                if (!string.IsNullOrEmpty(itemPath))
                {
                    FileInfo fi = new FileInfo(itemPath);
                    if (fi.Exists)
                    {
                        FileSize = fi.Length;
                        FileModifiedDate = fi.LastWriteTime;
                    }
                }
            }
            else
            {
                var modifyDate = FileItem.modified;
                FileModifiedDate = new DateTime(modifyDate.wYear, modifyDate.wMonth, modifyDate.wDay);

                FileSize = FileItem.length;
            }

            if (_defaultThumbnail == null)
            {
                try
                {
                    var thumbnail = new BitmapImage();
                    thumbnail.BeginInit();
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.UriSource = new Uri("pack://application:,,,/Resources/ImageThumbnail.ico");
                    thumbnail.DecodePixelHeight = 48;
                    thumbnail.EndInit();

                    _defaultThumbnail = thumbnail;
                }
                catch (Exception)
                {
                }
            }
        }

        public void UpdateThumbnail(RecentFile recent)
        {
            _thumbnail = recent._thumbnail;
            _isThumbnailLoaded = true;
        }

        public void UpdateThumbnail(BitmapSource thumbnail)
        {
            _thumbnail = ScaleThumbnail(thumbnail);
            _isThumbnailLoaded = true;
        }

        private BitmapSource ScaleThumbnail(BitmapSource bitmap)
        {
            if (bitmap != null)
            {
                var transform = new ScaleTransform();
                var scale = Math.Min(48 / bitmap.Height, 48 / bitmap.Width);
                transform.ScaleX = scale;
                transform.ScaleY = scale;
                var scaledBitmap = new TransformedBitmap(bitmap, transform);
                return scaledBitmap;
            }

            return null;
        }

        private void LoadThumbnail()
        {
            try
            {
                var previewImage = ImageService.ExternalGetPreviewImageFromPath(FileItem.itemId);

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _thumbnail = ScaleThumbnail(previewImage);
                    _isThumbnailLoaded = true;
                    _isThumbnailLoading = false;

                    Notify(nameof(FileThumbnail));
                }));
            }
            catch
            {
            }
        }
    }
}
