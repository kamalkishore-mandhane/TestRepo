using SBkUpUI.Utils;
using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SBkUpUI.WPFUI.ViewModel
{
    class SBkUpViewModel : ModelBase
    {
        public event EventHandler WinZipLoaded;

        private SBkUpView _sbkUpView;
        private RibbonTabViewModel _ribbonTabViewModel;
        private ObservableCollection<JobItem> _items;
        private List<JobItem> _selectedItems;
        private ObservableCollection<Backup> _backups;
        private JobItem _selectedJob;
        private Backup _selectedBackup;
        private int _selectedTabIndex;
        private bool _overWrite = false;

        private Task _delayLoadWinzipSharedService;
        private JobManagement _jobManagement;
        private bool _isWinZipLoaded = false;
        private FileSystemWatcher _swjfWatcher;
        private FileSystemWatcher _jobFolderWatcher;
        private static Mutex createSessionMutex = new Mutex(false, "createSessionMutex");

        public SBkUpViewModel(SBkUpView view, Action<bool> adjustPaneCursor) : base(adjustPaneCursor)
        {
            _sbkUpView = view;
            SBkUpViewModelInstance = this;
            _backups = new ObservableCollection<Backup>();
            _selectedTabIndex = 0;

            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            JobFolder = Path.Combine(documentsFolder, "WinZip Backups");
            if (!Directory.Exists(JobFolder))
            {
                Directory.CreateDirectory(JobFolder);
            }

            var files = Util.GetAllSwjfFiles(JobFolder);
            for (int i = 0; i < files.Length; i++)
            {
                var jobItem = new JobItem(files[i].FullName);
                if (jobItem.LoadFail)
                {
                    continue;
                }
                Task_Scheduler.TryInitJobItemFromTask(jobItem);
                WinZipLoaded += (sender, e) => { jobItem.LoadBackups(); };
                Items.Add(jobItem);
                jobItem.PropertyChanged += JobItem_PropertyChanged;
            }

            var missingCannedSwjfs = Util.GetMissingCannedSwjf(Items);
            foreach (var canned in missingCannedSwjfs)
            {
                var swjf = Util.GetCannedSwjf(canned);
                if (Directory.Exists(swjf.backupFolder.itemId))
                {
                    var path = Path.Combine(JobFolder, Util.GetCannedSwjfName(canned));
                    if (swjf.Save(path))
                    {
                        var job = new JobItem(path);
                        if (job.LoadFail)
                        {
                            continue;
                        }
                        Task_Scheduler.TryInitJobItemFromTask(job);
                        WinZipLoaded += (sender, e) => { job.LoadBackups(); };
                        Items.Add(job);
                        job.PropertyChanged += JobItem_PropertyChanged;
                    }
                }
            }
            SortItems();

            _swjfWatcher = new FileSystemWatcher(JobFolder, "*" + Swjf.Extension);
            _swjfWatcher.Deleted += FileSystemWatcher_Deleted;
            _swjfWatcher.Renamed += FileSystemWatcher_Renamed;
            _swjfWatcher.IncludeSubdirectories = false;
            _swjfWatcher.EnableRaisingEvents = true;

            _jobFolderWatcher = new FileSystemWatcher(documentsFolder, Util.GetFolderName(JobFolder));
            _jobFolderWatcher.Deleted += FolderWatcher_Deleted;
            _jobFolderWatcher.Renamed += FolderWatcher_Renamed;
            _jobFolderWatcher.IncludeSubdirectories = true;
            _jobFolderWatcher.EnableRaisingEvents = true;
        }

        private void FolderWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath.ToLower() == JobFolder.ToLower())
            {
                _sbkUpView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    DeleteAllItems();
                }));
            }
        }

        private void FolderWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.ToLower() == JobFolder.ToLower())
            {
                _sbkUpView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    DeleteAllItems();
                }));
            }
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            foreach (var item in Items)
            {
                if (item.FilePath.ToLower() == e.OldFullPath.ToLower())
                {
                    _sbkUpView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        DeleteOneItem(item);
                    }));
                    return;
                }
            }
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            foreach (var item in Items)
            {
                if (item.FilePath.ToLower() == e.FullPath.ToLower())
                {
                    _sbkUpView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        DeleteOneItem(item);
                    }));
                    return;
                }
            }
        }

        private void DeleteOneItem(JobItem item)
        {
            try
            {
                SelectedItems.Clear();
                Items.Remove(item);
                Task_Scheduler.TryDeleteTask(item);
            }
            catch (Exception)
            {

            }
        }

        private void DeleteAllItems()
        {
            try
            {
                foreach (var item in Items)
                {
                    Task_Scheduler.TryDeleteTask(item);
                }
            }
            catch
            {

            }
            SelectedItems.Clear();
            Items.Clear();
        }

        public void JobItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is JobItem job)
            {
                if (e.PropertyName == nameof(job.Backups))
                {
                    if (SelectedJob == job)
                    {
                        Backups.Clear();
                        foreach (var item in job.Backups)
                        {
                            Backups.Add(item);
                        }
                        Notify(nameof(Backups));
                    }
                    return;
                }
                if (e.PropertyName != nameof(job.Running) && e.PropertyName != nameof(job.IsSelected))
                {
                    Task_Scheduler.TrySaveJobItemToTask(job, Owner);
                }
            }
        }

        public static string JobFolder
        {
            get; set;
        }

        public ObservableCollection<JobItem> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<JobItem>();
                }

                return _items;
            }
        }

        public ObservableCollection<Backup> Backups
        {
            get
            {
                return _backups;
            }
            set
            {
                if (_backups != value)
                {
                    _backups = value;
                }
            }
        }

        public List<JobItem> SelectedItems
        {
            get
            {
                if (_selectedItems == null)
                {
                    _selectedItems = new List<JobItem>();
                }

                return _selectedItems;
            }
            set
            {
                _selectedItems = value;
            }
        }

        public JobItem SelectedJob
        {
            get
            {
                return _selectedJob;
            }
            set
            {
                _selectedJob = value;
                Notify(nameof(SelectedJob));
            }
        }

        public Backup SelectedBackup
        {
            get
            {
                return _selectedBackup;
            }
            set
            {
                _selectedBackup = value;
                Notify(nameof(SelectedBackup));
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                Notify(nameof(SelectedTabIndex));
            }
        }

        public RibbonTabViewModel RibbonTabViewModel
        {
            get
            {
                if (_ribbonTabViewModel == null)
                {
                    _ribbonTabViewModel = new RibbonTabViewModel(this);
                }

                return _ribbonTabViewModel;
            }
        }

        public bool OverWrite
        {
            get
            {
                return _overWrite;
            }
            set
            {
                _overWrite = value;
            }
        }

        public FileSystemWatcher SwjfWatcher
        {
            get
            {
                return _swjfWatcher;
            }
        }

        public static SBkUpViewModel SBkUpViewModelInstance
        {
            get;
            private set;
        }

        public IntPtr Owner => _sbkUpView.WindowHandle;

        private void SortItems()
        {
            var sortableList = new List<JobItem>(_items);
            sortableList.Sort((x, y) =>
            {
                if (x.Swjf.isCanned == y.Swjf.isCanned)
                {
                    return string.Compare(y.Name, x.Name, true) * (-1);
                }
                return x.Swjf.isCanned ? -1 : 1;
            });

            for (int i = 0; i < sortableList.Count; i++)
            {
                _items.Move(_items.IndexOf(sortableList[i]), i);
            }
            Notify(nameof(Items));
        }

        public Func<Func<Task>, Func<Exception, int>, Task> Executor => _executor;

        public bool IsWinZipLoaded
        {
            get
            {
                return _isWinZipLoaded;
            }
            private set
            {
                if (_isWinZipLoaded != value)
                {
                    _isWinZipLoaded = value;
                    if (_isWinZipLoaded)
                    {
                        WinZipLoaded(this, new EventArgs());
                    }
                }
            }
        }

        public void SetLoadWinzipSharedService(IntPtr parentHandle, string additionalCMDParameters)
        {
            IsWinZipLoaded = false;
            _sbkUpView.EnableSbkupView(false);

            _delayLoadWinzipSharedService = Task.Factory.StartNew(() =>
            {
                // service id: 3 APPLET_SECURE_BACKUP
                createSessionMutex.WaitOne();
                WinZipMethods.CreateSession(3, "INHERIT", additionalCMDParameters);
                createSessionMutex.ReleaseMutex();
            }).ContinueWith(task =>
            {
                try
                {
                    if (WinZipMethods.WinzipSharedServiceHandle != IntPtr.Zero)
                    {
                        _jobManagement = new JobManagement();
                        _jobManagement.AddProcess(WinZipMethods.WinzipSharedServiceHandle);
                    }

                    if (_sbkUpView.IsClosing)
                    {
                        if (WinZipMethods.WinzipSharedServiceHandle != IntPtr.Zero)
                        {
                            WinZipMethods.DestroySession(WinZipMethods.WinzipSharedServiceHandle);
                        }
                        _delayLoadWinzipSharedService = null;
                        return;
                    }

                    if (parentHandle != IntPtr.Zero)
                    {
                        NativeMethods.EnableWindow(parentHandle, false);
                    }

                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            if (_sbkUpView.WindowHandle != IntPtr.Zero)
                            {
                                break;
                            }

                            Thread.Sleep(10);
                        }

                    }).WaitWithMsgPump();

#if WZ_APPX
                    bool ret = true;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        ret = WinZipMethods.ShowUWPSubscription(_sbkUpView.WindowHandle);
                    }));

                    if (!ret)
                    {
                        CloseAppletWhileLoadSharedService();
                        return;
                    }
