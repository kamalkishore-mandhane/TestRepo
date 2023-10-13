using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using SafeShare.Util;

namespace SafeShare.WPFUI.ViewModel
{
    public class FileListPageViewModel : ViewModelBase
    {
        private const int DefaultItemsCount = 256;
        private const int ListViewItemHeight = 45;
        private const int ListViewItemMinVisibleCount = 4;
        private const int ListViewItemMaxVisibleCount = 10;
        private double _defaultPageHeight;
        private FileListPage _fileListPage;
        private FileListPageViewModelCommand _viewModelCommands = null;
        private List<string> _itemsFullPathList;
        private long _fileTotalSize;
        private string _itemTotalCount;
        private string _totalUnzipSize;
        private bool _showSelectFileTips;

        public FileListPageViewModel(FileListPage view, Action<bool> adjustPaneCursor) : base(IntPtr.Zero, adjustPaneCursor)
        {
            _fileListPage = view;
            ListViewItems = new ItemSource();
            _itemsFullPathList = new List<string>();
            TrackHelper.TrackHelperInstance.PathToTrackDic.Clear();
            _fileTotalSize = 0;
            ItemTotalCount = string.Format(Properties.Resources.TOTAL_ITEM_COUNT, 0);
            _defaultPageHeight = view.Height;
        }

