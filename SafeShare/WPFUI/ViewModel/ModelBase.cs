using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace SafeShare.WPFUI.ViewModel
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UIObjects : ObservableCollection<UIObject>
    {
        public T[] ToArray<T>() where T : UIObject
        {
            List<T> ret = new List<T>();
            foreach (var item in this)
            {
                if (item is T)
                {
                    ret.Add(item as T);
                }
            }
            return ret.ToArray();
        }
    }

    public abstract class UIObject : ObservableObject
    {
        public virtual ImageSource IconImage
        {
            get; protected set;
        }

        public virtual string Name
        {
            get; set;
        }
    }
}