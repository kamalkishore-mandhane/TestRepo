using SafeShare.Properties;
using SafeShare.Util;
using SafeShare.WPFUI.Model.Services;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SafeShare.WPFUI
{
    internal class WorkFlowManager
    {
        private enum WorkFlow
        {
            None,
            SetShareOption,
            ManageEmail,
            ManageCloud,
            EncryptOption,
            Conversion
        }

        private class WorkState
        {
            public Page PreviousPage = null;
            public Page CurrentPage = null;
            public WorkFlow WorkFlow = WorkFlow.None;

            public WorkState(Page previousPage, Page currentPage, WorkFlow workFlow)
            {
                PreviousPage = previousPage;
                CurrentPage = currentPage;
                WorkFlow = workFlow;
            }
        }

        private static EmailEncryptionPage _emailEncryptionPage;
        private static ManageEmailPage _manageEmailPage;
        private static ManageCloudPage _manageCloudPage;
        private static EncryptOptPage _encryptOptPage;
        private static ConversionPage _conversionPage;

        private static Stack<WorkState> _states = new Stack<WorkState>(10);
        private static bool _firstOpen = true;

        static WorkFlowManager()
        {
            ShareOptionData = new ShareOptionData();
            Initializer = new Initializer();
        }

        public static bool IsAppFirstOpen()
        {
            var firstOpenStatus = _firstOpen;
            _firstOpen = false;
            return firstOpenStatus;
        }

        private static WorkState CurrentState => _states.Count == 0 ? null : _states.Peek();

        public static ShareOptionData ShareOptionData
        {
            get;
            private set;
        }

        public static Initializer Initializer
        {
            get;
            private set;
        }

        public static void Clear()
        {
            _states.Clear();
            ShareOptionData.ItemsPath = null;
            ShareOptionData.ContainsProtectedFile = null;

            // clear temporary page and properties
            _encryptOptPage = null;
            if (_emailEncryptionPage != null && _emailEncryptionPage.DataContext is EmailEncryptionPageViewModel eepViewModel)
            {
                eepViewModel.SuggestedPassword = string.Empty;
                eepViewModel.IsCustomFileName = false;
            }

            _conversionPage = null;
        }

        public static void GoBack()
        {
            CurrentState.CurrentPage.NavigationService?.Navigate(CurrentState.PreviousPage);
            _states.Pop();
        }

        #region Main Process

        public static void GoEmailEncryptionPage(FileListPage fileListPage)
        {
            if (_emailEncryptionPage == null)
            {
                _emailEncryptionPage = new EmailEncryptionPage(fileListPage);
                _emailEncryptionPage.InitDataContext();
            }
            _emailEncryptionPage.UpdateDataContext(fileListPage);
            fileListPage.NavigationService?.Navigate(_emailEncryptionPage);

            _states.Push(new WorkState(fileListPage, _emailEncryptionPage, WorkFlow.SetShareOption));
        }

        public static void UpdateEmailEncryptionPageData(FileListPage fileListPage)
        {
            _emailEncryptionPage.UpdateDataContext(fileListPage);
        }

        public static void GoFrontPage(Page page)
        {
            var frontPage = new FrontPage();
            page.NavigationService?.Navigate(frontPage);
        }

        public static void GoConversionPage(FileListPage fileListPage, EmailEncryptionPage emailEncryptionPage)
        {
            if (_conversionPage == null)
            {
                _conversionPage = new ConversionPage(fileListPage, emailEncryptionPage);
                _conversionPage.InitDataContext();
            }
            _conversionPage.UpdatePage(fileListPage, emailEncryptionPage);
            emailEncryptionPage.NavigationService?.Navigate(_conversionPage);

            _states.Push(new WorkState(emailEncryptionPage, _conversionPage, WorkFlow.Conversion));
        }

        #endregion Main Process

        #region Manage Email SubProcess

        public static void StartManageEmail(Page callerPage)
        {
            if (_manageEmailPage == null)
            {
                _manageEmailPage = new ManageEmailPage();
            }

            callerPage.NavigationService?.Navigate(_manageEmailPage);

            _states.Push(new WorkState(callerPage, _manageEmailPage, WorkFlow.ManageEmail));
        }

        public static void DoneManageEmail()
        {
            if (CurrentState.WorkFlow == WorkFlow.ManageEmail)
            {
                _manageEmailPage.NavigationService?.Navigate(CurrentState.PreviousPage);
                _states.Pop();
            }
        }

        #endregion Manage Email SubProcess

        #region Manage Cloud SubProcess

        public static void StartManageCloud(Page callerPage)
        {
            if (_manageCloudPage == null)
            {
                _manageCloudPage = new ManageCloudPage();
            }

            var viewModel = _manageCloudPage.DataContext as ManageCloudPageViewModel;
            if (viewModel.HandleCloudServiceDiabled())
            {
                return;
            }

            callerPage.NavigationService?.Navigate(_manageCloudPage);

            _states.Push(new WorkState(callerPage, _manageCloudPage, WorkFlow.ManageCloud));
        }

        public static void DoneManageCloud()
        {
            if (CurrentState.WorkFlow == WorkFlow.ManageCloud)
            {
                _manageCloudPage.NavigationService?.Navigate(CurrentState.PreviousPage);
                _states.Pop();
            }
        }

        #endregion Manage Cloud SubProcess

        #region Encrypt Option SubProcess

        public static void StartEncryptOption(Page callerPage)
        {
            if (_encryptOptPage == null)
            {
                _encryptOptPage = new EncryptOptPage(callerPage.DataContext as EmailEncryptionPageViewModel);
            }

            callerPage.NavigationService?.Navigate(_encryptOptPage);

            _states.Push(new WorkState(callerPage, _encryptOptPage, WorkFlow.EncryptOption));
        }

        #endregion Encrypt Option SubProcess
    }

    public class ShareOptionData
    {
        public bool UseWinZipEmailer = false;

        #region Email Encryption Page Data

        public bool UseEmailAttachment = false;

        public bool UseEmailLink = false;

        public bool EncryptFile = false;

        public EmailService SelectedEmail = null;

        public CloudService SelectedCloud = null;

        #endregion Email Encryption Page Data

        #region Schedule For Deletion Data

        public bool IsScheduledForDeletion = false;

        public int ExpirationDays = 30;

        public bool IsDeleteEmptyFolder = false;

        #endregion Schedule For Deletion Data

        #region EDP options

        // There's no EDP support in WXF transformers and third party assemblies.
        // This field is used to decide whether need to unprotect files if process is protected by EDP
        //
        // null     : process is not protected by EDP.
        // true     : process is protected by EDP, and contains protected file before convert
        // false    : process is protected by EDP, but no file is protected
        public bool? ContainsProtectedFile = null;

        #endregion

        public List<string> ItemsPath = null;
    }

    public class Initializer
    {
        public Task ShareOptionDataInitializationTask
        {
            get;
            private set;
        } = Task.CompletedTask;

        public Task EmailServiceInitializationTask
        {
            get;
            private set;
        } = Task.CompletedTask;

        public Task CloudServiceInitializationTask
        {
            get;
            private set;
        } = Task.CompletedTask;

        public void StartInit()
        {
            EmailServiceInitializationTask = Task.Run(() => InitEmailServicesData(ManageEmailPageViewModel.EmailServicesData));
            CloudServiceInitializationTask = Task.Run(() => InitCloudServicesData(ref ManageCloudPageViewModel.CloudServicesDictionary));
            ShareOptionDataInitializationTask = Task.Run(() => InitShareOptionData(WorkFlowManager.ShareOptionData));
        }

        private async Task InitShareOptionData(ShareOptionData shareOptionData)
        {
            // Init Schedule Deletion Data
            WorkFlowManager.ShareOptionData.IsScheduledForDeletion = Settings.Default.DeleteFileChecked;
            WorkFlowManager.ShareOptionData.ExpirationDays = Settings.Default.DeleteFileDays;

            // Init Email Encryption Page Data
            WorkFlowManager.ShareOptionData.UseEmailAttachment = Settings.Default.EmailAttachmentIsChecked;
            WorkFlowManager.ShareOptionData.UseEmailLink = Settings.Default.EmailLinkIsChecked;
            WorkFlowManager.ShareOptionData.EncryptFile = Settings.Default.EncryptFile;

            // Detect if use WinZip's emailer
            shareOptionData.UseWinZipEmailer = !(RegeditOperation.GetLocalMachineRegistryStringValue(RegeditOperation.WzKey, RegeditOperation.WzNoInternalEmailerKey) == "1"
                                                || RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzKey, RegeditOperation.WzUseMapiKey) == "1");

            // Initialize email cloud service from registry
            if (shareOptionData.UseWinZipEmailer)
            {
                // Read default email settings if "Use WinZip's emailer" is checked
                string defaultEmailAccount = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultAccountKey);
                string defaultEmailService = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultEmailService);
                if (!string.IsNullOrEmpty(defaultEmailAccount) && !string.IsNullOrEmpty(defaultEmailService))
                {
                    await EmailServiceInitializationTask;
                    if (ManageEmailPageViewModel.EmailServicesData.EmailServicesDictionary.TryGetValue(defaultEmailService, out EmailService emailService))
                    {
                        foreach (var account in emailService.Accounts)
                        {
                            if (string.Compare((account as EmailItem).SenderAddress, defaultEmailAccount, true) == 0)
                            {
                                emailService.SelectedAccount = account;
                                shareOptionData.SelectedEmail = emailService;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Set selected email to a fake UsersOwnProgram email service if "Use my email program" is checked.
                shareOptionData.SelectedEmail = new UsersOwnProgram();
            }

            // Initialize default cloud service from registry
            string defaultCloudSpid = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudSpidKey);
            string defaultCloudAuthId = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudAuthIdKey);
            string defaultCloudNickName = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzSafeShareSubKey, RegeditOperation.WzDefaultCloudNickNameKey);
            if (!string.IsNullOrEmpty(defaultCloudSpid) && !string.IsNullOrEmpty(defaultCloudAuthId))
            {
                string[] parts = defaultCloudAuthId.Split('|');
                if (parts.Length == 2)
                {
                    await CloudServiceInitializationTask;
                    if (ManageCloudPageViewModel.CloudServicesDictionary.TryGetValue((WzSvcProviderIDs)int.Parse(defaultCloudSpid), out CloudService cloudService))
                    {
                        if (WinzipMethods.IsCloudAuthorized(IntPtr.Zero, cloudService.Spid, defaultCloudAuthId))
                        {
                            foreach (var account in cloudService.Accounts)
                            {
                                if (string.Compare((account as CloudItem).AuthId, parts[1], true) == 0)
                                {
                                    cloudService.SelectedAccount = account;
                                    shareOptionData.SelectedCloud = cloudService;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitEmailServicesData(EmailServicesData data)
        {
            // Load email services
            string servicesXmlString = RegeditOperation.GetLocalMachineRegistryStringValue(RegeditOperation.WzEmailServicesKey, null);
            string hkcuXmlString = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzEmailServicesKey, null);
            if (!string.IsNullOrEmpty(hkcuXmlString))
            {
                servicesXmlString = hkcuXmlString;
            }

            var content = XElement.Parse(servicesXmlString);
            data.DefaultServiceName = (string)content.Attribute("default");

            var servicesNodes = content.Elements("mailservice");
            foreach (var serverNode in servicesNodes)
            {
                string name = (string)serverNode.Attribute("name") ?? string.Empty;
                string login = (string)serverNode.Attribute("login") ?? string.Empty;
                string help = (string)serverNode.Attribute("help") ?? string.Empty;
                string encryption = (string)serverNode.Attribute("encryption") ?? string.Empty;
                var smtpNode = serverNode.Element("smtp");
                string server = (string)smtpNode.Attribute("server") ?? string.Empty;
                string port = (string)smtpNode.Attribute("port") ?? string.Empty;
                string domains = serverNode.Element("domains").Value ?? string.Empty;

                if (string.Compare(name, OutlookService.XmlServiceName, true) == 0)
                {
                    data.EmailServicesDictionary.Add(name, new OutlookService(name, login, help, encryption, server, port, domains));
                }
                else if (string.Compare(name, GmailService.XmlServiceName, true) == 0)
                {
                    data.EmailServicesDictionary.Add(name, new GmailService(name, login, help, encryption, server, port, domains));
                }
                else if (string.Compare(name, YahooService.XmlServiceName, true) == 0)
                {
                    data.EmailServicesDictionary.Add(name, new YahooService(name, login, help, encryption, server, port, domains));
                }
                else if (string.Compare(name, HotmailService.XmlServiceName, true) == 0)
                {
                    data.EmailServicesDictionary.Add(name, new HotmailService(name, login, help, encryption, server, port, domains));
                }
                else
                {
                    var customService = new CustomEmailService(name, login, help, encryption, server, port, domains);
                    data.EmailServicesDictionary.Add(name, customService);
                }
            }

            // Load email accounts
            string defaultAccountName = RegeditOperation.GetCurrentUserRegistryStringValue(RegeditOperation.WzEmailAccountsKey, null);
            string[] subKeyNames = RegeditOperation.GetCurrentUserRegistrySubKeyNames(RegeditOperation.WzEmailAccountsKey);
            foreach (string subKeyName in subKeyNames)
            {
                string subKey = RegeditOperation.WzEmailAccountsKey + "\\" + subKeyName;
                string serviceName = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "Service");
                string senderName = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "SenderName");
                string senderAddress = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "SenderAddress");
                string username = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "User");
                string oauth = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "OAuth");
                string passwordToken = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "WinInfo");
                string refreshToken = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "T1");
                if (!string.IsNullOrEmpty(passwordToken))
                {
                    WinzipMethods.UnprotectData(IntPtr.Zero, ref passwordToken);
                }

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    WinzipMethods.UnprotectData(IntPtr.Zero, ref refreshToken);
                }

                if (data.EmailServicesDictionary.ContainsKey(serviceName))
                {
                    var service = data.EmailServicesDictionary[serviceName];

                    if (service != null)
                    {
                        var emailItem = new EmailItem(service, senderName, senderAddress, username, passwordToken, oauth, refreshToken);
                        service.Accounts.Add(emailItem);

                        if (defaultAccountName == senderAddress)
                        {
                            data.DefaultAccountService = service;
                            data.DefaultAccountService.SelectedAccount = emailItem;
                        }
                    }
                }
            }
        }

        private void InitCloudServicesData(ref Dictionary<WzSvcProviderIDs, CloudService> data)
        {
            // FTP and WebDAV don't support share link
            data = new Dictionary<WzSvcProviderIDs, CloudService>
            {
                {WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE, new ZipShareService() },
                {WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE, new OneDriveService() },
                {WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT, new SharePointService() },
                {WzSvcProviderIDs.SPID_CLOUD_GOOGLE, new GoogleDriveService() },
                {WzSvcProviderIDs.SPID_CLOUD_BOX, new BoxService() },
                {WzSvcProviderIDs.SPID_CLOUD_DROPBOX, new DropBoxService() },
                {WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB, new AzureBlobService() },
                {WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3, new AmazonS3Service() },
                {WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE, new S3CompatibleService() },
                {WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD, new GoogleCloudService() },
                {WzSvcProviderIDs.SPID_CLOUD_OFFICE365, new Office365Service() },
                {WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT, new TeamsSharePointService() },
                {WzSvcProviderIDs.SPID_CLOUD_RACKSPACE, new RackspaceService() },
                {WzSvcProviderIDs.SPID_CLOUD_OPENSTACK, new OpenstackService() },
                {WzSvcProviderIDs.SPID_CLOUD_NASCLOUD, new NASCloudService() },
                {WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD, new IBMCloudService() },
                {WzSvcProviderIDs.SPID_CLOUD_CLOUDME, new CloudMeService() },
                {WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD, new AlibabaCloudService() },
                {WzSvcProviderIDs.SPID_CLOUD_IONOS, new IONOSService() },
                {WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE, new MediaFireService() },
                {WzSvcProviderIDs.SPID_CLOUD_OVH, new OVHService() },
                {WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC, new SugarSyncService() },
                {WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK, new SwiftStackService() },
                {WzSvcProviderIDs.SPID_CLOUD_WASABI, new WasabiService() },
            };

            // Check for unsupported services
            var unsupportedServices = new Collection<CloudService>();

            foreach (var service in data.Values)
            {
                if (!WinzipMethods.IsProviderSupport(IntPtr.Zero, service.Spid))
                {
                    unsupportedServices.Add(service);
                    continue;
                }
            }

            // Remove unsupported services
            foreach (var service in unsupportedServices)
            {
                data.Remove(service.Spid);
            }

            // Load cloud accounts
            foreach (var service in data.Values)
            {
                string authSubKey = service.RegistryPath + "\\Auth";
                var authGuids = RegeditOperation.GetCurrentUserRegistrySubKeyNames(authSubKey);

                if (authGuids != null)
                {
                    foreach (var authGuid in authGuids)
                    {
                        string subKey = authSubKey + "\\" + authGuid;
                        string authId = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "AuthID");
                        string nickname = RegeditOperation.GetCurrentUserRegistryStringValue(subKey, "NickName");

                        if (WinzipMethods.IsCloudAuthorized(IntPtr.Zero, service.Spid, service.Spid.ToString("d") + "|" + authId))
                        {
                            service.Accounts.Add(new CloudItem(service, authId, nickname));
                        }
                    }
                }
            }
        }
    }
}