using SafeShare.Util;
using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace SafeShare.WPFUI.ViewModel
{
    public class ManageCloudPageViewModel : ObservableObject
    {
        private const int DisplayedServiceCount = 5;

        private bool _isInitialized;

        private bool _isSeeAllButtonVisible;
        private string _manageServiceErrorMessage;
        private CloudService _selectedService;

        private ModelCommand _back;
        private ModelCommand _manageAccounts;
        private ModelCommand _seeAll;
        private ModelCommand _removeAccount;
        private ModelCommand _showAddAccountPage;
        private ModelCommand _doneManageService;

        private ObservableCollection<CloudService> _displayCloudServices;

        private ManageCloudPage _manageCloudPage;
        private ServiceSettingsPage _settingsPage;

        public ManageCloudPageViewModel(ManageCloudPage manageCloudPage)
        {
            _isInitialized = false;
            _manageCloudPage = manageCloudPage;
            _settingsPage = new ServiceSettingsPage(this);
            _displayCloudServices = new ObservableCollection<CloudService>();

            try
            {
                LoadSettings();
            }
            catch (Exception e)
            {
                SimpleMessageWindows.DisplayWarningConfirmationMessage(e.InnerException.Message);
            }
        }

        #region Static Fields

        public static Dictionary<WzSvcProviderIDs, CloudService> CloudServicesDictionary;

        #endregion Static Fields

        #region Properties

        public bool IsInternalNavigation
        {
            get;
            private set;
        }

        public bool IsSeeAllButtonVisible
        {
            get => _isSeeAllButtonVisible;
            set
            {
                if (value != _isSeeAllButtonVisible)
                {
                    _isSeeAllButtonVisible = value;
                    Notify(nameof(IsSeeAllButtonVisible));
                }
            }
        }

        public string ManageServiceErrorMessage
        {
            get => _manageServiceErrorMessage;
            set
            {
                if (_manageServiceErrorMessage != value)
                {
                    _manageServiceErrorMessage = value;
                    Notify(nameof(ManageServiceErrorMessage));
                }
            }
        }

        public CloudService SelectedService
        {
            get => _selectedService;
            set
            {
                if (value != _selectedService)
                {
                    _selectedService = value;
                    Notify(nameof(SelectedService));
                }

                // Add account when click list view item
                if (_isInitialized && _selectedService.Accounts.Count == 0)
                {
                    ManageAccountContext.SetService(_selectedService);
                    ExecuteShowAddAccountPageCommand(_manageCloudPage);
                }
            }
        }

        public ObservableCollection<CloudService> DisplayCloudServices => _displayCloudServices;

        public ManageAccountContext ManageAccountContext { get; private set; } = new ManageAccountContext();

        #endregion Properties

        #region Initialization Methods

        private void LoadSettings()
        {
            WorkFlowManager.Initializer.CloudServiceInitializationTask.WaitWithMsgPump();

            if (CloudServicesDictionary.Count == 0)
            {
                return;
            }

            // Fill cloud services to display
            int count = 0;
            // Put ZipShare to top
            if (CloudServicesDictionary.ContainsKey(WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE))
            {
                _displayCloudServices.Add(CloudServicesDictionary[WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE]);
                ++count;
            }
            foreach (var service in CloudServicesDictionary.Values)
            {
                if (count >= DisplayedServiceCount)
                {
                    break;
                }

                if (!_displayCloudServices.Contains(service))
                {
                    _displayCloudServices.Add(service);
                    ++count;
                }
            }

            // Check the radio button of current selected service
            if (WorkFlowManager.ShareOptionData.SelectedCloud != null)
            {
                if (_displayCloudServices.Contains(WorkFlowManager.ShareOptionData.SelectedCloud))
                {
                    SelectedService = WorkFlowManager.ShareOptionData.SelectedCloud;
                }
                else if (!_displayCloudServices.Contains(WorkFlowManager.ShareOptionData.SelectedCloud)
                    && CloudServicesDictionary.ContainsKey(WorkFlowManager.ShareOptionData.SelectedCloud.Spid))
                {
                    ExecuteSeeAllCommand(null);
                    SelectedService = WorkFlowManager.ShareOptionData.SelectedCloud;
                }
                else
                {
                    SelectedService = _displayCloudServices[0];
                }
            }
            else if (_displayCloudServices.Count > 0)
            {
                SelectedService = _displayCloudServices[0];
            }

            IsSeeAllButtonVisible = CloudServicesDictionary.Count > DisplayedServiceCount && CloudServicesDictionary.Count > _displayCloudServices.Count;

            // Selected the first accounts if it doesn't have a default account
            foreach (var service in CloudServicesDictionary.Values)
            {
                if (service.Accounts.Count > 0 && service.SelectedAccount is EmptyAccount)
                {
                    service.SelectedAccount = service.Accounts[0];
                }
            }

            _isInitialized = true;
        }

        #endregion Initialization Methods

        #region Commands

        public ModelCommand Back => _back ?? (_back = new ModelCommand(ExecuteBackCommand, p => true));

        public void ExecuteBackCommand(object parameter)
        {
            IsInternalNavigation = false;
            WorkFlowManager.GoBack();
        }

        public ModelCommand ManageAccounts => _manageAccounts ?? (_manageAccounts = new ModelCommand(ExecuteManageAccountsCommand, p => true));

        private void ExecuteManageAccountsCommand(object parameter)
        {
            var service = parameter as CloudService;
            ManageAccountContext.SetService(service);
            if (service != null)
            {
                if (service.Accounts.Count > 0)
                {
                    IsInternalNavigation = true;
                    _manageCloudPage.NavigationService.Navigate(_settingsPage);
                }
                else
                {
                    ExecuteShowAddAccountPageCommand(_manageCloudPage);
                }
            }
        }

        public ModelCommand SeeAll => _seeAll ?? (_seeAll = new ModelCommand(ExecuteSeeAllCommand, p => _displayCloudServices.Count <= DisplayedServiceCount));

        public void ExecuteSeeAllCommand(object parameter)
        {
            foreach (var service in CloudServicesDictionary.Values)
            {
                if (!_displayCloudServices.Contains(service))
                {
                    _displayCloudServices.Add(service);
                }
            }
            IsSeeAllButtonVisible = false;
        }

        public ModelCommand RemoveAccount => _removeAccount ?? (_removeAccount = new ModelCommand(ExecuteRemoveAccountCommand, p => true));

        private void ExecuteRemoveAccountCommand(object parameter)
        {
            var account = parameter as CloudItem;
            ManageAccountContext.RemoveAccount(account);
            WinzipMethods.LogoutCloud(IntPtr.Zero, account.Service.Spid, account.Service.Spid.ToString("d") + "|" + account.AuthId);
        }

        public ModelCommand ShowAddAccountPage => _showAddAccountPage ?? (_showAddAccountPage = new ModelCommand(ExecuteShowAddAccountPageCommand, p => true));

        private void ExecuteShowAddAccountPageCommand(object parameter)
        {
            var page = parameter as Page;
            page.IsEnabled = false;

            var service = ManageAccountContext.Service as CloudService;
            WzProfile2[] profile = new WzProfile2[1];
            WinzipMethods.AuthorizeCloud(new WindowInteropHelper(page.Parent as Window).Handle, service.Spid, string.Empty, string.Empty, profile);

            page.IsEnabled = true;

            // WinzipMethods.AuthorizeCloud can only return the first logged in account profile when logging in more than one accounts
            // So if log in more than one account, read the registry to get new account profile instead.
            if (service.Accounts.Count == 0)
            {
                if (!string.IsNullOrEmpty(profile[0].authId))
                {
                    string[] parts = profile[0].authId.Split('|');
                    if (parts.Length == 2)
                    {
                        var account = new CloudItem(service, parts[1], profile[0].name);
                        ManageAccountContext.AddAccount(account);
                    }
                }
            }
            else
            {
                bool alreadyLoggedIn = false;
                string authSubKey = service.RegistryPath + "\\Auth";
                var authGuids = RegeditOperation.GetCurrentUserRegistrySubKeyNames(authSubKey);
                foreach (var authGuid in authGuids)
                {
                    string subKey = authSubKey + "\\" + authGuid;
                    string authId = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "AuthID");

                    foreach (var account in service.Accounts)
                    {
                        // Found in previously logged in accounts
                        if (string.Compare((account as CloudItem).AuthId, authId, true) == 0)
                        {
                            alreadyLoggedIn = true;
                            break;
                        }
                    }

                    // Found the newly added account
                    if (!alreadyLoggedIn)
                    {
                        string nickname = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "NickName");
                        ManageAccountContext.AddAccount(new CloudItem(service, authId, nickname));
                        break;
                    }

                    alreadyLoggedIn = false;
                }
            }
        }

        public ModelCommand DoneManageService => _doneManageService ?? (_doneManageService = new ModelCommand(ExecuteDoneManageServiceCommand, p => true));

        private void ExecuteDoneManageServiceCommand(object parameter)
        {
            ManageServiceErrorMessage = string.Empty;
            IsInternalNavigation = false;

            if (!(SelectedService?.SelectedAccount is CloudItem))
            {
                ManageServiceErrorMessage = Properties.Resources.ACCOUNT_NEEDED;
                return;
            }

            var selectedAccount = SelectedService.SelectedAccount as CloudItem;
            RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudSpidKey, SelectedService.Spid.ToString("d"));
            RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudAuthIdKey, SelectedService.Spid.ToString("d") + "|" + selectedAccount?.AuthId);
            RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudNickNameKey, selectedAccount?.Nickname);

            WorkFlowManager.ShareOptionData.SelectedCloud = SelectedService;
            WorkFlowManager.DoneManageCloud();
        }

        #endregion Commands

        public bool HandleCloudServiceDiabled()
        {
            if (CloudServicesDictionary.Count == 0)
            {
                SimpleMessageWindows.DisplayWarningConfirmationMessage(Properties.Resources.CLOUDSERVICE_DISABLED_INFO);
                return true;
            }

            return false;
        }
    }
}