        public FileListPageViewModelCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new FileListPageViewModelCommand(this);
                }
                return _viewModelCommands;
            }
        }

        public ItemSource ListViewItems
        {
            set;
            get;
        }

        public List<string> ItemsFullPathList
        {
            get
            {
                return _itemsFullPathList;
            }
        }

        public string TotalUnzipSize
        {
            get
            {
                return _totalUnzipSize;
            }
            set
            {
                if (_totalUnzipSize != value)
                {
                    _totalUnzipSize = value;
                    Notify(nameof(TotalUnzipSize));
                }
            }
        }

        public long FileTotalSize
        {
            get
            {
                return _fileTotalSize;
            }
            set
            {
                if (_fileTotalSize != value)
                {
                    _fileTotalSize = value;
                    Notify(nameof(FileTotalSize));
                    TotalUnzipSize = string.Format(Properties.Resources.TOTAL_UNZIP_SIZE, FileSizeConverter.StrFormatByteSize(FileTotalSize), ItemTotalCount);
                }
            }
        }

        public string ItemTotalCount
        {
            get
            {
                return _itemTotalCount;
            }
            set
            {
                if (_itemTotalCount != value)
                {
                    _itemTotalCount = value;
                    Notify(nameof(ItemTotalCount));
                    TotalUnzipSize = string.Format(Properties.Resources.TOTAL_UNZIP_SIZE, FileSizeConverter.StrFormatByteSize(FileTotalSize), ItemTotalCount);
                }
            }
        }

        public bool ShowSelectFileTips
        {
            get => _showSelectFileTips;
            set
            {
                if (_showSelectFileTips != value)
                {
                    _showSelectFileTips = value;
                    Notify(nameof(ShowSelectFileTips));
                }
            }
        }

        public void CalcutTotalItems()
        {
            if (ListViewItems.ItemCount == 1)
            {
                ItemTotalCount = string.Format(Properties.Resources.TOTAL_ITEM_COUNT1, ListViewItems.ItemCount);
            }
            else
            {
                ItemTotalCount = string.Format(Properties.Resources.TOTAL_ITEM_COUNT, ListViewItems.ItemCount);
            }
        }

        public void CalcutListViewHeight()
        {
            var visibleCount = ListViewItems.ItemCount;
            if (visibleCount <= ListViewItemMinVisibleCount)
            {
                visibleCount = ListViewItemMinVisibleCount;
            }

            if (visibleCount >= ListViewItemMaxVisibleCount)
            {
                visibleCount = ListViewItemMaxVisibleCount;
            }

            _fileListPage.Height = (visibleCount - ListViewItemMinVisibleCount) * ListViewItemHeight + _defaultPageHeight + ListViewItemHeight / 2;
        }

        private void ChangeListViewContent(string fullPath, ListViewItemEntry itemEntry, bool fromPicker)
        {
            if (!_itemsFullPathList.Contains(fullPath))
            {
                ListViewItems.ItemSources.Add(itemEntry);
                _itemsFullPathList.Add(fullPath);
                TrackHelper.TrackHelperInstance.PathToTrackDic.Add(fullPath, fromPicker);
                CalcutTotalItems();
                CalcutListViewHeight();

                if (Directory.Exists(fullPath))
                {
                    itemEntry.Size = GetDirectorySize(fullPath, 0);
                    FileTotalSize += itemEntry.Size;
                }
                else
                {
                    itemEntry.Size = GetFileSize(fullPath);
                    FileTotalSize += itemEntry.Size;
                }
            }
        }

        public void ExecuteDragFromExploreTaskCommand(string[] files)
        {
            foreach (var file in files)
            {
                var name = string.Empty;
                if (Directory.Exists(file))
                {
                    var lastChar = file[file.Length - 1];
                    if (lastChar != '\\' && lastChar != '\\')
                    {
                        name = Path.GetFileName(file);
                    }
                    else
                    {
                        name = Path.GetFileName(Path.GetDirectoryName(file));
                    }
                }
                else if (File.Exists(file))
                {
                    name = Path.GetFileName(file);
                }
                else
                {
                    continue;
                }

                var item = new ListViewItemEntry(name, file);
                ChangeListViewContent(file, item, false);
            }
        }

        public void ExecuteOpenFromFilePickerTaskCommand(WzCloudItem4[] items)
        {
            foreach (var item in items)
            {
                var name = string.Empty;
                if (item.isFolder)
                {
                    var lastChar = item.itemId[item.itemId.Length - 1];
                    if (lastChar != '\\' && lastChar != '\\')
                    {
                        name = Path.GetFileName(item.itemId);
                    }
                    else
                    {
                        name = Path.GetFileName(Path.GetDirectoryName(item.itemId));
                    }
                }
                else
                {
                    if (WinZipMethodHelper.IsLocalPortableDeviceItem(item.profile.Id))
                    {
                        name = item.name;
                    }
                    else
                    {
                        name = Path.GetFileName(item.itemId);
                    }
                }

                var itemEntry = new ListViewItemEntry(name, item.itemId);
                ChangeListViewContent(item.itemId, itemEntry, true);
            }
        }

        public void ExecuteContinueAction()
        {
            var mainWindow = VisualTreeHelperUtils.FindAncestor<Window>(_fileListPage) as NavigationWindow;

            if (ListViewItems.ItemCount == 0
                || (ItemsFullPathList.Count == 1 && Path.GetFileName(ItemsFullPathList[0]).Equals("*.*")))
            {
                WorkFlowManager.GoFrontPage(_fileListPage);
            }
            else if (mainWindow.CanGoForward)
            {
                WorkFlowManager.UpdateEmailEncryptionPageData(_fileListPage);
                mainWindow.GoForward();
            }
            else
            {
                WorkFlowManager.ShareOptionData.ItemsPath = (_fileListPage.DataContext as FileListPageViewModel).ItemsFullPathList;
                WorkFlowManager.GoEmailEncryptionPage(_fileListPage);
            }
        }

        public void ExecuteAddFilesCommand()
        {
            _executor(() => ExecuteAddFilesTask(), RetryStrategy.Create(false, 0));
        }

        private Task ExecuteAddFilesTask()
        {
            return Task.Factory.StartNewTCS(tcs =>
            {
                var title = Properties.Resources.SELECT_FILES;
                var defaultBtn = Properties.Resources.SELECT_BUTTON_TITLE;
                var filters = Properties.Resources.OPEN_PICKER_FILTERS;
                int count = DefaultItemsCount;
                var defaultFolder = WinZipMethodHelper.GetOpenPickerDefaultFolder();
                var preSelectedItems = new WzCloudItem4[count];

                bool ret = WinzipMethods.FileSelection(_fileListPage.MainWindow.WindowHandle, title, defaultBtn, filters, defaultFolder, preSelectedItems,
                    ref count, true, true, true, true, true, false);

                if (ret)
                {
                    // The desired behavior is to select nothing if nothing is selected.
                    if (count == 1 && preSelectedItems[0].isFolder && preSelectedItems[0].itemId.EndsWith("\\*.*"))
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    var selectedItems = new WzCloudItem4[count];
                    Array.Copy(preSelectedItems, selectedItems, count);

                    WinZipMethodHelper.SetOpenPickerDefaultFolder(selectedItems[0]);
                    WinZipMethodHelper.SetSavePickerDefaultFolder(selectedItems[0]);
                    var isCloudItem = WinZipMethodHelper.IsCloudItem(selectedItems[0].profile.Id);
                    var isLocalPortableDeviceItem = WinZipMethodHelper.IsLocalPortableDeviceItem(selectedItems[0].profile.Id);

                    if (isCloudItem || isLocalPortableDeviceItem)
                    {
                        var res = WinZipMethodHelper.DownloadCloudItems(_fileListPage.MainWindow.WindowHandle, ref selectedItems);
                        if (!res)
                        {
                            tcs.TrySetCanceled();
                            return;
                        }
                    }
                    else
                    {
                        if (!EDPHelper.CheckProtectedFiles(selectedItems))
                        {
                            tcs.TrySetCanceled();
                            return;
                        }
                    }

                    ExecuteOpenFromFilePickerTaskCommand(selectedItems);
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            });
        }

        public void ExecuteRemoveFilesCommand()
        {
            var selectedEntries = new ListViewItemEntry[_fileListPage.fileListView.SelectedItems.Count];
            _fileListPage.fileListView.SelectedItems.CopyTo(selectedEntries, 0);
            long itemsSize = 0;
            foreach (var item in selectedEntries)
            {
                var itemEntry = item as ListViewItemEntry;
                ListViewItems.ItemSources.Remove(itemEntry);
                itemsSize += itemEntry.Size;
                ItemsFullPathList.Remove(itemEntry.FullPath);
                TrackHelper.TrackHelperInstance.PathToTrackDic.Remove(itemEntry.FullPath);
            }
            FileTotalSize -= itemsSize;
            CalcutTotalItems();
            CalcutListViewHeight();
            _fileListPage.fileListView.Focus();
        }

        public void ExecuteContinueCommand()
        {
            ExecuteContinueAction();
        }

        private long GetDirectorySize(string path, long fileSize)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }

            var directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                fileSize += file.Length;
            }

            foreach (var dir in directory.GetDirectories())
            {
                fileSize += GetDirectorySize(dir.FullName, 0);
            }

            return fileSize;
        }

        private long GetFileSize(string path)
        {
            if (!File.Exists(path))
            {
                return 0;
            }
            else
            {
                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
        }

        protected override bool HandleException(Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemSource : INotifyPropertyChanged
    {
        private UIObjects _itemSources = new UIObjects();

        public event PropertyChangedEventHandler PropertyChanged;

        private int _itemCount;

        public ItemSource()
        {
            ItemSources.CollectionChanged += ItemSources_CollectionChanged;
        }

        ~ItemSource()
        {
            ItemSources.CollectionChanged -= ItemSources_CollectionChanged;
        }

        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            private set
            {
                if (_itemCount != value)
                {
                    _itemCount = value;
                    Notify(nameof(ItemCount));
                }
            }
        }

        public UIObjects ItemSources
        {
            get
            {
                return _itemSources;
            }
            set
            {
                if (_itemSources != value)
                {
                    _itemSources = value;
                    Notify(nameof(ItemSources));
                }
            }
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ItemSources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var icons = sender as UIObjects;
            ItemCount = icons.Count;
        }
    }
}