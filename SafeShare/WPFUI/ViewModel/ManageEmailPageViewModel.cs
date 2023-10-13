using Microsoft.Win32;
using SafeShare.Util;
using SafeShare.WPFUI.Commands;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Xml.Linq;

namespace SafeShare.WPFUI.ViewModel
{
    public class ManageEmailPageViewModel : ObservableObject
    {
        private const int DisplayedServiceCount = 4;
        private const int FakeEmailServiceCount = 2; // 'User's Own Program' and 'Advanced Setup'

        private bool _isInitialized;
        private bool _isRefreshingDisplayList;

        private bool _isSeeAllButtonVisible;
        private string _manageServiceErrorMessage;
        private string _addAccountErrorMessage;
        private string _addServiceErrorMessage;
        private EmailService _selectedService;

        private ModelCommand _back;
        private ModelCommand _manageAccounts;
        private ModelCommand _seeAll;
        private ModelCommand _advancedSetup;
        private ModelCommand _removeAccount;
        private ModelCommand _showAddAccountPage;
        private ModelCommand _doneAddAccount;
        private ModelCommand _doneManageService;

        private ObservableCollection<EmailService> _displayEmailServices;

        private Collection<EmailService> _customEmailServices;
        private Collection<EmailService> _knownEmailServices;
        private Collection<EmailService> _fakeEmailServices;

        private ManageEmailPage _manageEmailPage;
        private ServiceSettingsPage _settingsPage;
        private AddEmailServicePage _addEmailServicePage;
        private AddEmailAccountPage _addEmailAccountPage;

        private AddAccountContext _addAccountContext;

