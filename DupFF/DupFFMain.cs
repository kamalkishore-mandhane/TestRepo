using Microsoft.Win32;
using DupFF.WPFUI.Utils;
using DupFF.WPFUI.View;
using DupFF.WPFUI.ViewModel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using static DupFF.WPFUI.Utils.WinZipMethods;
using System.Windows.Threading;

namespace DupFF
{
    class DupFFMain
    {
        private const string WinZipSetupEvent = "Local\\WinZipSetupEvent";
        private const string DupFFProgId = "WinZip.DUPFF";
        private static EventWaitHandle ExitEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static string _cmdPath = string.Empty;
        private static IntPtr _parentHandle = IntPtr.Zero;
        private static string _additionalCMDParameters = string.Empty;
        private static BGTUICategory _viewType = BGTUICategory.Deduplicator;

        private static void WorkThread(object data)
        {
            WinZipMethods.NecessaryInit();
            int langId = Utils.RegeditOperation.GetWinZipInstalledUILangID();
            if (langId != Thread.CurrentThread.CurrentUICulture.LCID)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(langId);
            }

            var args = (string[])data;

            if (!ParseCommand(args))
            {
                return;
            }

            Settings.LoadSettingsXML();

            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            var view = new DupFFView(_parentHandle, _viewType);
            var viewModel = view.DataContext as DupFFViewModel;
            viewModel.SetLoadWinzipSharedService(_parentHandle, _additionalCMDParameters);

            view.Closed += View_Closed;
            viewModel.CalLastWindowPostion(assemblyName);
            view.ShowDialog();

            Settings.SaveSettingsXML();
            WinZipMethods.NecessaryUnInit();
            ExitEventWaitHandle.Set();
        }

        [STAThread]
        public static int Main(string[] args)
        {
            Process CurProc = Process.GetCurrentProcess();
            var processList = Process.GetProcesses().Where(p =>
                p.ProcessName == CurProc.ProcessName).ToList();

            if (processList.Count > 1)
            {
                foreach (var proc in processList)
                {
                    if (proc.Id != CurProc.Id)
                    {
                        NativeMethods.SetForegroundWindow(proc.MainWindowHandle);
                        break;
                    }
                }
                return 0;
            }

            NativeMethods.SetCurrentProcessExplicitAppUserModelID(DupFFProgId);

            if (!Utils.RegeditOperation.IsAppletEnabled())
            {
                FlatMessageBox.ShowWarning(null, Properties.Resources.WARNING_RESTRICTED_FEATURE);
                return 0;
            }

            var watchThread = new Thread(WinZipSetupWatcherRoutine);
            watchThread.IsBackground = true;
            watchThread.Start();

            var threadStart = new ParameterizedThreadStart(WorkThread);
            var workThread = new Thread(threadStart);
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;
            workThread.Start(args);

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            ExitEventWaitHandle.WaitOne();
            SystemEvents.SessionEnding -= SystemEvents_SessionEnding;

            return 0;
        }

        private static bool ParseCommand(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Compare(args[i], "/parentid", true) == 0 && i + 1 < args.Length)
                {
                    i += 1;
                    long handle = 0;
                    if (long.TryParse(args[i], out handle))
                    {
                        _parentHandle = new IntPtr(handle);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (args[i].StartsWith("-cmd", StringComparison.OrdinalIgnoreCase))
                {
                    if (args[i].Length > "-cmd:".Length)
                    {
                        _additionalCMDParameters = args[i].Substring("-cmd:".Length);
                    }

                    i++;
                    while (i < args.Length)
                    {
                        var arg = args[i].Contains(" ") ? "\"" + args[i] + "\"" : args[i];
                        _additionalCMDParameters += " " + arg;
                        i++;
                    }
                }
                else if (args[i].StartsWith("-ViewType:", StringComparison.OrdinalIgnoreCase))
                {
                    var str = args[i].Substring("-ViewType:".Length);
                    if (string.Compare(str, "Organizer", true) == 0)
                    {
                        _viewType = BGTUICategory.Organizer;
                    }
                    else if (string.Compare(str, "Cleaner", true) == 0)
                    {
                        _viewType = BGTUICategory.Cleaner;
                    }
                    else if (string.Compare(str, "Deduplicator", true) == 0)
                    {
                        _viewType = BGTUICategory.Deduplicator;
                    }
                }
            }

            return true;
        }

        // Trigger this event for ending system session
        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            ExitEventWaitHandle.Set();
            e.Cancel = false;
        }

        private static void WinZipSetupWatcherRoutine()
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, WinZipSetupEvent);
            handle.WaitOne();

            if (DupFFView.MainWindow != null && DupFFView.MainWindow.Dispatcher != null)
            {
                DupFFView.MainWindow.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate ()
                {
                    DupFFView.MainWindow.Close();
                }));
            }
        }

        private static void View_Closed(object sender, System.EventArgs e)
        {
            if (_parentHandle != IntPtr.Zero)
            {
                NativeMethods.EnableWindow(_parentHandle, true);
                NativeMethods.SetForegroundWindow(_parentHandle);
            }

            if (WinZipMethods.WinzipSharedServiceHandle != IntPtr.Zero)
            {
                WinZipMethods.DestroyBGToolInfos(WinZipMethods.WinzipSharedServiceHandle);
                WinZipMethods.DestroySession(WinZipMethods.WinzipSharedServiceHandle);
            }
        }
    }

    public class Settings
    {
        private static Settings _instance;
        private static readonly Mutex SettingsXmlMutex = new Mutex(false, "DupFFSettingsXmlMutex");

        public double MainWindowLeft = -1;
        public double MainWindowTop = -1;
        public double MainWindowWidth = 0;
        public double MainWindowHeight = 0;

        public static Settings Instance
        {
            get { return _instance ?? (_instance = new Settings()); }
            set { _instance = value; }
        }

        public static string ProductName
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                if (Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)) is AssemblyProductAttribute attrib)
                {
                    return attrib.Product;
                }

                return null;
            }
        }

        public static string DefaultLocalUserSettingsPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "DupFF.AppletSettings");
            }
        }

        public static string LocalUserDataPath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(ProductName))
                {
                    path = Path.Combine(path, ProductName);
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static void LoadSettingsXML()
        {
            try
            {
                SettingsXmlMutex.WaitOne();
                var path = DefaultLocalUserSettingsPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(Settings));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        _instance = (Settings)formatter.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                SettingsXmlMutex.ReleaseMutex();
            }
        }

        public static void SaveSettingsXML()
        {
            try
            {
                SettingsXmlMutex.WaitOne();
                var path = DefaultLocalUserSettingsPath;
                var formatter = new XmlSerializer(typeof(Settings));
                using (var stream = File.Create(path))
                {
                    formatter.Serialize(stream, _instance);
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                SettingsXmlMutex.ReleaseMutex();
            }
        }
    }
}
