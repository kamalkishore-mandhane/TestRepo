using ImgUtil.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace ImgUtil.WPFUI.Model
{
    class RecentFileList : INotifyPropertyChanged
    {
        private const int MaxRecentFileCount = 15;
        private Dispatcher _dispatcher;
        private ObservableCollection<RecentFile> _recentFileList;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RecentFileList(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _recentFileList = new ObservableCollection<RecentFile>();
        }

        public RecentFileList(ObservableCollection<RecentFile> data)
        {
            _recentFileList = data;
        }

        public RecentFile this[int index]
        {
            get
            {
                return _recentFileList[index];
            }
        }

        public int Count => _recentFileList.Count;

        public ObservableCollection<RecentFile> RecentListData
        {
            get
            {
                return _recentFileList;
            }
            set
            {
                _recentFileList = value;
                Notify(nameof(RecentListData));
            }
        }

        public void TryPush(RecentFile file)
        {
            _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                var index = TryFindIndex(file);

                if (index != -1)
                {
                    _recentFileList.RemoveAt(index);
                }

                _recentFileList.Insert(0, file);

                if (_recentFileList.Count > MaxRecentFileCount)
                {
                    _recentFileList.RemoveAt(MaxRecentFileCount);
                }

                RefreshRecentFileIndex();
            }));
        }

        private int TryFindIndex(RecentFile file)
        {
            int index = -1;
            for (int i = 0; i < _recentFileList.Count; ++i)
            {
                if ((_recentFileList[i].RecentFileTooltip == file.RecentFileTooltip) && (_recentFileList[i].RecentFileFullName == file.RecentFileFullName))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public void TryRemove(RecentFile file)
        {
            _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                var index = TryFindIndex(file);

                if (index != -1)
                {
                    _recentFileList.RemoveAt(index);
                    RefreshRecentFileIndex();
                }
            }));
        }

        public void TryRemove(WzCloudItem4 item)
        {
            var file = new RecentFile(item);
            TryRemove(file);
        }

        private void RefreshRecentFileIndex()
        {
            for (int i = 0; i < _recentFileList.Count; ++i)
            {
                _recentFileList[i].RecentFileIndex = (uint)i + 1;
            }

            Notify(nameof(RecentListData));
        }
    }
}
