using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Controls;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System.IO;
using System.Windows.Media;

namespace SafeShare.WPFUI.ViewModel
{
    public class EmailEncryptionPageViewModel : ObservableObject
    {
        private EmailEncryptionPage _emailEncryptionPage;

        private string _zipFileName;
        private string _suggestedPassword;
        private string _customPassword;
        private bool _isPasswordCopied;
        private bool _isPasswordHide;
        private bool _emailAttachmentIsChecked;
        private bool _emailLinkIsChecked;
        private bool _encryptFileIsChecked;
        private bool _encryptFileIsEnabled;
        private string _fileNameErrorTips;
        private bool _showFileNameErrorTips;

        private EmailEncryptionPageViewModelCommand _viewModelCommands;

        public EmailEncryptionPageViewModel(EmailEncryptionPage page, FileListPage fileListPage)
        {
            FileListPageView = fileListPage;
            _emailAttachmentIsChecked = CanUseEmailAttachment;
            _emailLinkIsChecked = !CanUseEmailAttachment;
            TrackHelper.TrackHelperInstance.ShareType = _emailAttachmentIsChecked ? ShareType.Attachment : ShareType.Link;
            _encryptFileIsEnabled = true;
            _emailEncryptionPage = page;
            _zipFileName = GetDefaultZipFileName();
            IsPasswordCustom = false;
            IsCustomFileName = false;
            IsPasswordHide = true;

            WorkFlowManager.Initializer.ShareOptionDataInitializationTask.WaitWithMsgPump();
        }

        public EmailEncryptionPageViewModelCommand ViewModelCommands
        {
            get
            {
                if (_viewModelCommands == null)
                {
                    _viewModelCommands = new EmailEncryptionPageViewModelCommand(this);
                }
                return _viewModelCommands;
            }
        }

        public bool IsCustomFileName { set; get; }

        public FileListPage FileListPageView { set; get; }

        public string TotalUnzipSize
        {
            get
            {
                return string.Format(Properties.Resources.TOTAL_UNZIP_SIZE, FileSizeConverter.StrFormatByteSize(FileTotalSize), ItemTotalCount);
            }
        }

        public long FileTotalSize
        {
            get
            {
                if (FileListPageView.DataContext is FileListPageViewModel fileListViewModel)
                {
                    return fileListViewModel.FileTotalSize;
                }

                return 0;
            }
        }

        public bool CanUseEmailAttachment => FileTotalSize <= 10 * 1024 * 1024;

        public string ItemTotalCount
        {
            get
            {
                if (FileListPageView.DataContext is FileListPageViewModel fileListViewModel)
                {
                    return fileListViewModel.ItemTotalCount;
                }

                return string.Empty;
            }
        }

        public string ZipExt
        {
            get
            {
                var zipExt = ".zip";
                var fileListViewModel = FileListPageView.DataContext as FileListPageViewModel;
                var firstFile = fileListViewModel.ItemsFullPathList[0];
                if (fileListViewModel.ItemsFullPathList.Count == 1 && WinZipMethodHelper.ValidateZipFile(firstFile) && !EncryptFileIsChecked)
                {
                    zipExt = Path.GetExtension(FileOperation.RemoveInvalidCharactersInFileName(firstFile));
                }
                return zipExt;
            }
        }

        public string ZipFileName
        {
            get => _zipFileName;
            set
            {
                if (_zipFileName != value)
                {
                    _zipFileName = value;
                    Notify(nameof(ZipFileName));
                }
            }
        }

        public bool EmailAttachmentIsChecked
        {
            get => _emailAttachmentIsChecked;
            set
            {
                if (_emailAttachmentIsChecked != value)
                {
                    _emailAttachmentIsChecked = value;
                    EmailLinkIsChecked = !_emailAttachmentIsChecked;
                    TrackHelper.TrackHelperInstance.ShareType = _emailAttachmentIsChecked ? ShareType.Attachment : ShareType.Link;
                    Notify(nameof(EmailAttachmentIsChecked));
                    Notify(nameof(ShowAddAccountSubTitle));
                    Notify(nameof(ShowAddCloudSubTitle));
                    Notify(nameof(CanGoNextStep));
                }
            }
        }

