using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupFF.WPFUI.Controls
{
    class ActionItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _guid;
        private string _name;
        private int _fileNumber;
        private bool _processed;
        private int _processedFileNumber;
        private bool _selected;

        private string _lastRun;
        private string _result;

        public static List<ActionItem> ParseActionItem(string src)
        {
            var data = src.Split('\t');
            var list = new List<ActionItem>();
            if (data.Length == 0 || data.Length % 5 != 0)
            {
                return list;
            }
            try
            {
                for (int i = 0; i < data.Length;)
                {
                    var item = new ActionItem();
                    item._guid = data[i++];
                    if (!item._guid.StartsWith("{"))
                    {
                        item._guid = "{" + item._guid + "}";
                    }
                    item._name = string.Empty;
                    item._fileNumber = Int32.Parse(data[i++]);
                    item._processed = Int32.Parse(data[i++]) == 1;
                    item._processedFileNumber = Int32.Parse(data[i++]);
                    item._lastRun = data[i++];
                    if (item._processed)
                    {
                        item._result = string.Empty;
                    }
                    else
                    {
                        item._result = string.Format(Properties.Resources.RESULT_ACTION_NEEDED, item._fileNumber);
                    }
                    list.Add(item);
                }
            }
            catch (Exception)
            {
                ;
            }
            return list;
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                }
            }
        }

        public string LastRun
        {
            get
            {
                return _lastRun;
            }
            set
            {
                if (_lastRun != value)
                {
                    _lastRun = value;
                    Notify(nameof(LastRun));
                }
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    Notify(nameof(Result));
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

        public bool Processed
        {
            get
            {
                return _processed;
            }
            set
            {
                if (_processed != value)
                {
                    _processed = value;
                }
            }
        }

        public static void CloneItem(ActionItem dest, ActionItem src)
        {
            dest.Name = src.Name;
            dest._fileNumber = src._fileNumber;
            dest._processed = src._processed;
            dest._processedFileNumber = src._processedFileNumber;
            dest.LastRun = src.LastRun;
            dest.Result = src.Result;
        }
    }
}