#else
                    if (WinZipMethods.IsInGracePeriod(_sbkUpView.WindowHandle))
                    {
                        // in grace period, load grace banner
                        int gracePeriodIndex = 0;
                        int graceDaysRemaining = 0;
                        string userEmail = string.Empty;
                        if (!WinZipMethods.GetGracePeriodInfo(_sbkUpView.WindowHandle, ref gracePeriodIndex, ref graceDaysRemaining, ref userEmail))
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _sbkUpView.LoadGraceBannerFrame(gracePeriodIndex, graceDaysRemaining, userEmail);
                            }));
                        }

                        // show grace period dialog
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinZipMethods.ShowGracePeriodDialog(_sbkUpView.WindowHandle, false);
                        }));

                        // show grace period dialog return false, close applet
                        if (!ret)
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
                    else
                    {
                        // in normal trial period, load trial banner
                        int nagIndex = 0;
                        int trialDaysRemaining = 0;
                        bool isAlreadyRegistered = false;
                        string buyNowUrl = string.Empty;

                        if (!WinZipMethods.GetTrialPeriodInfo(_sbkUpView.WindowHandle, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, ref buyNowUrl))
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _sbkUpView.LoadNagBannerFrame(nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
                            }));
                        }

                        // show nag
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinZipMethods.ShowNag(_sbkUpView.WindowHandle);
                        }));

                        // show nag return false, close applet
                        if (!ret)
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
#endif

                    int licenseType = -1;
                    WinZipMethods.GetLicenseStatus(_sbkUpView.WindowHandle, ref licenseType);

                    if (licenseType != (int)WinZipMethods.WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION)
                    {
                        if (!WinZipMethods.CheckLicense(_sbkUpView.WindowHandle))
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
                }
                catch
                {
                    CloseAppletWhileLoadSharedService();
                    return;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (_sbkUpView.IsLoaded)
                    {
                        _sbkUpView.Activate();
                        _sbkUpView.Focus();
                    }
                    _sbkUpView.EnableSbkupView(true);
                    IsWinZipLoaded = true;
                }));
            });
        }

        public void WaitLoadWinzipSharedService()
        {
            if (_delayLoadWinzipSharedService != null)
            {
                _delayLoadWinzipSharedService.WaitWithMsgPump();
                _delayLoadWinzipSharedService = null;
            }
        }

        public void DisposeJobManagement()
        {
            if (_jobManagement != null)
            {
                //RemoveAssociatedCloseProcess
                //A normal exit does not require the Job to execute AssociatedClose
                _jobManagement.RemoveAssociatedCloseProcess();
                _jobManagement.Dispose();
            }
        }

        public void CalLastWindowPostion(string assemblyName)
        {
            var moduleFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var appName = System.IO.Path.GetFileName(moduleFilePath);

            if (appName.Contains(".bin"))
            {
                assemblyName = appName;
            }

            var rawProcesses = Process.GetProcessesByName(assemblyName);
            var processes = FilterAccessDeniedProcesses(rawProcesses);
            if (processes.Length > 1)
            {
                var windowFinder = new MainWindowFinder();
                Array.Sort(processes, ProcessStartTimeCompare);

                int lastIndex = processes.Length - 2;
                for (int i = lastIndex; i >= 0; i--)
                {
                    int lastWindowProcessId = processes[i].Id;
                    var hwnd = windowFinder.FindMainWindow(lastWindowProcessId);
                    if (hwnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hwnd) && !NativeMethods.IsIconic(hwnd))
                    {
                        var rect = new NativeMethods.RECT();
                        NativeMethods.GetWindowRect(hwnd, out rect);

                        _sbkUpView.LastPdfWindowLeft = rect.left;
                        _sbkUpView.LastPdfWindowTop = rect.top;
                        break;
                    }
                }
            }
        }

        private Process[] FilterAccessDeniedProcesses(Process[] processes)
        {
            var filteredProcesses = new List<Process>();
            foreach (Process process in processes)
            {
                try
                {
                    var tryToGetStartTime = process.StartTime;
                    filteredProcesses.Add(process);
                }
                catch (Win32Exception)
                {
                    // catch Win32Exception means it is an abnormal process, just ignore it
                    continue;
                }
            }
            return filteredProcesses.ToArray();
        }

        private int ProcessStartTimeCompare(Process firstProcess, Process secondProcess)
        {
            return firstProcess.StartTime.CompareTo(secondProcess.StartTime);
        }

        private void CloseAppletWhileLoadSharedService()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _delayLoadWinzipSharedService = null;
                _sbkUpView.Close();
            }));
        }
    }
}