        public ManageEmailPageViewModel(ManageEmailPage manageEmailPage)
        {
            _isInitialized = false;
            _isRefreshingDisplayList = false;

            _manageEmailPage = manageEmailPage;

            _settingsPage = new ServiceSettingsPage(this);
            _addEmailServicePage = new AddEmailServicePage(this);
            _addEmailAccountPage = new AddEmailAccountPage(this);

            _displayEmailServices = new ObservableCollection<EmailService>();

            _customEmailServices = new Collection<EmailService>();
            _knownEmailServices = new Collection<EmailService>();
            _fakeEmailServices = new Collection<EmailService>();

            // Context used in add account workflow
            _addAccountContext = null;

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

        public static EmailServicesData EmailServicesData = new EmailServicesData();

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

        public string AddAccountErrorMessage
        {
            get => _addAccountErrorMessage;
            set
            {
                if (_addAccountErrorMessage != value)
                {
                    _addAccountErrorMessage = value;
                    Notify(nameof(AddAccountErrorMessage));
                }
            }
        }

        public string AddServiceErrorMessage
        {
            get => _addServiceErrorMessage;
            set
            {
                if (_addServiceErrorMessage != value)
                {
                    _addServiceErrorMessage = value;
                    Notify(nameof(AddServiceErrorMessage));
                }
            }
        }

        public EmailService SelectedService
        {
            get => _selectedService;
            set
            {
                if (value != _selectedService)
                {
                    _selectedService = value;
                    Notify(nameof(SelectedService));
                }
            }
        }

        public ObservableCollection<EmailService> DisplayEmailServices => _displayEmailServices;

        public ManageAccountContext ManageAccountContext
        {
            get;
            private set;
        } = new ManageAccountContext();

        #endregion Properties

        #region Initialization Methods

        private void LoadSettings()
        {
            WorkFlowManager.Initializer.EmailServiceInitializationTask.WaitWithMsgPump();

            // Sort with display order
            var orderedServices = EmailServicesData.EmailServicesDictionary.OrderBy(pair => pair.Value.DisplayOrder);

            foreach (var pair in orderedServices)
            {
                if (pair.Value is CustomEmailService)
                {
                    _customEmailServices.Add(pair.Value);
                }

                if (pair.Value.GetType().IsDefined(typeof(KnownEmailServiceAttribute), false))
                {
                    _knownEmailServices.Add(pair.Value);
                }
            }

            _fakeEmailServices.Add(new UsersOwnProgram());
            _fakeEmailServices.Add(new AdvancedSetup());

            FillDisplayEmailServices(DisplayedServiceCount);

            // Check the radio button of current selected service
            if (!WorkFlowManager.ShareOptionData.UseWinZipEmailer)
            {
                SelectedService = _displayEmailServices.Where(e => e is UsersOwnProgram).FirstOrDefault();
            }
            else if (WorkFlowManager.ShareOptionData.SelectedEmail != null)
            {
                if (_displayEmailServices.Contains(WorkFlowManager.ShareOptionData.SelectedEmail))
                {
                    SelectedService = WorkFlowManager.ShareOptionData.SelectedEmail;
                }
                else if (!_displayEmailServices.Contains(WorkFlowManager.ShareOptionData.SelectedEmail)
                    && EmailServicesData.EmailServicesDictionary.ContainsKey(WorkFlowManager.ShareOptionData.SelectedEmail.Name))
                {
                    ExecuteSeeAllCommand(null);
                    SelectedService = WorkFlowManager.ShareOptionData.SelectedEmail;
                }
                else
                {
                    SelectedService = _displayEmailServices[0];
                }
            }
            else
            {
                SelectedService = _displayEmailServices[0];
            }

            IsSeeAllButtonVisible = EmailServicesData.EmailServicesDictionary.Count > DisplayedServiceCount && EmailServicesData.EmailServicesDictionary.Count > _displayEmailServices.Count - FakeEmailServiceCount;

            // Selected the first accounts if it doesn't have a default account
            foreach (var service in EmailServicesData.EmailServicesDictionary.Values)
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
            ClearErrorMessage();
            var page = parameter as Page;

            if (page is AddEmailServicePage addEmailServicePage)
            {
                if (!string.IsNullOrEmpty(addEmailServicePage.ServiceNameTextBox.Text)
                    || !string.IsNullOrEmpty(addEmailServicePage.ServerTextBox.Text))
                {
                    if (!SimpleMessageWindows.DisplayWarningMessage(Properties.Resources.WARNING_DISCARD_CHANGES))
                    {
                        return;
                    }
                }

                addEmailServicePage.NavigationService?.GoBack();
                addEmailServicePage.Clear();
            }
            else if (page is AddEmailAccountPage addEmailAccountPage)
            {
                if (!string.IsNullOrEmpty(addEmailAccountPage.SenderNameTextBox.Text)
                    || !string.IsNullOrEmpty(addEmailAccountPage.SenderAddressTextBox.Text)
                    || !string.IsNullOrEmpty(addEmailAccountPage.UserNameTextBox.Text))
                {
                    if (!SimpleMessageWindows.DisplayWarningMessage(Properties.Resources.WARNING_DISCARD_CHANGES))
                    {
                        return;
                    }
                }

                addEmailAccountPage.NavigationService?.GoBack();
                addEmailAccountPage.Clear();
            }
            else
            {
                WorkFlowManager.GoBack();
                _addAccountContext = null;
                IsInternalNavigation = false;
            }
        }

        public ModelCommand ManageAccounts => _manageAccounts ?? (_manageAccounts = new ModelCommand(ExecuteManageAccountsCommand, p => true));

        private void ExecuteManageAccountsCommand(object parameter)
        {
            ClearErrorMessage();

            var service = parameter as EmailService;

            if (service is UsersOwnProgram)
            {
                Process.Start("ms-settings:emailandaccounts");
            }
            else
            {
                ManageAccountContext.SetService(service);

                // If the service doesn't have any account, go to the add account page
                if (ManageAccountContext.Service.Accounts.Count == 0)
                {
                    ExecuteShowAddAccountPageCommand(_manageEmailPage);
                }
                // If the service has account(s), go to manage account page
                else
                {
                    IsInternalNavigation = true;
                    _manageEmailPage.NavigationService.Navigate(_settingsPage);
                }
            }
        }

        public ModelCommand SeeAll => _seeAll ?? (_seeAll = new ModelCommand(ExecuteSeeAllCommand, p => true));

        public void ExecuteSeeAllCommand(object parameter)
        {
            FillDisplayEmailServices(EmailServicesData.EmailServicesDictionary.Count);
            IsSeeAllButtonVisible = false;
        }

        public ModelCommand RemoveAccount => _removeAccount ?? (_removeAccount = new ModelCommand(ExecuteRemoveAccount, p => true));

        private void ExecuteRemoveAccount(object parameter)
        {
            var itemToRemove = parameter as EmailItem;
            ManageAccountContext.RemoveAccount(itemToRemove);
            string subKeyName = RegeditOperation.WzEmailAccountsKey + "\\" + itemToRemove.SenderAddress;
            RegeditOperation.RemoveCurrentUserRegistryKey(subKeyName);
        }

        public ModelCommand ShowAddAccountPage => _showAddAccountPage ?? (_showAddAccountPage = new ModelCommand(ExecuteShowAddAccountPageCommand, p => true));

        private void ExecuteShowAddAccountPageCommand(object parameter)
        {
            if (parameter is Page callerPage)
            {
                ClearErrorMessage();

                // Case 1: Add new service and account
                if (callerPage is AddEmailServicePage addEmailServicePage)
                {
                    if (EmailServicesData.EmailServicesDictionary.ContainsKey(addEmailServicePage.ServiceNameTextBox.Text))
                    {
                        AddServiceErrorMessage = Properties.Resources.SERVICE_NAME_CONFLICT;
                        return;
                    }

                    if (string.IsNullOrEmpty(addEmailServicePage.ServiceNameTextBox.Text)
                        || string.IsNullOrEmpty(addEmailServicePage.EncryptionComboBox.Text)
                        || string.IsNullOrEmpty(addEmailServicePage.ServerTextBox.Text)
                        || string.IsNullOrEmpty(addEmailServicePage.PortTextBox.Text))
                    {
                        AddServiceErrorMessage = Properties.Resources.REQUIRED_FIELD_EMPTY;
                        return;
                    }

                    _addAccountContext = new AddAccountContext
                    {
                        AddingService = true,
                        EmailService = new CustomEmailService(
                        addEmailServicePage.ServiceNameTextBox.Text,
                        addEmailServicePage.LoginCheckBox.IsChecked == true ? "yes" : "no",
                        "no",
                        addEmailServicePage.EncryptionComboBox.Text,
                        addEmailServicePage.ServerTextBox.Text,
                        addEmailServicePage.PortTextBox.Text,
                        string.Empty)
                    };
                    _addEmailAccountPage.StepText.Visibility = Visibility.Visible;

                    _addEmailAccountPage.Clear();
                    IsInternalNavigation = true;
                    callerPage.NavigationService.Navigate(_addEmailAccountPage);
                }
                // Case 2: Add account use oauth
                else if (ManageAccountContext.Service?.GetType().IsDefined(typeof(OAuth2EmailServiceAttribute), false) ?? false)
                {
                    AuthorizeEmailAccount(callerPage, ManageAccountContext.Service as EmailService);
                    return;
                }
                // Case 3: Add normal account
                else
                {
                    _addAccountContext = new AddAccountContext { AddingService = false, EmailService = ManageAccountContext.Service as EmailService };
                    _addEmailAccountPage.StepText.Visibility = Visibility.Hidden;

                    _addEmailAccountPage.Clear();
                    IsInternalNavigation = true;
                    callerPage.NavigationService.Navigate(_addEmailAccountPage);
                }
            }
        }

        public ModelCommand DoneAddAccount => _doneAddAccount ?? (_doneAddAccount = new ModelCommand(ExecuteDoneAddAccountCommand, p => true));

        private void ExecuteDoneAddAccountCommand(object parameter)
        {
            if (parameter is AddEmailAccountPage addEmailAccountPage && _addAccountContext != null)
            {
                ClearErrorMessage();

                if (string.IsNullOrEmpty(addEmailAccountPage.SenderNameTextBox.Text)
                    || string.IsNullOrEmpty(addEmailAccountPage.SenderAddressTextBox.Text)
                    || string.IsNullOrEmpty(addEmailAccountPage.UserNameTextBox.Text))
                {
                    AddAccountErrorMessage = Properties.Resources.REQUIRED_FIELD_EMPTY;
                    return;
                }

                foreach (var account in _addAccountContext.EmailService.Accounts)
                {
                    if (account is EmailItem emailItem && emailItem.SenderAddress.ToLower() == addEmailAccountPage.SenderAddressTextBox.Text.ToLower())
                    {
                        AddAccountErrorMessage = Properties.Resources.ACCOUNT_NAME_CONFLICT;
                        return;
                    }
                }

                if (_addAccountContext.AddingService)
                {
                    if (!EmailServicesData.EmailServicesDictionary.ContainsKey(_addAccountContext.EmailService.Name))
                    {
                        EmailServicesData.EmailServicesDictionary.Add(_addAccountContext.EmailService.Name, _addAccountContext.EmailService);
                        _customEmailServices.Add(_addAccountContext.EmailService);
                    }

                    FillDisplayEmailServices(_displayEmailServices.Count - FakeEmailServiceCount + 1);

                    IsSeeAllButtonVisible = EmailServicesData.EmailServicesDictionary.Count > DisplayedServiceCount && EmailServicesData.EmailServicesDictionary.Count > _displayEmailServices.Count - FakeEmailServiceCount;

                    // Serialize services settings to xml and store to registry
                    SaveEmailServices();

                    _addEmailServicePage.Clear();
                }

                var newItem = new EmailItem(
                    _addAccountContext.EmailService,
                    addEmailAccountPage.SenderNameTextBox.Text,
                    addEmailAccountPage.SenderAddressTextBox.Text,
                    addEmailAccountPage.UserNameTextBox.Text,
                    addEmailAccountPage.PasswordTextBox.Password,
                    "0",
                    string.Empty);

                // Add account info to registry
                SaveEmailAccount(newItem);

                _addAccountContext.EmailService.Accounts.Add(newItem);
                TrackHelper.LogAddEmailAccountEvent(newItem.SenderAddress, !(newItem.Service is CustomEmailService));

                IsInternalNavigation = true;
                addEmailAccountPage.NavigationService.Navigate(_manageEmailPage);
                _addAccountContext.EmailService.SelectedAccount = newItem;
                SelectedService = _addAccountContext.EmailService;
            }
        }

        public ModelCommand AdvancedSetup => _advancedSetup ?? (_advancedSetup = new ModelCommand(ExecuteAdvancedSetupCommand, p => true));

        public void ExecuteAdvancedSetupCommand(object parameter)
        {
            ClearErrorMessage();
            _manageEmailPage.NavigationService.Navigate(_addEmailServicePage);
            IsInternalNavigation = true;
        }

        public ModelCommand DoneManageService => _doneManageService ?? (_doneManageService = new ModelCommand(ExecuteDoneManageServiceCommand, p => true));

        private void ExecuteDoneManageServiceCommand(object parameter)
        {
            _addAccountContext = null;
            IsInternalNavigation = false;
            ClearErrorMessage();

            if (SelectedService.SelectedAccount is EmptyAccount)
            {
                ManageServiceErrorMessage = Properties.Resources.ACCOUNT_NEEDED;
                return;
            }

            WorkFlowManager.ShareOptionData.UseWinZipEmailer = !(SelectedService is UsersOwnProgram);

            // If use WinZip's emailer, set selected account as default email account
            if (WorkFlowManager.ShareOptionData.UseWinZipEmailer)
            {
                RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultAccountKey, (SelectedService.SelectedAccount as EmailItem)?.SenderAddress);
                RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultEmailService, SelectedService.Name);
                // Set default account
                RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzEmailAccountsKey, null, (SelectedService.SelectedAccount as EmailItem).SenderAddress);
            }

