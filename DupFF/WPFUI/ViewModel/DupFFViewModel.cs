using DupFF.Utils;
using DupFF.WPFUI.Controls;
using DupFF.WPFUI.Utils;
using DupFF.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace DupFF.WPFUI.ViewModel
{
    class DupFFViewModel : ModelBase
    {
        public event EventHandler WinZipLoaded;

        private DupFFView _view;
        private RibbonTabViewModel _ribbonTabViewModel;
        private ObservableCollection<DisplayItem> _items;
        private List<DisplayItem> _selectedItems;
        private ObservableCollection<ActionItem> _actionItems;
        private List<ActionItem> _selectedActionItems;
        private int _selectedTabIndex = 0;

        private bool _isWinZipLoaded = false;
        private JobManagement _jobManagement;
        private Task _delayLoadWinzipSharedService;
        private DispatcherTimer _timer;
        private static Mutex createSessionMutex = new Mutex(false, "createSessionMutex");

        public Func<Func<Task>, Func<Exception, int>, Task> Executor => _executor;

        public DupFFViewModel(DupFFView view, Action<bool> adjustPaneCursor) : base(adjustPaneCursor)
        {
            _view = view;

            WinZipLoaded += (sender, e) =>
            {
                var str = WinZipMethods.GetBGToolInfos(IntPtr.Zero, _view.ViewType);
                RibbonTabViewModel.UpdateDisplayItem(str);
                str = WinZipMethods.GetBGTRNInfos(IntPtr.Zero, _view.ViewType);
                RibbonTabViewModel.UpdateActionItem(str);

                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                _timer.Tick += (s, ea) =>
                {
                    var infos = WinZipMethods.RefreshBGToolsStatus(IntPtr.Zero);
                    RibbonTabViewModel.UpdateDisplayItem(infos);
                    var updateInfo = WinZipMethods.GetBGTRNInfos(IntPtr.Zero, DupFFView.MainWindow.ViewType);
                    RibbonTabViewModel.UpdateActionItem(updateInfo);
                };
                _timer.Start();
            };
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

        public ObservableCollection<DisplayItem> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<DisplayItem>();
                }

                return _items;
            }
        }

        public List<DisplayItem> SelectedItems
        {
            get
            {
                if (_selectedItems == null)
                {
                    _selectedItems = new List<DisplayItem>();
                }

                return _selectedItems;
            }
            set
            {
                _selectedItems = value;
            }
        }

        public ObservableCollection<ActionItem> ActionItems
        {
            get
            {
                if (_actionItems == null)
                {
                    _actionItems = new ObservableCollection<ActionItem>();
                }

                return _actionItems;
            }
        }

        public List<ActionItem> SelectedActionItems
        {
            get
            {
                if (_selectedActionItems == null)
                {
                    _selectedActionItems = new List<ActionItem>();
                }

                return _selectedActionItems;
            }
            set
            {
                _selectedActionItems = value;
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
            _view.EnableView(false);

            _delayLoadWinzipSharedService = Task.Factory.StartNew(() =>
            {
                // service id: 4 APPLET_DUPFF
                createSessionMutex.WaitOne();
                WinZipMethods.CreateSession(4, "INHERIT", additionalCMDParameters);
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

                    if (_view.IsClosing)
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
                            if (_view.WindowHandle != IntPtr.Zero)
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
                        ret = WinZipMethods.ShowUWPSubscription(_view.WindowHandle);
                    }));

                    if (!ret)
                    {
                        CloseAppletWhileLoadSharedService();
                        return;
                    }
#else

                    // show nag or grace period dialog
                    bool ret = true;
                    bool isInGracePeriod = WinZipMethods.IsInGracePeriod(_view.WindowHandle);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        ret = isInGracePeriod ? WinZipMethods.ShowGracePeriodDialog(_view.WindowHandle, false) : WinZipMethods.ShowNag(_view.WindowHandle);
                    }));

                    if (!ret)
                    {
                        CloseAppletWhileLoadSharedService();
                        return;
                    }
#endif
                }
                catch
                {
                    CloseAppletWhileLoadSharedService();
                    return;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (_view.IsLoaded)
                    {
                        _view.Activate();
                        _view.Focus();
                    }
                    _view.EnableView(true);
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

                        _view.LastPdfWindowLeft = rect.left;
                        _view.LastPdfWindowTop = rect.top;
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
                _view.Close();
            }));
        }
    }
}
