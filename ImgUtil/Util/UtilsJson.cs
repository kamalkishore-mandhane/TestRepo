using ImgUtil.WPFUI;
using ImgUtil.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ImgUtil.Util
{
    public partial class UtilsJson
    {
        public static ItemList AnalysisJson(string file)
        {
            if (File.Exists(file))
            {
                var ser = new DataContractJsonSerializer(typeof(ItemList));
                using (var stream = File.OpenRead(file))
                {
                    var byteBuffer = new byte[stream.Length];
                    stream.Read(byteBuffer, 0, byteBuffer.Length);
                    using (var ms = new MemoryStream(byteBuffer))
                    {
                        try
                        {
                            return ser.ReadObject(ms) as ItemList;
                        }
                        catch (Exception)
                        {
                            return new ItemList();
                        }
                    }
                }
            }

            return new ItemList();
        }
    }

    public class WzProfile
    {
        [DataMember]
        public int spid { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string aid { get; set; }
    };

    public class CreatedTime
    {
        [DataMember]
        public string val { get; set; }
    }

    public class ModifiedTime
    {
        [DataMember]
        public string val { get; set; }
    }

    public class WzCloudItem
    {
        [DataMember]
        public WzProfile prof { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string iid { get; set; }

        [DataMember]
        public string desc { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string pid { get; set; }

        [DataMember]
        public string uri { get; set; }

        [DataMember]
        public string rev { get; set; }

        [DataMember]
        public bool fldr { get; set; }

        [DataMember]
        public bool dnld { get; set; }

        [DataMember]
        public long len { get; set; }

        [DataMember]
        public string path { get; set; }

        [DataMember]
        public CreatedTime crt { get; set; }

        [DataMember]
        public ModifiedTime mod { get; set; }

    }

    public class ItemList
    {
        [IgnoreDataMember]
        private List<WzCloudItem> Items = new List<WzCloudItem>();

        [DataMember]
        public WzCloudItem[] cloudItems
        {
            get
            {
                return Items.ToArray();
            }
            set
            {
                Items = new List<WzCloudItem>(value);
            }
        }

        public WzCloudItem4 ConvertToWzCloudItem4(WzCloudItem item)
        {
            WzCloudItem4 item4 = ImageHelper.InitWzCloudItem();

            item4.profile.Id = (WzSvcProviderIDs)item.prof.spid;
            item4.profile.name = item.prof.name;
            item4.profile.authId = item.prof.aid;
            item4.name = item.name;
            item4.itemId = item.iid;
            item4.description = item.desc;
            item4.type = item.type;
            item4.parentId = item.pid;
            item4.uri = item.uri;
            item4.revision = item.rev;
            item4.isFolder = item.fldr;
            item4.isDownloadable = item.dnld;
            item4.length = item.len;
            item4.path = item.path;

            SYSTEMTIME tmpTime;
            var date = DateTime.Now;
            tmpTime.wYear = (ushort)date.Year;
            tmpTime.wMonth = (ushort)date.Month;
            tmpTime.wDay = (ushort)date.Day;
            tmpTime.wDayOfWeek = (ushort)date.DayOfWeek;
            tmpTime.wHour = (ushort)date.Hour;
            tmpTime.wMinute = (ushort)date.Minute;
            tmpTime.wSecond = (ushort)date.Second;
            tmpTime.wMilliseconds = (ushort)date.Millisecond;

            item4.created = tmpTime;
            item4.modified = tmpTime;

            return item4;
        }
    }
}
