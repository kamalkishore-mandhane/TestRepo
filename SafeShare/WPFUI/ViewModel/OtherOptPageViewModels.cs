using SafeShare.Properties;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System.Windows;

namespace SafeShare.WPFUI.ViewModel
{
    public enum OtherOpts
    {
        SAVE_COPY_OPT,
        SCHEDULE_DELETION_OPT
    }

    public class OtherOptPageViewModels : ObservableObject
    {
        private OtherOptPage _otherOptPageView;
        private OtherOpts _currentOpt;
        private bool _isSaveCopyPanelVisible;
        private bool _isLocalDeviceChecked;
        private string _saveCopyPath;
        private bool _isScheduleDeletionPanelVisible;
        private int _deleteDays;

        public OtherOptPageViewModels(OtherOptPage view, OtherOpts opt)
        {
            _otherOptPageView = view;
            _currentOpt = opt;

            DeleteDays = Settings.Default.DeleteFileDays;
            SaveCopyPath = Settings.Default.SaveFilePath;
            _isSaveCopyPanelVisible = _currentOpt == OtherOpts.SAVE_COPY_OPT;
            _isScheduleDeletionPanelVisible = _currentOpt == OtherOpts.SCHEDULE_DELETION_OPT;
        }

        public bool IsSaveCopyPanelVisible
        {
            get
            {
                return _isSaveCopyPanelVisible;
            }
            set
            {
                if (_isSaveCopyPanelVisible != value)
                {
                    _isSaveCopyPanelVisible = value;
                    Notify(nameof(IsSaveCopyPanelVisible));
                }
            }
        }

        public bool IsLocalDeviceChecked
        {
            get
            {
                return _isLocalDeviceChecked;
            }
            set
            {
                if (_isLocalDeviceChecked != value)
                {
                    _isLocalDeviceChecked = value;
                    Notify(nameof(IsLocalDeviceChecked));
                }
            }
        }

        public string SaveCopyPath
        {
            get
            {
                return _saveCopyPath;
            }
            set
            {
                if (_saveCopyPath != value)
                {
                    Settings.Default.SaveFilePath = value;
                    Settings.Default.Save();

                    _saveCopyPath = value;
                    Notify(nameof(SaveCopyPath));
                }
            }
        }

        public bool IsScheduleDeletionPanelVisible
        {
            get
            {
                return _isScheduleDeletionPanelVisible;
            }
            set
            {
                if (_isScheduleDeletionPanelVisible != value)
                {
                    _isScheduleDeletionPanelVisible = value;
                    Notify(nameof(IsScheduleDeletionPanelVisible));
                }
            }
        }

        public int DeleteDays
        {
            get
            {
                return _deleteDays;
            }
            set
            {
                if (_deleteDays != value)
                {
                    _deleteDays = value;
                    Notify(nameof(DeleteDays));
                }
            }
        }

        public void ClickBrowserButtonAction()
        {
            var title = Resources.SELECTFOLDER_PICKER_TITLE;
            var defaultBtn = Properties.Resources.SELECTFOLDER_PICKER_BUTTON;
            var defaultFolder = WinZipMethodHelper.InitWzCloudItem();
            defaultFolder.itemId = Settings.Default.SaveFilePath;
            defaultFolder.isFolder = true;
            WzCloudItem4 selectedItem = WinZipMethodHelper.InitWzCloudItem();
            selectedItem.profile.Id = WzSvcProviderIDs.SPID_UNKNOWN;

            var mainWindow = VisualTreeHelperUtils.FindAncestor<Window>(_otherOptPageView) as SafeShareView;
            bool res = WinzipMethods.DestinationFolderSelection(mainWindow.WindowHandle, title, defaultBtn, defaultFolder, ref selectedItem, false, true, true, false);
            if (res)
            {
                SaveCopyPath = selectedItem.itemId;
            }
        }
    }
}