using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;

namespace SafeShare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static void InitApp()
        {
            if (Application.Current == null)
            {
                new App();
            }
        }

        private void SetHighContrastTheme(bool isHighContrastMode)
        {
            this.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (isHighContrastMode)
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/ColorTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }

        private void UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            SetHighContrastTheme(SystemParameters.HighContrast);
        }
    }

    public static class ApplicationHelper
    {
        public static string ProductName
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                var attrib = AssemblyProductAttribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
                if (attrib != null)
                {
                    return attrib.Product;
                }

                return null;
            }
        }

        public static string LocalUserDataPath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(ApplicationHelper.ProductName))
                {
                    path = Path.Combine(path, ApplicationHelper.ProductName);
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static string DefaultLocalUserSafeShareSettingsPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "SafeShare.AppletSettings");
            }
        }
    }

    public class SafeShareSettings
    {
        private static SafeShareSettings _instance;
        private static readonly Mutex SafeShareSettingsXmlMutex = new Mutex(false, "SafeShareSettingsXmlMutex");

        public int WindowsState = -1;
        public int ShowEvaluationInterval = 3;
        public double WindowPosLeft = -1;
        public double WindowPosTop = -1;
        public double WindowsWidth = 0;
        public double WindowsHeight = 0;
        public string RecordAppRunFirstTime;
        public string RecordOpenPickerPath;
        public string RecordOpenPickerAuthId;
        public string RecordSavePickerPath;
        public string RecordSavePickerAuthId;
        public string RecordEvaluationLastShowTime;

        public static SafeShareSettings Instance
        {
            get { return _instance ?? (_instance = new SafeShareSettings()); }
            set { _instance = value; }
        }

        public SafeShareSettings()
        {
            if (string.IsNullOrEmpty(RecordAppRunFirstTime))
            {
                RecordAppRunFirstTime = DateTime.Now.ToShortDateString();
            }
        }

        public static void LoadSafeShareSettingsXML()
        {
            try
            {
                SafeShareSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserSafeShareSettingsPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(SafeShareSettings));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        _instance = (SafeShareSettings)formatter.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                SafeShareSettingsXmlMutex.ReleaseMutex();
            }
        }

        public static void SaveSafeShareSettingsXML()
        {
            try
            {
                SafeShareSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserSafeShareSettingsPath;
                var formatter = new XmlSerializer(typeof(SafeShareSettings));
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
                SafeShareSettingsXmlMutex.ReleaseMutex();
            }
        }
    }
}