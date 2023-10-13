using ImgUtil.Util;
using ImgUtil.WPFUI.Utils;
using ImgUtil.WPFUI.View;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ImgUtil
{
    class ImgUtils
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

            string[] args = (string[])data;
            if (args.Length == 1 && args[0] == "/admincfg")
            {
                ProcessCommand.ChangeIntegration();
                ExitEventWaitHandle.Set();
                return;
            }

            var imgUtilView = new ImgUtilView();
            imgUtilView.InitDataContext();

            EnvironmentService.Init(ProcessCommand.GetAdditionalCMDParameters(in args), ExitEventWaitHandle, imgUtilView);

            var watchThread = new Thread(WinZipSetupWatcherRoutine);
            watchThread.IsBackground = true;
            watchThread.Start(imgUtilView);

            if (args.Length > 0)
            {
                ProcessCommand processCommand = new ProcessCommand(imgUtilView);
                processCommand.Process(args);
            }
            else
            {
                imgUtilView.IsMultipleWindow = false;
                imgUtilView.CalLastWindowPostion();
                imgUtilView.ShowDialog();
            }

            EnvironmentService.Uninit();
            WinzipMethods.NecessaryUnInit();
            ExitEventWaitHandle.Set();
        }

        [STAThread]
        static void Main(string[] args)
        {
            NativeMethods.SetCurrentProcessExplicitAppUserModelID(WPFUI.ImageHelper.ImageProgId);

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

            if (view is ImgUtilView imgUtilView)
            {
                imgUtilView.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                {
                    imgUtilView.Close();
                }));
            }
        }
    }
}