        public bool EmailLinkIsChecked
        {
            get => _emailLinkIsChecked;
            set
            {
                if (_emailLinkIsChecked != value)
                {
                    _emailLinkIsChecked = value;
                    EmailAttachmentIsChecked = !_emailLinkIsChecked;
                    TrackHelper.TrackHelperInstance.ShareType = _emailAttachmentIsChecked ? ShareType.Attachment : ShareType.Link;
                    Notify(nameof(EmailLinkIsChecked));
                    Notify(nameof(ShowAddAccountSubTitle));
                    Notify(nameof(ShowAddCloudSubTitle));
                    Notify(nameof(CanGoNextStep));
                }
            }
        }

        public bool EncryptFileIsChecked
        {
            get => _encryptFileIsChecked;
            set
            {
                if (_encryptFileIsChecked != value)
                {
                    _encryptFileIsChecked = value;
                    Notify(nameof(EncryptFileIsChecked));
                }
            }
        }

        public bool EncryptFileIsEnabled
        {
            get => _encryptFileIsEnabled;
            set
            {
                if (_encryptFileIsEnabled != value)
                {
                    _encryptFileIsEnabled = value;
                    Notify(nameof(EncryptFileIsEnabled));
                }
            }
        }

        public string FileNameErrorTips
        {
            get => _fileNameErrorTips;
            set
            {
                if (_fileNameErrorTips != value)
                {
                    _fileNameErrorTips = value;
                    Notify(nameof(FileNameErrorTips));
                }
            }
        }

        public bool ShowFileNameErrorTips
        {
            get => _showFileNameErrorTips;
            set
            {
                if (_showFileNameErrorTips != value)
                {
                    _showFileNameErrorTips = value;
                    Notify(nameof(ShowFileNameErrorTips));
                }
            }
        }

        public bool ShowAddAccountSubTitle => WorkFlowManager.ShareOptionData.UseWinZipEmailer && EmailAttachmentIsChecked 
                                                && (WorkFlowManager.ShareOptionData.SelectedEmail == null || WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount is EmptyAccount);

        public bool ShowAddCloudSubTitle => EmailLinkIsChecked && (WorkFlowManager.ShareOptionData.SelectedCloud == null
                                                || WorkFlowManager.ShareOptionData.SelectedCloud.SelectedAccount is EmptyAccount);

        public bool CanGoNextStep => EmailAttachmentIsChecked || EmailLinkIsChecked;

        public bool IsPasswordCustom
        {
            get;
            set;
        }

        public string SuggestedPassword
        {
            get => _suggestedPassword;
            set
            {
                if (_suggestedPassword != value)
                {
                    _suggestedPassword = value;
                }
                Notify(nameof(EncryptPassword));
            }
        }

        public string CustomPassword
        {
            get => _customPassword;
            set
            {
                if (_customPassword != value)
                {
                    _customPassword = value;
                }
                Notify(nameof(EncryptPassword));
            }
        }

        public string EncryptPassword
        {
            get
            {
                TrackHelper.TrackHelperInstance.StdPassword = !IsPasswordCustom;
                return IsPasswordCustom ? CustomPassword : SuggestedPassword;
            }
        }

        public bool IsPasswordCopied
        {
            get => _isPasswordCopied;
            set
            {
                if (_isPasswordCopied != value)
                {
                    _isPasswordCopied = value;
                }
            }
        }

        public bool IsPasswordHide
        {
            get => _isPasswordHide;
            set
            {
                if (_isPasswordHide != value)
                {
                    _isPasswordHide = value;
                    Notify(nameof(IsPasswordHide));
                }
            }
        }

