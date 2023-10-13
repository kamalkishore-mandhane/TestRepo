using Microsoft.Win32;
using SBkUpUI;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;

namespace SBkUp
{
    class SBkUpMain
    {
        private const string WinZipSetupEvent = "Local\\WinZipSetupEvent";
        private const string SBkUpProgId = "WinZip.SecureBackup";
        private static EventWaitHandle ExitEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private static int appResult = 0;

        private static void WorkThread(object data)
        {
            int langId = RegeditOperation.GetWinZipInstalledUILangID();
            if (langId != Thread.CurrentThread.CurrentUICulture.LCID)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(langId);
            }

            var args = (string[])data;
            SBkUpSettings.LoadImgUtilSettingsXML();

            WzSBkUpUI.settings.MainWindowTop = SBkUpSettings.Instance.MainWindowTop;
            WzSBkUpUI.settings.MainWindowLeft = SBkUpSettings.Instance.MainWindowLeft;
            WzSBkUpUI.settings.MainWindowWidth = SBkUpSettings.Instance.MainWindowWidth;
            WzSBkUpUI.settings.MainWindowHeight = SBkUpSettings.Instance.MainWindowHeight;

            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            appResult = WzSBkUpUI.DllMain(args, assemblyName);

            SBkUpSettings.Instance.MainWindowTop = WzSBkUpUI.settings.MainWindowTop;
            SBkUpSettings.Instance.MainWindowLeft = WzSBkUpUI.settings.MainWindowLeft;
            SBkUpSettings.Instance.MainWindowWidth = WzSBkUpUI.settings.MainWindowWidth;
            SBkUpSettings.Instance.MainWindowHeight = WzSBkUpUI.settings.MainWindowHeight;
            SBkUpSettings.SaveImgUtilSettingsXML();

            ExitEventWaitHandle.Set();
        }

        [STAThread]
        public static int Main(string[] args)
        {
            SBkUpUI.WPFUI.Utils.NativeMethods.SetCurrentProcessExplicitAppUserModelID(SBkUpProgId);

            if (!RegeditOperation.IsAppletEnabled())
            {
                WzSBkUpUI.WarningFeatureDisabled();
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

            return appResult;
        }

        // Trigger this event for ending system session
        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            ExitEventWaitHandle.Set();
            e.Cancel = false;
        }

        // await signal for WinZip uninstall workflow
        private static void WinZipSetupWatcherRoutine()
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, WinZipSetupEvent);
            handle.WaitOne();
            ExitEventWaitHandle.Set();
        }
    }

    public class SBkUpSettings
    {
        private static SBkUpSettings _instance;
        private static readonly Mutex SBkUpSettingsXmlMutex = new Mutex(false, "SBkUpSettingsXmlMutex");

        public double MainWindowLeft = -1;
        public double MainWindowTop = -1;
        public double MainWindowWidth = 0;
        public double MainWindowHeight = 0;

        public static SBkUpSettings Instance
        {
            get { return _instance ?? (_instance = new SBkUpSettings()); }
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

        public static string DefaultLocalUserSBkUpSettingsPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "SBkUp.AppletSettings");
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

        public static void LoadImgUtilSettingsXML()
        {
            try
            {
                SBkUpSettingsXmlMutex.WaitOne();
                var path = DefaultLocalUserSBkUpSettingsPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(SBkUpSettings));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        _instance = (SBkUpSettings)formatter.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                SBkUpSettingsXmlMutex.ReleaseMutex();
            }
        }

        public static void SaveImgUtilSettingsXML()
        {
            try
            {
                SBkUpSettingsXmlMutex.WaitOne();
                var path = DefaultLocalUserSBkUpSettingsPath;
                var formatter = new XmlSerializer(typeof(SBkUpSettings));
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
                SBkUpSettingsXmlMutex.ReleaseMutex();
            }
        }
    }
}
