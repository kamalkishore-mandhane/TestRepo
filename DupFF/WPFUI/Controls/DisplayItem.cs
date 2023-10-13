using DupFF.WPFUI.Utils;
using DupFF.WPFUI.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DupFF.WPFUI.Controls
{
    // BGTool
    enum ItemStatus { Unknown = 0, Loaded, Normal, Delete, ExtSched, Busy };

    enum ItemStatusUI { Unknown = 0, ReadyToRun, Complete, Running, Deleted, ExtSched };

    enum WzBGToolIDs
    {
        BGTID_UNKNOWN = 0,
        BGTID_DOWNLOADS,
        BGTID_DOCUMENTS,
        BGTID_PICTURES,
        BGTID_TEMPORARY,
        BGTID_RECYCLEBIN,
        BGTID_UNPROTECTEDCLOUD,
        BGTID_CUSTOM,
        BGTID_CUSTOMCLOUD,
        BGTID_PINNEDFOLDERS,
        BGTID_ZIPSHAREFOLDER,
        BGTID_DEDUPER,
        BGTID_ORGANIZER,
        BGTID_CANNED_DEDUPER
    };

    enum BGToolProps
    {
        None                            = 0x00000000,
        RequiresPro                     = 0x00000001,
        SupportsDateFilter              = 0x00000002,
        SupportsFileNameFilter          = 0x00000004,
        SupportsExcludedFolders         = 0x00000008,
        UserModifiableDateFilter        = 0x00000010,
        UserModifiableFileNameFilter    = 0x00000020,
        UserModifiableToolName          = 0x00000040,
        UserModifiableDescription       = 0x00000080,
        UserModifiableSourceFolders     = 0x00000100,
        UserModifiableDestFolders       = 0x00000200,
        UserModifiableExcludedFolders   = 0x00000400,
        SupportsMultipleSourceFolders   = 0x00000800,
        SupportsLookInsideZip           = 0x00001000,
        SupportsAppendToDestFolder      = 0x00002000,
        SupportsAutomate                = 0x00004000,

        LastProp = SupportsAutomate, // ! update as needed

        // All-flags value for XML version 2. Create new values for new versions.
        AllFlagsV2 = RequiresPro | SupportsDateFilter | SupportsFileNameFilter | SupportsExcludedFolders
        | UserModifiableDateFilter | UserModifiableFileNameFilter | UserModifiableToolName
        | UserModifiableDescription | UserModifiableSourceFolders | UserModifiableDestFolders
        | UserModifiableExcludedFolders | SupportsMultipleSourceFolders
        | SupportsLookInsideZip | SupportsAppendToDestFolder | SupportsAutomate,

        // All-flags value for current version
        AllFlagsCurrent = AllFlagsV2
    };

    class DisplayItem : INotifyPropertyChanged
    {
        public static readonly string DownloadsGuid = "{86028757-4F6C-4F2A-8836-9311464E3B36}";
        public static readonly string DocumentsGuid = "{58152587-FDC3-46BF-90FC-B9D3143F7CF2}";
        public static readonly string PicturesGuid = "{8E12013C-A1EA-4D14-BD80-618D43334D5C}";

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RunComplete;

        private string _guid;
        private string _name;
        private string _description;
        private int _index;
        private bool _isEnabled;
        private int _frequency;
        private bool _selected;
        private ItemStatus _itemStatus;
        private DateTime _time;
        private DateTime _date;
        private WzBGToolIDs _toolType;
        private BGToolProps _toolProps;
        private ActionItem _actionItem;

        public static List<DisplayItem> ParseDisplayItem(string src)
        {
            var data = src.Split('\t');
            var list = new List<DisplayItem>();
            if (data.Length == 0 || data.Length % 10 != 0)
            {
                return list;
            }
            try
            {
                for (int i = 0; i < data.Length;)
                {
                    var item = new DisplayItem();
                    item._guid = data[i++];
                    item._name = data[i++];
                    item._description = data[i++];
                    item._index = Int32.Parse(data[i++]);
                    item._isEnabled = Int32.Parse(data[i++]) == 1;
                    item._frequency = Int32.Parse(data[i++]);
                    var baseTime = new DateTime(1970, 1, 1);
                    var dateTime = baseTime.AddSeconds(long.Parse(data[i++]));
                    item._time = dateTime;
                    item._date = dateTime;
                    item._itemStatus = (ItemStatus)Int32.Parse(data[i++]);
                    item._toolType = (WzBGToolIDs)Int32.Parse(data[i++]);
                    item._toolProps = (BGToolProps)Int32.Parse(data[i++]);
                    list.Add(item);
                }
            }
            catch(Exception)
            {
                ;
            }
            return list;
        }

        public string ConvertDisplayItem()
        {
            var delimiter = '\t';
            var res = string.Empty;
            res += _name;
            res += delimiter;
            res += _index;
            res += delimiter;
            res += _isEnabled ? "1" : "0";
            res += delimiter;
            res += _frequency;
            res += delimiter;
            var date = new DateTime(_date.Year, _date.Month, _date.Day, _time.Hour, _time.Minute, 0);
            res += date.ToString("yyyy-MM-dd-HH-mm-ss");
            res += delimiter;
            res += (int)_itemStatus;
            res += delimiter;
            return res;
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DisplayItem()
        {
            PropertyChanged += DisplayItem_PropertyChanged;
        }

        public bool StopSyncBGToolInfos { get; set; } = false;

        private void DisplayItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (StopSyncBGToolInfos)
            {
                return;
            }

            if (e.PropertyName == nameof(Frequency) || e.PropertyName == nameof(IsEnabled)
                || e.PropertyName == nameof(Date) || e.PropertyName == nameof(Time))
            {
                WinZipMethods.SetBGToolInfos(DupFFView.MainWindow.WindowHandle, ConvertDisplayItem());

                if (e.PropertyName == nameof(IsEnabled))
                {
                    TrackHelper.LogFinderEnabledEvent(GetDupFinderTrackName(), IsEnabled);
                }
            }
        }

        private string GetDupFinderTrackName()
        {
            if (_toolType == WzBGToolIDs.BGTID_CANNED_DEDUPER)
            {
                if (Guid.ToUpper().Equals(PicturesGuid))
                    return "pics";
                if (Guid.ToUpper().Equals(DownloadsGuid))
                    return "downloads";
                if (Guid.ToUpper().Equals(DocumentsGuid))
                    return "docs";
            }

            return string.Empty;
        }

        public string Guid
        {
            get
            {
                return _guid;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Notify(nameof(Name));
                    Notify(nameof(ToolTip));
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    Notify(nameof(Description));
                    Notify(nameof(ToolTip));
                }
            }
        }

        public string ToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return Name;
                }
                else
                {
                    return Name + "\n" + Description;
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    Notify(nameof(IsEnabled));
                }
            }
        }

        public int Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    Notify(nameof(Frequency));
                }
            }
        }

        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    Notify(nameof(Time));
                }
            }
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    Notify(nameof(Date));
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Notify(nameof(IsSelected));
                }
            }
        }

        public bool Available
        {
            get
            {
                return _itemStatus == ItemStatus.Normal || _itemStatus == ItemStatus.Loaded;
            }
        }

        public ItemStatusUI ItemStatusUI
        {
            get
            {
                switch (ItemStatus)
                {
                    case ItemStatus.Busy:
                        return ItemStatusUI.Running;
                    case ItemStatus.Delete:
                        return ItemStatusUI.Deleted;
                    case ItemStatus.ExtSched:
                        return ItemStatusUI.ExtSched;
                    case ItemStatus.Loaded:
                    case ItemStatus.Normal:
                        {
                            if (ActionItem == null)
                            {
                                return ItemStatusUI.ReadyToRun;
                            }
                            else
                            {
                                return ItemStatusUI.Complete;
                            }
                        }
                    default:
                        return ItemStatusUI.Unknown;
                }
            }
        }

        public ItemStatus ItemStatus
        {
            get
            {
                return _itemStatus;
            }
            set
            {
                if (_itemStatus != value)
                {
                    var oldValue = _itemStatus;
                    _itemStatus = value;
                    Notify(nameof(Available));
                    Notify(nameof(ItemStatusUI));
                    Notify(nameof(ItemStatus));
                    if (oldValue == ItemStatus.Busy && RunComplete != null)
                    {
                        RunComplete(this, new EventArgs());
                    }
                }
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index != value)
                {
                    _index = value;
                }
            }
        }

        public bool IsCanned
        { 
            get
            {
                return _toolType != WzBGToolIDs.BGTID_CUSTOM && _toolType != WzBGToolIDs.BGTID_CUSTOMCLOUD
                    && _toolType != WzBGToolIDs.BGTID_ORGANIZER && _toolType != WzBGToolIDs.BGTID_DEDUPER;
            }
        }

        public bool SupportFilter
        {
            get
            {
                return (_toolProps & BGToolProps.UserModifiableFileNameFilter) != 0;
            }
        }

        public ActionItem ActionItem
        {
            get
            {
                return _actionItem;
            }
            set
            {
                _actionItem = value;
                Notify(nameof(ItemStatusUI));
            }
        }

        public static void CloneItem(DisplayItem dest, DisplayItem src)
        {
            dest.Date = src.Date;
            dest.Frequency = src.Frequency;
            dest.IsEnabled = src.IsEnabled;
            dest.ItemStatus = src.ItemStatus;
            dest.Name = src.Name;
            dest.Description = src.Description;
            dest.Time = src.Time;
        }
    }
}