            // set UseMapi key
            RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzKey, RegeditOperation.WzUseMapiKey, WorkFlowManager.ShareOptionData.UseWinZipEmailer ? "0" : "1");

            WorkFlowManager.ShareOptionData.SelectedEmail = SelectedService;
            WorkFlowManager.DoneManageEmail();
        }

        #endregion Commands

        #region Helper functions

        public void ClearErrorMessage()
        {
            ManageServiceErrorMessage = string.Empty;
            AddAccountErrorMessage = string.Empty;
            AddServiceErrorMessage = string.Empty;
        }

        public void HandleSelectionChange(EmailService addedItem)
        {
            // Add account when click list view item
            if (addedItem != null
                && _isInitialized
                && !_isRefreshingDisplayList
                && addedItem.Accounts.Count == 0
                && !addedItem.GetType().IsDefined(typeof(FakeEmailServiceAttribute), false))
            {
                ManageAccountContext.SetService(addedItem);
                ExecuteShowAddAccountPageCommand(_manageEmailPage);
            }
        }

        private void FillDisplayEmailServices(int displayCount)
        {
            _isRefreshingDisplayList = true;

            int count = 0;
            var previousSelectedService = SelectedService;
            _displayEmailServices.Clear();

            // Put custom services to top
            // Bigger display order for custom service means newer addition
            foreach (var service in _customEmailServices.Reverse())
            {
                if (count < displayCount && !_displayEmailServices.Contains(service))
                {
                    _displayEmailServices.Add(service);
                    ++count;
                }
            }

            // Put known services next to custom services
            foreach (var service in _knownEmailServices)
            {
                if (count < displayCount && !_displayEmailServices.Contains(service))
                {
                    _displayEmailServices.Add(service);
                    ++count;
                }
            }

            // Put fake services to buttom
            // Fake services are always displayed
            foreach (var service in _fakeEmailServices)
            {
                _displayEmailServices.Add(service);
            }

            SelectedService = previousSelectedService;

            _isRefreshingDisplayList = false;
        }

        private void SaveEmailAccount(EmailItem item)
        {
            string subKeyName = RegeditOperation.WzEmailAccountsKey + "\\" + item.SenderAddress;
            RegeditOperation.SetCurrentUserStringValue(subKeyName, "OAuth", item.Oauth);
            RegeditOperation.SetCurrentUserStringValue(subKeyName, "SenderAddress", item.SenderAddress);
            RegeditOperation.SetCurrentUserStringValue(subKeyName, "SenderName", item.SenderName);
            RegeditOperation.SetCurrentUserStringValue(subKeyName, "Service", item.Service.Name);
            RegeditOperation.SetCurrentUserStringValue(subKeyName, "User", item.UserName);

            string passwordToken = item.PasswordToken;
            if (!string.IsNullOrEmpty(passwordToken))
            {
                WinzipMethods.ProtectData(IntPtr.Zero, ref passwordToken);
                RegeditOperation.SetCurrentUserStringValue(subKeyName, "WinInfo", passwordToken);
            }

            string refreshToken = item.RefreshToken;
            if (item.Oauth == "1" && !string.IsNullOrEmpty(refreshToken))
            {
                WinzipMethods.ProtectData(IntPtr.Zero, ref refreshToken);
                RegeditOperation.SetCurrentUserStringValue(subKeyName, "T1", refreshToken);
            }
        }

        private void SaveEmailServices()
        {
            var servicesXml = new XElement("mailservices", new XAttribute("default", EmailServicesData.DefaultServiceName));
            foreach (var service in EmailServicesData.EmailServicesDictionary.Values)
            {
                servicesXml.Add(new XElement("mailservice",
                    string.IsNullOrEmpty(service.Name) ? null : new XAttribute("name", service.Name),
                    string.IsNullOrEmpty(service.Login) ? null : new XAttribute("login", service.Login),
                    string.IsNullOrEmpty(service.Help) ? null : new XAttribute("help", service.Help),
                    string.IsNullOrEmpty(service.Encryption) ? null : new XAttribute("encryption", service.Encryption),
                        new XElement("smtp",
                            string.IsNullOrEmpty(service.Server) ? null : new XAttribute("server", service.Server),
                            string.IsNullOrEmpty(service.Port) ? null : new XAttribute("port", service.Port)),
                        new XElement("domains", service.Domains)));
            }

            RegeditOperation.SetCurrentUserStringValue(RegeditOperation.WzEmailServicesKey, null, servicesXml.ToString());
        }

        private bool AuthorizeEmailAccount(Page callerPage, EmailService service)
        {
            bool ret = false;

            try
            {
                // Disable caller page
                callerPage.IsEnabled = false;

                RecipientClient.Client client = null;

                if (service is OutlookService)
                {
                    if (OutlookService.Client == null)
                    {
                        client = RecipientClient.OutlookContacts.ClientFactory.Create(Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfOutlookContactKey),
                            Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfBaseKey), (int)WzSvcProviderIDs.SPID_RECIPIENT_OUTLOOKCONTACTS, "OUTLOOKCONTACTS");
                        OutlookService.Client = client;
                    }
                    else
                    {
                        client = OutlookService.Client;
                    }
                }
                else if (service is GmailService)
                {
                    if (GmailService.Client == null)
                    {
                        client = RecipientClient.GoogleContacts.ClientFactory.Create(Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfGoogleContactKey),
                            Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfBaseKey), (int)WzSvcProviderIDs.SPID_RECIPIENT_GOOGLECONTACTS, "GOOGLECONTACTS");
                        GmailService.Client = client;
                    }
                    else
                    {
                        client = GmailService.Client;
                    }
                }
                else if (service is YahooService)
                {
                    if (YahooService.Client == null)
                    {
                        client = RecipientClient.YahooContacts.ClientFactory.Create(Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfYahooContactKey),
                            Registry.CurrentUser.CreateSubKey(RegeditOperation.WzWxfBaseKey), (int)WzSvcProviderIDs.SPID_RECIPIENT_YAHOOCONTACTS, "YAHOOCONTACTS");
                        YahooService.Client = client;
                    }
                    else
                    {
                        client = YahooService.Client;
                    }
                }
                else
                {
                    ret = false;
                }

                string email = string.Empty;
                string refreshToken = string.Empty;
                string accessToken = string.Empty;
                bool succeeded = client.ExtraLogin(new WindowInteropHelper(callerPage.Parent as Window).Handle, false, ref email, ref refreshToken, ref accessToken, true);

                if (succeeded)
                {
                    bool alreadyExists = false;
                    foreach (var account in service.Accounts)
                    {
                        if (string.Compare((account as EmailItem).SenderAddress, email, true) == 0)
                        {
                            SimpleMessageWindows.DisplayWarningConfirmationMessage(Properties.Resources.ACCOUNT_NAME_CONFLICT);
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        var newItem = new EmailItem(service, email, email, email, string.Empty, "1", refreshToken);
                        ManageAccountContext.AddAccount(newItem);
                        TrackHelper.LogAddEmailAccountEvent(newItem.SenderAddress, !(newItem.Service is CustomEmailService));
                        SaveEmailAccount(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                // If exception occurs, set these cache to null for the next login
                OutlookService.Client = null;
                GmailService.Client = null;
                YahooService.Client = null;
                SimpleMessageWindows.DisplayWarningConfirmationMessage(e.InnerException?.Message ?? e.Message);
                ret = false;
            }

            callerPage.IsEnabled = true;
            return ret;
        }

        #endregion Helper functions
    }

    #region Helper classes

    public class AddAccountContext
    {
        public bool AddingService = false;
        public EmailService EmailService = null;
    }

    public class ManageAccountContext : ObservableObject
    {
        private ServiceItemBase _selectedAccount;
        private string _serviceName = string.Empty;

        public ServiceBase Service = null;

        public string ServiceName
        {
            get => _serviceName;
            private set
            {
                if (value != _serviceName)
                {
                    _serviceName = value;
                    Notify(nameof(ServiceName));
                }
            }
        }

        public ServiceItemBase SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                Service.SelectedAccount = value;
                Notify(nameof(SelectedAccount));
            }
        }

        public ObservableCollection<ServiceItemBase> DisplayAccounts { get; private set; } = new ObservableCollection<ServiceItemBase>();

        public void SetService(ServiceBase service)
        {
            if (service != null)
            {
                Service = service;
                ServiceName = service.DisplayName;

                DisplayAccounts.Clear();
                foreach (var account in Service.Accounts)
                {
                    DisplayAccounts.Add(account);
                }

                SelectedAccount = Service.SelectedAccount;
            }
        }

        public void RemoveAccount(ServiceItemBase account)
        {
            if (account != null)
            {
                if (_selectedAccount == account)
                {
                    Service?.Accounts.Remove(account);
                    DisplayAccounts.Remove(account);
                    SelectedAccount = DisplayAccounts.Count > 0 ? DisplayAccounts[0] : ServiceBase.EmptyAccountItem;
                }
                else
                {
                    Service?.Accounts.Remove(account);
                    DisplayAccounts.Remove(account);
                    if (DisplayAccounts.Count == 0)
                    {
                        SelectedAccount = ServiceBase.EmptyAccountItem;
                    }
                }
            }
        }

        public void AddAccount(ServiceItemBase account)
        {
            if (account != null)
            {
                if (!Service.Accounts.Contains(account))
                {
                    Service.Accounts.Add(account);
                    DisplayAccounts.Add(account);
                    SelectedAccount = account;
                }
            }
        }
    }

    public class RegistrySettingsRecord
    {
        public string DefaultServiceName = string.Empty;
        public EmailService DefaultAccountService = null;
    }

    public class EmailServicesData
    {
        public Dictionary<string, EmailService> EmailServicesDictionary = new Dictionary<string, EmailService>();
        public string DefaultServiceName = string.Empty;
        public EmailService DefaultAccountService = null;
    }

    #endregion Helper classes
}