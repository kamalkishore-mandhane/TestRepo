using Microsoft.Win32;
using PdfUtil.Util;
using PdfUtil.WPFUI.Utils;
using PdfUtil.WPFUI.View;
using PdfUtil.WPFUI.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PdfUtil
{
    class PdfUtils
    {
        private const string WinZipSetupEvent = "Local\\WinZipSetupEvent";
        private static EventWaitHandle ExitEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static void WorkThread(object data)
        {
            WinzipMethods.NecessaryInit();
            int langId = RegeditOperation.GetWinZipInstalledUILangID();
            if (langId != Thread.CurrentThread.CurrentUICulture.LCID)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(langId);
            }

            var args = (string[])data;
            if (args.Length == 1 && args[0].Equals("/admincfg"))
            {
                ProcessCommand.ChangeIntegration();
                ExitEventWaitHandle.Set();
                return;
            }

            InitLicense().ContinueWith(task =>
            {
                if (!task.Result)
                {
                    ExitEventWaitHandle.Set();
                    return;
                }
            });

            var pdfUtilView = new PdfUtilView();

            pdfUtilView.InitDataContext();

            var viewModel = pdfUtilView.DataContext as PdfUtilViewModel;
            viewModel.SetLoadWinzipSharedService(ProcessCommand.GetAdditionalCMDParameters(in args));

            var watchThread = new Thread(WinZipSetupWatcherRoutine);
            watchThread.IsBackground = true;
            watchThread.Start(pdfUtilView);

            if (args.Length > 0)
            {
                var processCommand = new ProcessCommand(pdfUtilView);
                processCommand.Process(args);
                FileOperation.DeleteGlobalTempDir();

                viewModel.WaitLoadWinzipSharedService();
                if (pdfUtilView.WinzipSharedServiceHandle != IntPtr.Zero)
                {
                    WinzipMethods.DestroySession(pdfUtilView.WinzipSharedServiceHandle);
                    pdfUtilView.WinzipSharedServiceHandle = IntPtr.Zero;
                }
            }
            else
            {
                pdfUtilView.CalLastWindowPostion();
                pdfUtilView.ShowDialog();
            }

            EDPHelper.TimerEventWaitHandle.WaitOne();
            WinzipMethods.NecessaryUnInit();
            ExitEventWaitHandle.Set();
        }

        [STAThread]
        static void Main(string[] args)
        {
            NativeMethods.SetCurrentProcessExplicitAppUserModelID(WPFUI.PdfHelper.PdfProgId);

            if (!RegeditOperation.IsAppletEnabled())
            {
                FlatMessageWindows.DisplayWarningMessage(IntPtr.Zero, Properties.Resources.WARNING_RESTRICTED_FEATURE);
                return;
            }

            var threadStart = new ParameterizedThreadStart(WorkThread);
            var workThread = new Thread(threadStart);
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;
            workThread.Start(args);

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            ExitEventWaitHandle.WaitOne();
            SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
        }

        private static Task<bool> InitLicense()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                return License.Init();
            });
        }

        // Trigger this event for ending system session
        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            ExitEventWaitHandle.Set();
            e.Cancel = false;
        }

        // await signal for WinZip uninstall workflow
        private static void WinZipSetupWatcherRoutine(object view)
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, WinZipSetupEvent);
            handle.WaitOne();

            if (view is PdfUtilView pdfUtilView)
            {
                pdfUtilView.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                {
                    pdfUtilView.Close();
                }));
            }
        }
    }
}
