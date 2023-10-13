using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SafeShare.WPFUI.Model.Services
{
    public class ServiceBase : INotifyPropertyChanged
    {
        private ServiceItemBase _selectedAccount;
        private ImageSource _icon;

        public static ServiceItemBase EmptyAccountItem = new EmptyAccount();

        public ServiceBase(string displayName)
        {
            DisplayName = displayName;
            _selectedAccount = EmptyAccountItem;
            Accounts = new Collection<ServiceItemBase>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DisplayName
        {
            get;
            protected set;
        }

        public ImageSource Icon => _icon ?? (_icon = LoadIcon());

        public virtual ServiceItemBase SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (value != null)
                {
                    _selectedAccount = value;
                    Notify(nameof(SelectedAccount));
                }
            }
        }

        public Collection<ServiceItemBase> Accounts { get; protected set; }

        protected virtual ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("GrayRectangleDrawingImage") as DrawingImage;
        }
    }

    public class ServiceItemBase
    {
        public ServiceItemBase(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName
        {
            get;
            protected set;
        }
    }

    public class EmptyAccount : ServiceItemBase
    {
        public EmptyAccount()
            : base(Properties.Resources.TEXT_ADD_ACCOUNT)
        {

        }
    }
}