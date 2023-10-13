using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace PdfUtil
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if (SystemParameters.HighContrast)
            {
                this.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
                this.Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/WPFUI/Themes/HighContrastTheme.xaml", UriKind.RelativeOrAbsolute) });
            }
        }

        public static void InitApp()
        {
            if (Application.Current == null)
            {
                new App();
            }
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

        public static string DefaultLocalUserRecentFilesPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "PdfUtil.RecentFiles");
            }
        }

        public static string DefaultLocalUserPdfUtilSettingsPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "PdfUtil.AppletSettings");
            }
        }

        public static string DefaultLocalUserPdfUtilSignature
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "PdfUtilSignature");
            }
        }
    }

    public class PdfUtilSettings
    {
        private static PdfUtilSettings _instance;
        private static readonly Mutex PdfUtilSettingsXmlMutex = new Mutex(false, "PdfUtilSettingsXmlMutex");

        public int WindowsState = -1;
        public int ViewZoomInOutSelectedIndex = -1;
        public double WindowPosLeft = -1;
        public double WindowPosTop = -1;
        public double WindowsWidth = 0;
        public double WindowsHeight = 0;
        public bool IsTabsControlHidden = false;
        public double TabControlsWidth = 0;
        public double TabsControlLastLeftPaneWidth = 0;
        public string RecordOpenPickerPath;
        public string RecordOpenPickerAuthId;
        public string RecordSavePickerPath;
        public string RecordSavePickerAuthId;
        public double ThumbnailZoom = 1;
        public bool DoNotShowSpecifiedFieldsDetectedDialog = false;
        public float SignatureFontSize = 24;
        public string SignatureFontFamily = "Arial";
        public Color SignatureForeground = Colors.Black;
        public System.Drawing.FontStyle SignatureFontStyle = System.Drawing.FontStyle.Regular;

        public static PdfUtilSettings Instance
        {
            get { return _instance ?? (_instance = new PdfUtilSettings()); }
            set { _instance = value; }
        }

        public static void LoadPDFUtilSettingsXML()
        {
            try
            {
                PdfUtilSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserPdfUtilSettingsPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(PdfUtilSettings));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        _instance = (PdfUtilSettings)formatter.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                PdfUtilSettingsXmlMutex.ReleaseMutex();
            }
        }

        public static void SavePDFUtilSettingsXML()
        {
            try
            {
                PdfUtilSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserPdfUtilSettingsPath;
                var formatter = new XmlSerializer(typeof(PdfUtilSettings));
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
                PdfUtilSettingsXmlMutex.ReleaseMutex();
            }
        }
    }
}
