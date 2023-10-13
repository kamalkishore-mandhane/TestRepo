using SafeShare.Util;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SafeShare.WPFUI.ViewModel
{
    public enum ExeFrom
    {
        SELF,
        PDFUTIL,
        IMGUTIL,
        WINZIPGUI,
        SHELLMENU,
        DRAG
    }

    internal class SafeShareViewModel : ViewModelBase
    {
        private SafeShareView _safeShareView;
        private Task _delayLoadWinzipSharedService;
        private JobManagement _jobManagement;
        private ExeFrom _callFrom = ExeFrom.SELF;

        public SafeShareViewModel(SafeShareView view, Action<bool> adjustPaneCursor) : base(view.WindowHandle, adjustPaneCursor)
        {
            _safeShareView = view;
        }

        public ExeFrom CallFrom
        {
            get => _callFrom;
            set
            {
                if (_callFrom != value)
                {
                    _callFrom = value;
                }
            }
        }

        public bool WinZipSessionCreated
        {
            get;
            private set;
        }

        protected override bool HandleException(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void SetLoadWinzipSharedService()
        {
            _safeShareView.AdjustPaneCursor(false);
            var viewModel = _safeShareView.DataContext as SafeShareViewModel;
            _delayLoadWinzipSharedService = Task.Factory.StartNew(() =>
            {
                // service id: 5 APPLET_SAFE_SHARE
                string accessPermisson = "ALL";
                string launchedContext = "/launchedContext normal";
                if (viewModel.CallFrom == ExeFrom.SHELLMENU)
                {
                    launchedContext = "/launchedContext shellext";
                }
                else if (viewModel.CallFrom == ExeFrom.DRAG)
                {
                    launchedContext = "/launchedContext dnd";
                }
                _safeShareView.WinzipSharedServiceHandle = WinzipMethods.CreateSession(5, accessPermisson, launchedContext);
            }).ContinueWith(task =>
            {
                try
                {
                    if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                    {
                        _jobManagement = new JobManagement();
                        _jobManagement.AddProcess(_safeShareView.WinzipSharedServiceHandle);
                    }

                    if (_safeShareView.IsVisible)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            while (true)
                            {
                                if (_safeShareView.WindowHandle != IntPtr.Zero)
                                {
                                    break;
                                }

                                Thread.Sleep(10);
                            }
                        }).WaitWithMsgPump();
                    }

                    if (_safeShareView.IsClosing)
                    {
                        if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                        {
                            WinzipMethods.DestroySession(_safeShareView.WinzipSharedServiceHandle);
                        }
                        _delayLoadWinzipSharedService = null;
                        return;
                    }

#if WZ_APPX
                    bool ret = true;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        ret = WinzipMethods.ShowUWPSubscription(_safeShareView.SafeHandle);
                    }));

                    if (!ret)
                    {
                        if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
#else
                    if (WinzipMethods.IsInGracePeriod(_safeShareView.WindowHandle))
                    {
                        // winzip is in grace period, load grace banner
                        int gracePeriodIndex = 0;
                        int graceDaysRemaining = 0;
                        string userEmail = string.Empty;
                        if (!WinzipMethods.GetGracePeriodInfo(_safeShareView.WindowHandle, ref gracePeriodIndex, ref graceDaysRemaining, ref userEmail))
                        {
                            if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _safeShareView.LoadGraceBannerFrame(gracePeriodIndex, graceDaysRemaining, userEmail);
                            }));
                        }

                        // show grace period dialog
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinzipMethods.ShowGracePeriodDialog(_safeShareView.SafeHandle, false);
                        }));

                        // show grace period dialog return false, close applet
                        if (!ret)
                        {
                            if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                    }
                    else
                    {
                        // in normal trial period, load trial banner
                        int nagIndex = 0;
                        int trialDaysRemaining = 0;
                        bool isAlreadyRegistered = false;
                        string buyNowUrl = string.Empty;

                        if (!WinzipMethods.GetTrialPeriodInfo(_safeShareView.WindowHandle, ref nagIndex, ref trialDaysRemaining, ref isAlreadyRegistered, ref buyNowUrl))
                        {
                            if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                _safeShareView.LoadNagBannerFrame(nagIndex, trialDaysRemaining, isAlreadyRegistered, buyNowUrl);
                            }));
                        }

                        // show nag
                        bool ret = true;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            ret = WinzipMethods.ShowNag(_safeShareView.SafeHandle);
                        }));

                        // show nag return false, close applet
                        if (!ret)
                        {
                            if (_safeShareView.WinzipSharedServiceHandle != IntPtr.Zero)
                            {
                                CloseAppletWhileLoadSharedService();
                                return;
                            }
                        }
                    }
#endif
                    int licenseType = -1;
                    WinzipMethods.GetLicenseStatus(_safeShareView.SafeHandle, ref licenseType);

                    if (licenseType != (int)WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION)
                    {
                        if (!WinzipMethods.CheckLicense(_safeShareView.SafeHandle))
                        {
                            CloseAppletWhileLoadSharedService();
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    CloseAppletWhileLoadSharedService();
                    return;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _safeShareView.AdjustPaneCursor(true);

                    if (_safeShareView.IsLoaded)
                    {
                        _safeShareView.Activate();
                        _safeShareView.Focus();
                        _safeShareView.FocusSelectFileButton();
                    }
                }));
                WinZipSessionCreated = true;
                // Start initializing necessary data async after WinZip shared service loaded
                WorkFlowManager.Initializer.StartInit();
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

        private void CloseAppletWhileLoadSharedService()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _delayLoadWinzipSharedService = null;
                _safeShareView.Close();
            }));
        }
    }
}