using Microsoft.Win32;
using SafeShare.Util;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace SafeShare
{
    internal class SafeShareApplet
    {
        private const string SafeShareProgId = "WinZip.SafeShare";
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

            var safeShareView = new SafeShareView();
            safeShareView.InitDataContext();

            var watchThread = new Thread(WinZipSetupWatcherRoutine);
            watchThread.IsBackground = true;
            watchThread.Start(safeShareView);

            var args = (string[])data;
            ProcessCommand processCommand = null;
            if (args.Length > 0)
            {
                processCommand = new ProcessCommand(safeShareView);
                processCommand.Process(args);
            }

            SurveyXmlDownloadHelper.StartLoadSurveyXmlFromServer();

            var viewModel = safeShareView.DataContext as SafeShareViewModel;
            viewModel.SetLoadWinzipSharedService();

            if (args.Length > 0)
            {
                processCommand.ProcessCall();
            }
            else
            {
                safeShareView.ShowDialog();
            }

            WinzipMethods.NecessaryUnInit();
            ExitEventWaitHandle.Set();
        }

        [STAThread]
        private static void Main(string[] args)
        {
            // Only allow one application to run.
            var current_prc = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current_prc.ProcessName);
            foreach (var p in processes)
            {
                if (p.Id != current_prc.Id)
                {
                    NativeMethods.SetForegroundWindow(p.MainWindowHandle);
                    return;
                }
            }

            NativeMethods.SetCurrentProcessExplicitAppUserModelID(SafeShareProgId);

            if (!RegeditOperation.IsAppletEnabled())
            {
                SimpleMessageWindows.DisplayWarningConfirmationMessage(Properties.Resources.WARNING_RESTRICTED_FEATURE);
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

        private static void WinZipSetupWatcherRoutine(object view)
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, WinZipSetupEvent);
            handle.WaitOne();

            if (view is SafeShareView safeShareView)
            {
                safeShareView.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                {
                    safeShareView.Close();
                }));
            }
        }
    }
}