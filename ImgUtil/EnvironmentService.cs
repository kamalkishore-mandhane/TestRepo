using ImgUtil.Util;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ImgUtil
{
    public enum CommandLineOperation
    {
        None,
        ModifyImage,
        CropImage,
        OpenPickerFromFilePane,
        OpenImageFromFilePane,
        OpenImageFromZipPane,
        DropImageOnIcon
    }

    internal static class EnvironmentService
    {
        #region Fields

        private static string Open_ImgUtil_From_WinZip_PIPE_NAME = "openimgutilfromwinzip_pipe_{0}";
        private static string Open_ImgUtil_From_WinZip_EVENT_NAME = "openimgutilfromwinzip_event_{0}";
        private static readonly Mutex createSessionMutex = new Mutex(false, "createSessionMutex");
        private static JobManagement _jobManagement;

        private const int inBufferSize = 4096;
        private const int outBufferSize = 65536;

        private static List<WzCloudItem4> _filePaneItems;
        private static List<string> _sourceFiles;

        #endregion

        #region Common Static Properties

        public static IntPtr WinzipSharedServiceHandle { get; set; }

        public static IntPtr WinZipHandle { get; set; }

        public static string WinZipProcessID { get; set; }

        public static bool IsCalledByWinZipFilePane { get; set; } = false;

        public static bool IsCalledByWinZipZipPane { get; set; } = false;

        public static CommandLineOperation CommandLineOperation { get; set; } = CommandLineOperation.None;

        public static NamedPipeServerStream PipeServer { get; set; } = null;

        public static Task<bool> InitEnvironmentTask { get; private set; }

        public static bool SafeShareEnabled => !RegeditOperation.GetIsEnterprise() || (RegeditOperation.GetIsEnterprise() && RegeditOperation.IsSafeShareEnabled());

        public static bool IsCalledByWinZip => IsCalledByWinZipFilePane || IsCalledByWinZipZipPane;

        public static Task<bool> InitWinzipLicenseTask { get; set; }

        #endregion

        #region Public Functions

        public static void Init(string additionalCMDParameters, EventWaitHandle exitEventWaitHandle, ImgUtilView view)
        {
            InitEnvironmentTask = Task.Run(() =>
            {
                bool success = AsposeLicense.Init();
                if (!success)
                {
                    exitEventWaitHandle.Set();
                    return success;
                }

                InitSharedService(additionalCMDParameters);

                return true;
            });
        }

        public static void InitPipeServiceWhenCalledFromWinzip()
        {
            string pipeName = string.Format(Open_ImgUtil_From_WinZip_PIPE_NAME, WinZipProcessID);

            NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough, inBufferSize, outBufferSize);

            pipeServer.BeginWaitForConnection(ar =>
            {
                PipeServer = (NamedPipeServerStream)ar.AsyncState;
                PipeServer.EndWaitForConnection(ar);
            }, pipeServer);

            string eventName = string.Format(Open_ImgUtil_From_WinZip_EVENT_NAME, WinZipProcessID);
            IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);

            if (hEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(hEvent);
            }
        }

        public static void Uninit()
        {
            //RemoveAssociatedCloseProcess
            //A normal exit does not require the Job to execute AssociatedClose
            _jobManagement?.RemoveAssociatedCloseProcess();
            _jobManagement?.Dispose();
            _jobManagement = null;

            if (WinzipSharedServiceHandle != IntPtr.Zero)
            {
                WinzipMethods.DestroySession(WinzipSharedServiceHandle);
                WinzipSharedServiceHandle = IntPtr.Zero;
            }

            if (IsCalledByWinZip)
            {
                PipeServer?.Disconnect();
                PipeServer?.Dispose();
                PipeServer = null;
            }

        }

        public static void SetCommandLineFilePaneItems(List<WzCloudItem4> items)
        {
            _filePaneItems = items;
        }

        public static List<WzCloudItem4> GetCommandLineFilePaneItems()
        {
            return _filePaneItems;
        }

        public static void SetCommandLineSourceFiles(List<string> files)
        {
            _sourceFiles = files;
        }

        public static List<string> GetCommandLineSourceFiles()
        {
            return _sourceFiles;
        }

        public static bool InitWinzipLicense(IntPtr windowHandle)
        {
#if WZ_APPX
            bool ret = true;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ret = WinzipMethods.ShowUWPSubscription(windowHandle);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ret = WinzipMethods.ShowUWPSubscription(windowHandle);
                }));
            }

            if (!ret)
            {
                if (WinzipSharedServiceHandle != IntPtr.Zero)
                {
                    return false;
                }
            }
#else
            // show nag or grace period dialog
            bool ret = true;
            bool isInGracePeriod = WinzipMethods.IsInGracePeriod(windowHandle);
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ret = isInGracePeriod ? WinzipMethods.ShowGracePeriodDialog(windowHandle, false) : WinzipMethods.ShowNag(windowHandle);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ret = isInGracePeriod ? WinzipMethods.ShowGracePeriodDialog(windowHandle, false) : WinzipMethods.ShowNag(windowHandle);
                }));
            }

            if (!ret)
            {
                if (WinzipSharedServiceHandle != IntPtr.Zero)
                {
                    return false;
                }
            }
#endif

            int licenseType = -1;
            WinzipMethods.GetLicenseStatus(windowHandle, ref licenseType);

            if (licenseType != (int)WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION)
            {
                if (!WinzipMethods.CheckLicense(windowHandle))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Private Functions

        private static void InitSharedService(string additionalCMDParameters)
        {
            string accessPermisson = string.Format("TransformAccess({0},{1},{2},{3})",
                    ((int)WzSvcProviderIDs.SPID_CONVERTPHOTOS_TRANSFORM).ToString(), ((int)WzSvcProviderIDs.SPID_ENLARGE_REDUCE_IMAGE).ToString(),
                    ((int)WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM).ToString(), ((int)WzSvcProviderIDs.SPID_REMOVEPERSONALDATA_TRANSFORM).ToString());

            // service id: 2 APPLET_IMG_UTILITY
            createSessionMutex.WaitOne();
            WinzipSharedServiceHandle = WinzipMethods.CreateSession(2, accessPermisson, additionalCMDParameters);
            createSessionMutex.ReleaseMutex();

            if (WinzipSharedServiceHandle != IntPtr.Zero)
            {
                _jobManagement = new JobManagement();
                _jobManagement.AddProcess(WinzipSharedServiceHandle);
            }
        }

        #endregion
    }
}
