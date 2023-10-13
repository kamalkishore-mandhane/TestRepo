using SafeShare.Util;
using SafeShare.WPFUI.Model;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Xml;
using System.Threading;

namespace SafeShare.WPFUI.Utils
{
    public class DipusConfig
    {
        public string appPath;
        public string parent;
        public string compBasePath;
        public string pkgBasePath;
        public int version;
    }

    public static class SurveyXmlDownloadHelper
    {
        public const string DipusXmlName = "DIPUS.xml";
        public const string DipusText = "DIPUS";
        public const string HttpsFormat = "https://{0}";
        private static int SurveyXmlDownloadInterval = 30;
        public static bool SurveyXmlAvailable = false;
        public const string PuSurveyXmlUrlForUWP = "https://update.winzip.com/static/dipus/app/winzip/76/64/winzip-survey.xml";

        public const string SurveyOperationName = "safeshare";
        public const int SurveyXmlDaysInterval = 7;
        public const int SurveyXmlFreqInterval = 3;

        public static void StartLoadSurveyXmlFromServer()
        {
            if (RegeditOperation.IsRatingSurveyDisabled())
            {
                SurveyXmlAvailable = false;
            }
            else
            {
                var downloadThread = new Thread(new ThreadStart(new Action(delegate
                {
                    if (IsSurveyXmlNeedDownload())
                    {
                        var xmlPath = DownloadSurveyXml();
                        if (File.Exists(xmlPath) && !IsSurveyXmlNeedDownload())
                        {
                            LoadSurveyXmlFreqAndDays(xmlPath);
                            SurveyXmlAvailable = true;
                            return;
                        }
                        SurveyXmlAvailable = false;
                    }
                    else
                    {
                        SurveyXmlAvailable = true;
                    }
                })));

                downloadThread.IsBackground = true;
                downloadThread.Start();
            }
        }