        public ImageSource SelectedEmailServiceIcon
        {
            get
            {
                if (WorkFlowManager.ShareOptionData.SelectedEmail != null && !(WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount is EmptyAccount))
                {
                    return WorkFlowManager.ShareOptionData.SelectedEmail.Icon;
                }
                else
                {
                    return null;
                }
            }
        }

        public string SelectedEmailAccount
        {
            get
            {
                if (WorkFlowManager.ShareOptionData.UseWinZipEmailer)
                {
                    if (WorkFlowManager.ShareOptionData.SelectedEmail == null || WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount is EmptyAccount)
                    {
                        return Properties.Resources.TEXT_ADD_ACCOUNT;
                    }
                    else
                    {
                        return WorkFlowManager.ShareOptionData.SelectedEmail.SelectedAccount.DisplayName;
                    }
                }
                else
                {
                    return WorkFlowManager.ShareOptionData.SelectedEmail?.DisplayName ?? string.Empty;
                }
            }
        }

        public ImageSource SelectedCloudServiceIcon
        {
            get
            {
                if (WorkFlowManager.ShareOptionData.SelectedCloud != null
                    && !(WorkFlowManager.ShareOptionData.SelectedCloud.SelectedAccount is EmptyAccount))
                {
                    return WorkFlowManager.ShareOptionData.SelectedCloud.Icon;
                }
                else
                {
                    return null;
                }
            }
        }

        public string SelectedCloudAccount
        {
            get
            {
                if (WorkFlowManager.ShareOptionData.SelectedCloud == null || WorkFlowManager.ShareOptionData.SelectedCloud.SelectedAccount is EmptyAccount)
                {
                    return Properties.Resources.TEXT_ADD_ACCOUNT;
                }
                else
                {
                    return WorkFlowManager.ShareOptionData.SelectedCloud.SelectedAccount.DisplayName;
                }
            }
        }

        public string GetDefaultZipFileName()
        {
            var fileListViewModel = FileListPageView.DataContext as FileListPageViewModel;
            if (fileListViewModel.ItemsFullPathList.Count > 0)
            {
                var firstFile = fileListViewModel.ItemsFullPathList[0];
                if (Directory.Exists(firstFile))
                {
                    return Path.GetFileName(firstFile) + ZipExt;
                }
                else
                {
                    return Path.GetFileNameWithoutExtension(firstFile) + ZipExt;
                }
            }
            else
            {
                // no file
                return string.Empty;
            }
        }

        public void GenerateSuggestedPasswordIfEmpty()
        {
            if (string.IsNullOrEmpty(SuggestedPassword))
            {
                SuggestedPassword = PasswordGenerater.GenerateSuggestedPassword();
            }
        }

        public void UpdateZipFileName()
        {
            if (!IsCustomFileName)
            {
                ZipFileName = GetDefaultZipFileName();
            }
            else
            {
                ZipFileName = Path.GetFileNameWithoutExtension(ZipFileName) + ZipExt;
            }
        }

        public void NotifyEmailAndCloudChanges()
        {
            Notify(nameof(SelectedEmailServiceIcon));
            Notify(nameof(SelectedEmailAccount));
            Notify(nameof(ShowAddAccountSubTitle));
            Notify(nameof(ShowAddCloudSubTitle));
            Notify(nameof(SelectedCloudServiceIcon));
            Notify(nameof(SelectedCloudAccount));
        }

        public void NotifyFilesizeAndCount()
        {
            Notify(nameof(FileTotalSize));
            Notify(nameof(ItemTotalCount));
            Notify(nameof(TotalUnzipSize));
            Notify(nameof(CanUseEmailAttachment));
            if (!CanUseEmailAttachment)
            {
                EmailAttachmentIsChecked = false;
            }
        }

        public void ExecuteNextCommand()
        {
            _emailEncryptionPage.NextPage();
        }
    }
}