using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.View;
using SBkUpUI.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace SBkUpUI.WPFUI.Controls
{
    public class JobItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _name;
        bool _isEnabled;
        int _frequency = 2;
        string _filePath;
        Swjf _swjf;
        bool _selected;
        bool _running;
        bool _loadFail;
        ObservableCollection<Backup> _backups;

        DateTime _time;
        DateTime _date;

        public JobItem(string filePath)
        {
            _filePath = filePath;
            _name = Path.GetFileNameWithoutExtension(_filePath);
            _loadFail = false;
            _date = DateTime.Now;
            _time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 30, 0);
            if (DateTime.Now.Minute >= 30)
            {
                _time = _time.AddMinutes(30);
            }
            _swjf = new Swjf();
            if (!_swjf.Load(_filePath))
            {
                _loadFail = true;
                return;
            }

            if (_swjf.isCanned)
            {
                _name = Util.GetCannedJobDisPlayName(_name);
            }

            _backups = new ObservableCollection<Backup>();
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name 
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
                    Notify(nameof(Name));
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    Notify(nameof(IsEnabled));

                    TrackHelper.LogEnableSBkUpEvent(this, value);
                }
            }
        }

        public int Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    Notify(nameof(Frequency));
                }
            }
        }
        
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    Notify(nameof(Time));
                }
            }
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    Notify(nameof(Date));
                }
            }
        }

        public bool ModifiedByUser
        {
            get; set;
        }

        public Swjf Swjf
        {
            get
            {
                return _swjf;
            }
            set
            {
                if (_swjf != value)
                {
                    _swjf = value;
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Notify(nameof(IsSelected));
                }
            }
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        public bool Running
        {
            get
            {
                return _running;
            }
            set
            {
                if (_running != value)
                {
                    _running = value;
                    Notify(nameof(Running));
                }
            }
        }

        public ObservableCollection<Backup> Backups
        {
            get
            {
                return _backups;
            }
        }

        public bool LoadFail
        {
            get
            {
                return _loadFail;
            }
        }

        public void LoadBackups()
        {
            Task.Factory.StartNew(() =>
            {
                _backups.Clear();
                if (WinZipMethods.IsCloudItem(_swjf.storeFolder.profile.Id) || _swjf.storeFolder.profile.Id == WinZipMethods.WzSvcProviderIDs.SPID_LOCAL_NETWORK)
                {
                    var list = new List<WinZipMethods.WzCloudItem4>();
                    if (!WinZipMethods.ListItems(IntPtr.Zero, _swjf.storeFolder, ref list))
                    {
                        // try to update the storeFolder
                        var path = Util.GetRestoreFolderStorageText(in _swjf.storeFolder);
                        path = Path.DirectorySeparatorChar + path;
                        var rootItem = _swjf.storeFolder;
                        rootItem.itemId = string.Empty;
                        rootItem.path = string.Empty;
                        while (WinZipMethods.ListItems(IntPtr.Zero, rootItem, ref list))
                        {
                            bool ifContinue = false;
                            bool found = false;
                            foreach (var item in list)
                            {
                                if (item.isFolder && path.Equals(item.path, StringComparison.OrdinalIgnoreCase))
                                {
                                    _swjf.storeFolder.itemId = item.itemId;
                                    if (item.itemId.EndsWith("\\*.*"))
                                    {
                                        _swjf.storeFolder.itemId = item.itemId.Substring(0, item.itemId.Length - "\\*.*".Length);
                                    }
                                    found = true;
                                    break;
                                }
                                else if (item.isFolder && path.StartsWith(item.path + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                                {
                                    rootItem = item;
                                    ifContinue = true;
                                    break;
                                }
                            }
                            if (found)
                            {
                                SBkUpViewModel.SBkUpViewModelInstance.SwjfWatcher.EnableRaisingEvents = false;
                                _swjf.Save(_filePath);
                                SBkUpViewModel.SBkUpViewModelInstance.SwjfWatcher.EnableRaisingEvents = true;
                                WinZipMethods.ListItems(IntPtr.Zero, _swjf.storeFolder, ref list);
                                break;
                            }
                            list.Clear();
                            if (ifContinue)
                            {
                                continue;
                            }
                            break;
                        }
                    }

                    foreach (var item in list)
                    {
                        if (IsBackup(item))
                        {
                            var bk = new Backup(item);
                            if (!string.IsNullOrEmpty(bk.Name))
                            {
                                _backups.Add(new Backup(item));
                            }
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(_swjf.storeFolder.itemId))
                    {
                        var dirInfo = new DirectoryInfo(_swjf.storeFolder.itemId);
                        var Files = dirInfo.GetFiles();
                        foreach (var file in Files)
                        {
                            var item = WinZipMethods.InitCloudItemFromPath(file.FullName);
                            if (IsBackup(item))
                            {
                                var bk = new Backup(item);
                                if (!string.IsNullOrEmpty(bk.Name))
                                {
                                    _backups.Add(new Backup(item));
                                }
                            }
                        }
                    }
                }
                SortBackups();
                SBkUpView.MainWindow.Dispatcher.Invoke(new Action(() => { Notify(nameof(Backups)); })); ;
            });
        }

        private void SortBackups()
        {
            var sortableList = new List<Backup>(_backups);
            sortableList.Sort((x, y) => { return string.Compare(y.Name, x.Name); });
            for (int i = 0; i < sortableList.Count; i++)
            {
                _backups.Move(_backups.IndexOf(sortableList[i]), i);
            }
        }

        private bool IsBackup(WinZipMethods.WzCloudItem4 item)
        {
            if (Swjf.limitMaxBackupNumber)
            {
                return CheckBackupByTimestamp(item, Path.GetFileNameWithoutExtension(Swjf.zipName));
            }
            else
            {
                return !item.isFolder && Path.GetFileName(item.path) == Swjf.zipName;
            }
        }

        private bool CheckBackupByTimestamp(WinZipMethods.WzCloudItem4 item, string zipName)
        {
            if (item.isFolder || item.path.Length <= Backup.BackupExtension.Length + Backup.Timestamp.Length)
            {
                return false;
            }

            if (!Path.GetFileName(item.path).StartsWith(zipName))
            {
                return false;
            }

            if (!item.path.EndsWith(Backup.BackupExtension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var tempName = item.path.Substring(item.path.Length - Backup.BackupExtension.Length - Backup.Timestamp.Length, Backup.Timestamp.Length);

            try
            {
                DateTime myDate = DateTime.ParseExact(tempName, Backup.Timestamp,
                                       System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
