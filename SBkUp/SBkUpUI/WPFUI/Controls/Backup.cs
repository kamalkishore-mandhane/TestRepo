using SBkUpUI.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SBkUpUI.WPFUI.Controls
{
    public class Backup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private Utils.WinZipMethods.WzCloudItem4 _item;

        public static readonly string Timestamp = "yyyy-MM-dd-HH-mm-ss";
        public static readonly string BackupExtension = ".zipx";

        public Backup(Utils.WinZipMethods.WzCloudItem4 item)
        {
            _item = item;
            var tempName = string.Empty;
            try
            {
                if (Utils.WinZipMethods.IsCloudItem(item.profile.Id))
                {
                    tempName = item.name.Substring(item.name.Length - BackupExtension.Length - Timestamp.Length, Timestamp.Length);
                }
                else
                {
                    tempName = item.path.Substring(item.path.Length -BackupExtension.Length - Timestamp.Length, Timestamp.Length);
                }

                var myDate = DateTime.ParseExact(tempName, Timestamp, System.Globalization.CultureInfo.InvariantCulture);
                _name = myDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {
                _name = WinZipMethods.SYSTEMTIMEToDateTime(_item.modified).ToString("yyyy-MM-dd HH:mm:ss"); ;
            }
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                Notify(nameof(Name));
            }
        }

        public Utils.WinZipMethods.WzCloudItem4 Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                Notify(nameof(Item));
            }
        }
    }
}
