using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;

namespace ImgUtil
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
    }

    public static class ApplicationHelper
    {
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

        public static string DefaultLocalUserRecentFilesPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "ImgUtil.RecentFiles");
            }
        }

        public static string DefaultLocalUserImgUtilSettingsPath
        {
            get
            {
                return Path.Combine(LocalUserDataPath, "ImgUtil.AppletSettings");
            }
        }
    }

    public class ImgUtilSettings
    {
        private static ImgUtilSettings _instance;
        private static readonly Mutex ImgUtilSettingsXmlMutex = new Mutex(false, "ImgUtilSettingsXmlMutex");

        public int WindowsState = -1;
        public int ViewZoomInOutSelectedIndex = -1;
        public double WindowPosLeft = -1;
        public double WindowPosTop = -1;
        public double WindowsWidth = 0;
        public double WindowsHeight = 0;
        public string RecordOpenPickerPath;
        public string RecordOpenPickerAuthId;
        public string RecordSavePickerPath;
        public string RecordSavePickerAuthId;

        public static ImgUtilSettings Instance
        {
            get { return _instance ?? (_instance = new ImgUtilSettings()); }
            set { _instance = value; }
        }

        public static void LoadImgUtilSettingsXML()
        {
            try
            {
                ImgUtilSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserImgUtilSettingsPath;
                if (File.Exists(path))
                {
                    var formatter = new XmlSerializer(typeof(ImgUtilSettings));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var memoryStream = new MemoryStream(buffer);
                        _instance = (ImgUtilSettings)formatter.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                ImgUtilSettingsXmlMutex.ReleaseMutex();
            }
        }

        public static void SaveImgUtilSettingsXML()
        {
            try
            {
                ImgUtilSettingsXmlMutex.WaitOne();
                var path = ApplicationHelper.DefaultLocalUserImgUtilSettingsPath;
                var formatter = new XmlSerializer(typeof(ImgUtilSettings));
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
                ImgUtilSettingsXmlMutex.ReleaseMutex();
            }
        }
    }
}