        private static string DownloadSurveyXml()
        {
            var dipusFolder = GetDipusFolder();
            var dipusConfig = LoadDipusXml();

            var surveyXmlUrl = string.Empty;
            var surveyXmlPath = string.Empty;
            var surveyXmlName = string.Empty;

            if (dipusConfig == null)
            {
                surveyXmlName = GetDownloadFileNameFromUrl(PuSurveyXmlUrlForUWP);
                surveyXmlPath = Path.Combine(dipusFolder, surveyXmlName);
                surveyXmlUrl = PuSurveyXmlUrlForUWP;
            }
            else
            {
                if (!string.IsNullOrEmpty(dipusConfig.appPath))
                {
                    var winzipConfigUrl = string.Format(HttpsFormat, dipusConfig.appPath);
                    var winzipConfigName = GetDownloadFileNameFromUrl(winzipConfigUrl);
                    if (!string.IsNullOrEmpty(winzipConfigName))
                    {
                        var winzipConfigPath = Path.Combine(dipusFolder, winzipConfigName);

                        // Download application control file
                        if (!DownloadFile(winzipConfigUrl, winzipConfigPath))
                        {
                            return null;
                        }

                        if (File.Exists(winzipConfigPath))
                        {
                            var surveyPath = GetSurveyUrlPathFromXml(winzipConfigPath);
                            if (!string.IsNullOrEmpty(surveyPath))
                            {
                                surveyXmlUrl = string.Format(HttpsFormat, surveyPath);
                                surveyXmlName = GetDownloadFileNameFromUrl(surveyXmlUrl);
                                if (!string.IsNullOrEmpty(surveyXmlName))
                                {
                                    surveyXmlPath = Path.Combine(dipusFolder, surveyXmlName);
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(surveyXmlPath))
            {
                // Adjust the survey url to the current language
                var langName = RegeditOperation.LangIDToShortName(RegeditOperation.GetWinZipInstalledUILangID()).ToLower();
                var surveyXmlLangName = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(surveyXmlName), langName);
                surveyXmlUrl = surveyXmlUrl.Replace(Path.GetFileNameWithoutExtension(surveyXmlName), surveyXmlLangName);

                // Download survey xml
                if (!DownloadFile(surveyXmlUrl, surveyXmlPath))
                {
                    return null;
                }

                if (File.Exists(surveyXmlPath))
                {
                    return surveyXmlPath;
                }
            }

            return null;
        }

        public static SurveyOperation LoadSurveyXmlFromLocal()
        {
            if (RegeditOperation.IsRatingSurveyDisabled())
            {
                return null;
            }

            var dipusFolder = GetDipusFolder();
            if (Directory.Exists(dipusFolder))
            {
                var dipusConfig = LoadDipusXml();
                if (dipusConfig != null && !string.IsNullOrEmpty(dipusConfig.appPath))
                {
                    var winzipConfigName = GetDownloadFileNameFromUrl(string.Format(HttpsFormat, dipusConfig.appPath));
                    var winzipConfigPath = Path.Combine(dipusFolder, winzipConfigName);

                    if (File.Exists(winzipConfigPath))
                    {
                        var surveyPath = GetSurveyUrlPathFromXml(winzipConfigPath);
                        if (!string.IsNullOrEmpty(surveyPath))
                        {
                            var surveyXmlName = GetDownloadFileNameFromUrl(string.Format(HttpsFormat, surveyPath));
                            var surveyXmlPath = Path.Combine(dipusFolder, surveyXmlName);
                            if (File.Exists(surveyXmlPath))
                            {
                                return LoadSurveyXml(surveyXmlPath);
                            }
                        }
                    }
                }
                else
                {
                    var surveyXmlName = GetDownloadFileNameFromUrl(PuSurveyXmlUrlForUWP);
                    var surveyXmlPath = Path.Combine(dipusFolder, surveyXmlName);
                    if (File.Exists(surveyXmlPath))
                    {
                        return LoadSurveyXml(surveyXmlPath);
                    }
                }
            }

            return null;
        }

        private static bool IsSurveyXmlNeedDownload()
        {
            var surveyXMLPath = TryGetSurveyXmlPath();
            if (!File.Exists(surveyXMLPath))
            {
                return true;
            }

            var dateDownload = File.GetLastWriteTime(surveyXMLPath);
            int daySpan = (DateTime.Now - dateDownload).Days;

            var operation = LoadSurveyXmlFromLocal();
            return operation == null || operation.Items.Count == 0 || daySpan >= SurveyXmlDownloadInterval;
        }

        private static bool DownloadFile(string url, string filePath)
        {
            // force to use the TLS 1.2
            const System.Security.Authentication.SslProtocols tls12 = (System.Security.Authentication.SslProtocols)0x0C00;
            const SecurityProtocolType tls12Type = (SecurityProtocolType)tls12;
            ServicePointManager.SecurityProtocol = tls12Type;

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            // Check if url is reachable
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, filePath);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static SurveyOperation LoadSurveyXml(string surveyXmlPath)
        {
            try
            {
                // Load Update info xml
                var doc = new XmlDocument();
                doc.Load(surveyXmlPath);

                var nodeList = doc.GetElementsByTagName("user-feedback");
                if (nodeList.Count != 1)
                    return null;

                var root = nodeList[0];
                var surveyOperation = new SurveyOperation();

                var expirationAttr = root.Attributes.GetNamedItem("expiration");
                int expirationAttrValue = 0;
                if (expirationAttr != null && int.TryParse(expirationAttr.Value, out expirationAttrValue))
                {
                    SurveyXmlDownloadInterval = expirationAttrValue;
                }

                foreach (XmlNode node in root.ChildNodes)
                {
                    string name = node.Name.ToLower();
                    if (name.ToLower().Equals("operation"))
                    {
                        var opreation = node.Attributes.GetNamedItem("op")?.InnerText;
                        if (!string.IsNullOrEmpty(opreation) && opreation.ToLower().Equals(SurveyOperationName))
                        {
                            foreach (XmlNode optionNode in node.ChildNodes)
                            {
                                string optionNodeName = optionNode.Name.ToLower();
                                if (optionNodeName.Equals("review-header"))
                                {
                                    surveyOperation.ReviewHeader = optionNode.InnerText;
                                }
                                else if (optionNodeName.Equals("survey"))
                                {
                                    foreach (XmlNode surveyNode in optionNode.ChildNodes)
                                    {
                                        string surveyNodeName = surveyNode.Name.ToLower();
                                        if (surveyNodeName.Equals("header"))
                                        {
                                            surveyOperation.SurveyHeader = surveyNode.InnerText;
                                        }
                                        else if (surveyNodeName.Equals("items"))
                                        {
                                            foreach (XmlNode itemNode in surveyNode.ChildNodes)
                                            {
                                                var content = itemNode.InnerText;
                                                var isFreeForm = itemNode.Attributes.GetNamedItem("freeform")?.InnerText == "yes";
                                                var abbreviation = itemNode.Attributes.GetNamedItem("abbreviation")?.InnerText;
                                                if (string.IsNullOrEmpty(abbreviation))
                                                {
                                                    abbreviation = content;
                                                }

                                                surveyOperation.Items.Add(new SurveyChoiceItem(content, abbreviation, isFreeForm));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return surveyOperation;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void LoadSurveyXmlFreqAndDays(string surveyXmlPath)
        {
            // load freq and days for all operations in xml to HKCU
            try
            {
                var doc = new XmlDocument();
                doc.Load(surveyXmlPath);

                var nodeList = doc.GetElementsByTagName("user-feedback");
                if (nodeList.Count != 1)
                    return;

                var root = nodeList[0];
                foreach (XmlNode node in root.ChildNodes)
                {
                    string name = node.Name.ToLower();
                    if (name.ToLower().Equals("operation"))
                    {
                        var opreation = node.Attributes.GetNamedItem("op")?.InnerText;
                        if (!string.IsNullOrEmpty(opreation))
                        {
                            var days = node.Attributes.GetNamedItem("days");
                            int daysAttrValue = 0;
                            if (days != null && int.TryParse(days.Value, out daysAttrValue) && daysAttrValue > 0)
                            {
                                RegeditOperation.SetSurveyDays(daysAttrValue, opreation);
                            }
                            else
                            {
                                RegeditOperation.SetSurveyDays(SurveyXmlDaysInterval, opreation);
                            }

                            var freq = node.Attributes.GetNamedItem("freq");
                            int freqAttrValue = 0;
                            if (freq != null && int.TryParse(freq.Value, out freqAttrValue) && freqAttrValue > 0)
                            {
                                RegeditOperation.SetSurveyFreq(freqAttrValue, opreation);
                            }
                            else
                            {
                                RegeditOperation.SetSurveyFreq(SurveyXmlFreqInterval, opreation);
                            }
                        }
                    }
                }

                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        private static string GetDipusFolder()
        {
            // get local dipus folder, if not exist, create it
            var dipusConfig = LoadDipusXml();
            var dipusFolder = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), dipusConfig == null ? "winzip" : dipusConfig.parent), DipusText);
            if (!Directory.Exists(dipusFolder))
            {
                Directory.CreateDirectory(dipusFolder);
            }

            return dipusFolder;
        }

        private static string GetDownloadFileNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Segments.Last();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static DipusConfig LoadDipusXml()
        {
            // load dipux.xml in install folder
            var dipusPath = Path.Combine(RegeditOperation.GetInstallFolder(), DipusXmlName);
            var config = new DipusConfig();

            try
            {
                // Load Update info xml
                var doc = new XmlDocument();
                doc.Load(dipusPath);

                var nodeList = doc.GetElementsByTagName("dipus");
                if (nodeList.Count != 1) // should be one and only one <dipus> node
                    return null;

                var root = nodeList[0];
                var ver = root.Attributes.GetNamedItem("version")?.InnerText;
                if (!string.IsNullOrEmpty(ver))
                {
                    if (!int.TryParse(ver, out config.version))
                        config.version = 1;
                }

                // Loop through the nodes
                var ienum = root.GetEnumerator();
                while (ienum.MoveNext())
                {
                    var node = (XmlNode)ienum.Current;
                    string name = node.Name.ToLower();

                    switch (name.ToLower())
                    {
                        case "apppath":
                            {
                                if (!string.IsNullOrEmpty(node.InnerText))
                                {
                                    config.appPath = node.InnerText;
                                }
                            }
                            break;
                        case "parent":
                            {
                                if (!string.IsNullOrEmpty(node.InnerText))
                                {
                                    config.parent = node.InnerText;
                                }
                            }
                            break;
                        case "compbasepath":
                            {
                                if (!string.IsNullOrEmpty(node.InnerText))
                                {
                                    config.compBasePath = node.InnerText;
                                }
                            }
                            break;
                        case "pkgbasepath":
                            {
                                if (!string.IsNullOrEmpty(node.InnerText))
                                {
                                    config.pkgBasePath = node.InnerText;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return config;
        }

        private static string GetSurveyUrlPathFromXml(string path)
        {
            // get survey path from winzip config xml
            try
            {
                var doc = new XmlDocument();
                doc.Load(path);

                var nodeList = doc.GetElementsByTagName("dipus_app_ctrl");
                if (nodeList.Count != 1) // should be one and only one <dipus_comp_ctrl> node
                    return null;

                // Loop through the nodes
                var ienum = nodeList[0].GetEnumerator();
                while (ienum.MoveNext())
                {
                    var node = (XmlNode)ienum.Current;
                    string name = node.Name.ToLower();
                    if (name.ToLower().Equals("surveypath"))
                    {
                        return node.InnerText;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string TryGetWinZipConfigPath()
        {
            var dipusFolder = GetDipusFolder();
            var dipusConfig = LoadDipusXml();

            if (dipusConfig != null && !string.IsNullOrEmpty(dipusConfig.appPath))
            {
                var winzipConfigName = GetDownloadFileNameFromUrl(string.Format(HttpsFormat, dipusConfig.appPath));
                return Path.Combine(dipusFolder, winzipConfigName);
            }

            return string.Empty;
        }

        private static string TryGetSurveyXmlPath()
        {
            var winzipConfigPath = TryGetWinZipConfigPath();
            var dipusFolder = GetDipusFolder();
            if (File.Exists(winzipConfigPath))
            {
                var surveyPath = GetSurveyUrlPathFromXml(winzipConfigPath);
                if (!string.IsNullOrEmpty(surveyPath))
                {
                    var surveyXmlName = GetDownloadFileNameFromUrl(string.Format(HttpsFormat, surveyPath));
                    return Path.Combine(dipusFolder, surveyXmlName);
                }
            }
            else
            {
                var surveyXmlName = GetDownloadFileNameFromUrl(PuSurveyXmlUrlForUWP);
                return Path.Combine(dipusFolder, surveyXmlName);
            }

            return string.Empty;
        }
    }
}
