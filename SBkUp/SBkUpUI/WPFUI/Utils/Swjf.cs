using SBkUpUI.WPFUI.Controls;
using SBkUpUI.WPFUI.View;
using SBkUpUI.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace SBkUpUI.WPFUI.Utils
{
    public enum CannedSwjf
    {
        Desktop = 0,
        Documents,
        Favorites,
        Pictures,
        End
    }

    public class Swjf
    {
        public WinZipMethods.WzCloudItem4 backupFolder = WinZipMethods.InitWzCloudItem();
        public WinZipMethods.WzCloudItem4 storeFolder = WinZipMethods.InitWzCloudItem();
        public bool encrypt = false;
        public WinZipMethods.Encryption encryption = WinZipMethods.Encryption.CRYPT_NONE;
        public string password = string.Empty;
        public bool limitMaxBackupNumber = false;
        public int maxBackupNumber = 7;
        public string zipName;
        public string excludeFilter;
        public bool isCanned = false;

        public static string Extension
        {
            get
            {
                return ".swjf";
            }
        }

        public Swjf CloneSwjf()
        {
            var obj = new Swjf();
            obj.backupFolder = backupFolder;
            obj.storeFolder = storeFolder;
            obj.encrypt = encrypt;
            obj.encryption = encryption;
            obj.password = password;
            obj.limitMaxBackupNumber = limitMaxBackupNumber;
            obj.maxBackupNumber = maxBackupNumber;
            obj.zipName = zipName;
            obj.excludeFilter = excludeFilter;
            obj.isCanned = isCanned;
            return obj;
        }

        public bool Save(string path)
        {
            string enterpriseId = string.Empty;
            if (EDPAPIHelper.IsProcessProtectedByEDP())
            {
                if (File.Exists(path))
                {
                    enterpriseId = EDPAPIHelper.GetEnterpriseId(path);
                }
            }

            var result = SaveInternal(path);

            if (EDPAPIHelper.IsProcessProtectedByEDP())
            {
                if (enterpriseId == string.Empty)
                {
                    EDPAPIHelper.UnProtectItem(path);
                }
                else
                {
                    using (var enter = new EDPAutoRestoreTempEnterpriseID(enterpriseId))
                    {
                        EDPAPIHelper.ProtectNewItem(path);
                    }
                }
            }

            return result;
        }

        private bool SaveInternal(string path)
        {
            try
            {
                if (!Directory.Exists(SBkUpViewModel.JobFolder))
                {
                    Directory.CreateDirectory(SBkUpViewModel.JobFolder);
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
                FlatMessageBox.ShowWarning(SBkUpView.MainWindow, string.Format(Properties.Resources.WARNING_ACCESS_DENY, Path.GetFileName(path)));
                return false;
            }

            using (var writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("winzipjob");
                writer.WriteAttributeString("version", "10");
                writer.WriteAttributeString("minwinrun", "61");
                writer.WriteAttributeString("type", isCanned ? "1" : "2");
                writer.WriteWhitespace(Environment.NewLine);

                // item
                if (WinZipMethods.IsCloudItem(backupFolder.profile.Id))
                {
                    writer.WriteStartElement("items");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("type", "cloud");
                    writer.WriteAttributeString("spid", ((int)backupFolder.profile.Id).ToString());
                    writer.WriteAttributeString("auth-id", backupFolder.profile.authId);
                    writer.WriteAttributeString("nickname", backupFolder.profile.name);
                    writer.WriteAttributeString("parent-id", backupFolder.parentId);
                    writer.WriteAttributeString("folder", "yes");
                    writer.WriteAttributeString("dnld", backupFolder.isDownloadable ? "yes" : "no");
                    writer.WriteAttributeString("item-id", backupFolder.itemId);
                    writer.WriteAttributeString("guid", "no");//??
                    writer.WriteAttributeString("exclude", "no");
                    writer.WriteAttributeString("recurse", "yes");
                    writer.WriteString(backupFolder.path);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                }
                else
                {
                    writer.WriteStartElement("items");
                    writer.WriteWhitespace(Environment.NewLine);
                    if (!string.IsNullOrEmpty(excludeFilter))
                    {
                        writer.WriteStartElement("filter");
                        writer.WriteAttributeString("exclude", "yes");
                        writer.WriteString(excludeFilter);
                        writer.WriteEndElement();
                        writer.WriteWhitespace(Environment.NewLine);
                    }

                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("spid", ((int)(backupFolder.profile.Id)).ToString());
                    writer.WriteAttributeString("exclude", "no");
                    writer.WriteAttributeString("recurse", "yes");
                    writer.WriteAttributeString("folder", "yes");
                    writer.WriteAttributeString("dnld", backupFolder.isDownloadable ? "yes" : "no");
                    writer.WriteString(Util.GetFolderPathFromID(backupFolder.itemId));
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                }

                // output
                if (WinZipMethods.IsCloudItem(storeFolder.profile.Id))
                {
                    writer.WriteStartElement("output");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("spid");
                    writer.WriteString(((int)storeFolder.profile.Id).ToString());
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("auth-id");
                    writer.WriteString(storeFolder.profile.authId);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("nickname");
                    writer.WriteString(storeFolder.profile.name);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("item-id");
                    writer.WriteString(storeFolder.itemId);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("name");
                    writer.WriteAttributeString("append", limitMaxBackupNumber ? "datetime" : "none");
                    writer.WriteString(zipName);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("folder");
                    writer.WriteAttributeString("append", "none");
                    writer.WriteString(Util.GetRestoreFolderStorageText(in storeFolder));
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                }
                else
                {
                    writer.WriteStartElement("output");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("spid");
                    writer.WriteString(storeFolder.profile.authId);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("auth-id");
                    writer.WriteString(storeFolder.profile.authId);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("name");
                    writer.WriteAttributeString("append", limitMaxBackupNumber ? "datetime" : "none");
                    writer.WriteString(zipName);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("folder");
                    writer.WriteAttributeString("append", "none");
                    writer.WriteString(storeFolder.itemId);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                }

                writer.WriteStartElement("options");
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("compression");
                writer.WriteAttributeString("type", "best");
                writer.WriteEndElement();
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("folders");
                writer.WriteAttributeString("mode", "relative");
                writer.WriteEndElement();
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("progress");
                writer.WriteString("yes");
                writer.WriteEndElement();
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("encryption");
                writer.WriteAttributeString("mode", encryption == WinZipMethods.Encryption.CRYPT_NONE ? "none" : "store");
                writer.WriteAttributeString("method", WinZipMethods.EncryptionToString(encryption));
                writer.WriteEndElement();
                writer.WriteWhitespace(Environment.NewLine);
                if (encryption != WinZipMethods.Encryption.CRYPT_NONE)
                {
                    writer.WriteStartElement("xflags");
                    string protectData = password;
                    WinZipMethods.ProtectData(IntPtr.Zero, ref protectData);
                    writer.WriteString(protectData);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                }
                if (limitMaxBackupNumber)
                {
                    writer.WriteStartElement("retain");
                    writer.WriteString(maxBackupNumber.ToString());
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                }
                writer.WriteEndElement();
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
            
            return true;
        }

        public bool Load(string path)
        {
            var doc = new XmlDocument();
            try
            {
                DateTime createTime = File.GetCreationTime(path);
                DateTime modifiedTime = File.GetLastWriteTime(path);
                this.storeFolder.created = new WinZipMethods.SYSTEMTIME()
                {
                    wYear = (ushort)createTime.Year,
                    wMonth = (ushort)createTime.Month,
                    wDay = (ushort)createTime.Day,
                    wDayOfWeek = (ushort)createTime.DayOfWeek,
                    wHour = (ushort)createTime.Hour,
                    wMinute = (ushort)createTime.Minute,
                    wSecond = (ushort)createTime.Second,
                    wMilliseconds = (ushort)createTime.Millisecond
                };

                this.storeFolder.modified = new WinZipMethods.SYSTEMTIME()
                {
                    wYear = (ushort)modifiedTime.Year,
                    wMonth = (ushort)modifiedTime.Month,
                    wDay = (ushort)modifiedTime.Day,
                    wDayOfWeek = (ushort)modifiedTime.DayOfWeek,
                    wHour = (ushort)modifiedTime.Hour,
                    wMinute = (ushort)modifiedTime.Minute,
                    wSecond = (ushort)modifiedTime.Second,
                    wMilliseconds = (ushort)modifiedTime.Millisecond
                };

                doc.Load(path);

                XmlNode node;
                XmlNodeList nodeList;

                nodeList = doc.GetElementsByTagName("winzipjob");
                if (nodeList.Count != 1)
                {
                    return false;
                }
                else
                {
                    bool hasType = false;
                    foreach (var obj in nodeList[0].Attributes)
                    {
                        if (obj is XmlAttribute attr)
                        {
                            if (attr.Name.ToLower() == "type")
                            {
                                isCanned = attr.Value == "1";
                                hasType = true;
                            }
                        }
                    }
                    if (!hasType)
                    {
                        return false;
                    }
                }

                nodeList = doc.GetElementsByTagName("items");
                if (nodeList.Count != 1)
                {
                    return false;
                }

                XmlNode root = nodeList[0];

                var ienum = root.GetEnumerator();
                while (ienum.MoveNext())
                {
                    node = (XmlNode)ienum.Current;
                    string name = node.Name.ToLower();
                    if (name == "item")
                    {
                        GetBackupFolder(node);
                    }
                    else if (name == "filter")
                    {
                        excludeFilter = node.InnerText;
                    }
                    else
                    {
                        return false;
                    }
                }

                nodeList = doc.GetElementsByTagName("output");
                if (nodeList.Count != 1)
                {
                    return false;
                }

                GetStoreFolder(nodeList[0]);

                nodeList = doc.GetElementsByTagName("options");
                if (nodeList.Count != 1)
                {
                    return false;
                }

                GetOptions(nodeList[0]);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool GetBackupFolder(XmlNode node)
        {
            backupFolder.path = node.InnerText;
            backupFolder.itemId = Util.GetIDFromFolderPath(backupFolder.path);
            backupFolder.isFolder = true;
            foreach (var obj in node.Attributes)
            {
                if (obj is XmlAttribute attr)
                {
                    if (attr.Name.ToLower() == "spid")
                    {
                        backupFolder.profile.Id = (WinZipMethods.WzSvcProviderIDs)Int32.Parse(attr.Value);
                    }
                    else if (attr.Name.ToLower() == "auth-id")
                    {
                        backupFolder.profile.authId = attr.Value;
                    }
                    else if (attr.Name.ToLower() == "nickname")
                    {
                        backupFolder.profile.name = attr.Value;
                    }
                    else if (attr.Name.ToLower() == "parent-id")
                    {
                        backupFolder.parentId = attr.Value;
                    }
                    else if (attr.Name.ToLower() == "item-id")
                    {
                        backupFolder.itemId = attr.Value;
                    }
                }
            }
            return true;
        }

        private bool GetStoreFolder(XmlNode root)
        {
            storeFolder.isFolder = true;
            var ienum = root.GetEnumerator();
            while (ienum.MoveNext())
            {
                XmlNode node = (XmlNode)ienum.Current;
                string name = node.Name.ToLower();
                if (name == "spid")
                {
                    storeFolder.profile.Id = (WinZipMethods.WzSvcProviderIDs)(Int32.Parse(node.InnerText));
                }
                else if (name == "auth-id")
                {
                    storeFolder.profile.authId = node.InnerText;
                }
                else if (name == "nickname")
                {
                    storeFolder.profile.name = node.InnerText;
                }
                else if (name == "item-id")
                {
                    storeFolder.itemId = node.InnerText;
                }
                else if (name == "name")
                {
                    zipName = node.InnerText;
                }
                else if (name == "folder")
                {
                    storeFolder.path = node.InnerText;
                }
            }
            if (WinZipMethods.IsCloudItem(storeFolder.profile.Id))
            {
                LoadAfterWinZipLoaded(() => { Util.GetRestoreFolderRecordText(ref storeFolder); });
            }
            else
            {
                storeFolder.itemId = storeFolder.path;
            }

            return true;
        }

        private bool GetOptions(XmlNode root)
        {
            var ienum = root.GetEnumerator();
            while (ienum.MoveNext())
            {
                XmlNode node = (XmlNode)ienum.Current;
                string name = node.Name.ToLower();
                if (name == "encryption")
                {
                    foreach (var obj in node.Attributes)
                    {
                        if (obj is XmlAttribute attr)
                        {
                            if (attr.Name.ToLower() == "method")
                            {
                                encryption = WinZipMethods.StringToEncryption(attr.Value);
                                encrypt = (encryption != WinZipMethods.Encryption.CRYPT_NONE);
                            }
                        }
                    }
                }
                else if (name == "xflags")
                {
                    string protectData = node.InnerText;
                    LoadAfterWinZipLoaded(() =>
                    {
                        WinZipMethods.UnprotectData(IntPtr.Zero, ref protectData);
                        password = protectData;
                    });
                }
                else if (name == "retain")
                {
                    var backups = Int32.Parse(node.InnerText);
                    if (backups >= NumericUpDown.minvalue && backups <= NumericUpDown.maxvalue)
                    {
                        limitMaxBackupNumber = true;
                        maxBackupNumber = backups;
                    }
                }
            }
            return true;
        }

        private void LoadAfterWinZipLoaded(Action action)
        {
            if (SBkUpViewModel.SBkUpViewModelInstance.IsWinZipLoaded)
            {
                action();
            }
            else
            {
                SBkUpViewModel.SBkUpViewModelInstance.WinZipLoaded += (sender, e) => { action(); };
            }
        }
    }
}
