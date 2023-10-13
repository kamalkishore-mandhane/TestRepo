using SafeShare.WPFUI.Utils;
using System;
using System.Windows;
using System.Windows.Media;

namespace SafeShare.WPFUI.Model.Services
{
    #region Cloud Service Base Class

    public class CloudService : ServiceBase
    {
        public CloudService(string serviceName)
            : base(serviceName)
        {
        }

        public WzSvcProviderIDs Spid
        {
            get;
            protected set;
        } = WzSvcProviderIDs.SPID_UNKNOWN;

        public string RegistryPath
        {
            get;
            protected set;
        } = string.Empty;

        public static CloudService CreateServiceFromSpid(WzSvcProviderIDs spid)
        {
            switch (spid)
            {
                case WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD:
                    return new AlibabaCloudService();

                case WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3:
                    return new AmazonS3Service();

                case WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB:
                    return new AzureBlobService();

                case WzSvcProviderIDs.SPID_CLOUD_BOX:
                    return new BoxService();

                case WzSvcProviderIDs.SPID_CLOUD_CLOUDME:
                    return new CloudMeService();

                case WzSvcProviderIDs.SPID_CLOUD_DROPBOX:
                    return new DropBoxService();

                case WzSvcProviderIDs.SPID_CLOUD_FTP:
                    return new FTPService();

                case WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD:
                    return new GoogleCloudService();

                case WzSvcProviderIDs.SPID_CLOUD_GOOGLE:
                    return new GoogleDriveService();

                case WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD:
                    return new IBMCloudService();

                case WzSvcProviderIDs.SPID_CLOUD_IONOS:
                    return new IONOSService();

                case WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE:
                    return new MediaFireService();

                case WzSvcProviderIDs.SPID_CLOUD_NASCLOUD:
                    return new NASCloudService();

                case WzSvcProviderIDs.SPID_CLOUD_OFFICE365:
                    return new Office365Service();

                case WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE:
                    return new OneDriveService();

                case WzSvcProviderIDs.SPID_CLOUD_OPENSTACK:
                    return new OpenstackService();

                case WzSvcProviderIDs.SPID_CLOUD_OVH:
                    return new OVHService();

                case WzSvcProviderIDs.SPID_CLOUD_RACKSPACE:
                    return new RackspaceService();

                case WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE:
                    return new S3CompatibleService();

                case WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT:
                    return new SharePointService();

                case WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC:
                    return new SugarSyncService();

                case WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK:
                    return new SwiftStackService();

                case WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT:
                    return new TeamsSharePointService();

                case WzSvcProviderIDs.SPID_CLOUD_WASABI:
                    return new WasabiService();

                case WzSvcProviderIDs.SPID_CLOUD_WEBDAV:
                    return new WebDAVService();

                case WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE:
                    return new ZipShareService();

                case WzSvcProviderIDs.SPID_UNKNOWN:
                default:
                    return new CloudService(string.Empty);
            }
        }
    }

    #endregion

    #region Specific Cloud Service Classes

    [NoEmailAddress]
    public class AlibabaCloudService : CloudService
    {
        public AlibabaCloudService()
            : base("Alibaba Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_ALIBABACLOUD;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFalibabacloud";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("AlibabaDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class AmazonS3Service : CloudService
    {
        public AmazonS3Service()
            : base("Amazon S3")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_AMAZON_S3;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFas3";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("AmazonS3DrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class AzureBlobService : CloudService
    {
        public AzureBlobService()
            : base("Azure Blob")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_AZUREBLOB;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFazureblob";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("AzureDrawingImage") as DrawingImage;
        }
    }

    public class BoxService : CloudService
    {
        public BoxService()
            : base("Box")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_BOX;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFbox";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("BoxDrawingImage") as DrawingImage;
        }
    }

    public class CloudMeService : CloudService
    {
        public CloudMeService()
            : base("CloudMe")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_CLOUDME;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFcldme";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("CloudMeDrawingImage") as DrawingImage;
        }
    }

    public class DropBoxService : CloudService
    {
        public DropBoxService()
            : base("Dropbox")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_DROPBOX;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFdbox";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("DropBoxDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class FTPService : CloudService
    {
        public FTPService()
            : base("FTP")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_FTP;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFFTP";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("FTPDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class GoogleCloudService : CloudService
    {
        public GoogleCloudService()
            : base("Google Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_GOOGLECLOUD;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFgcloud";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("GoogleCloudDrawingImage") as DrawingImage;
        }
    }

    public class GoogleDriveService : CloudService
    {
        public GoogleDriveService()
            : base("Google Drive")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_GOOGLE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFgdrv";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("GoogleDriveDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class IBMCloudService : CloudService
    {
        public IBMCloudService()
            : base("IBM Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_IBMCLOUD;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFibmcloud";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("IBMDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class IONOSService : CloudService
    {
        public IONOSService()
            : base("IONOS")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_IONOS;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFionos";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("IONOSDrawingImage") as DrawingImage;
        }
    }

    public class MediaFireService : CloudService
    {
        public MediaFireService()
            : base("MediaFire")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_MEDIAFIRE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFmfire";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("MediaFireDrawingImage") as DrawingImage;
        }
    }

    public class NASCloudService : CloudService
    {
        public NASCloudService()
            : base("NAS Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_NASCLOUD;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFnas";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("NASCloudDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class Office365Service : CloudService
    {
        public Office365Service()
            : base("Office365 Business")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_OFFICE365;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFofficespt";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("Office365DrawingImage") as DrawingImage;
        }
    }

    public class OneDriveService : CloudService
    {
        public OneDriveService()
            : base("OneDrive")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_ONEDRIVE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFoned";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("OneDriveDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class OpenstackService : CloudService
    {
        public OpenstackService()
            : base("Openstack Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_OPENSTACK;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFopenstack";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("OpenStackDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class OVHService : CloudService
    {
        public OVHService()
            : base("OVH Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_OVH;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFOVH";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("OVHDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class RackspaceService : CloudService
    {
        public RackspaceService()
            : base("Rackspace Cloud")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_RACKSPACE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFrackspace";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("RackSpaceDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class S3CompatibleService : CloudService
    {
        public S3CompatibleService()
            : base("S3Compatible")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFs3compatible";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("S3CompatibleDrawingImage") as DrawingImage;
        }
    }

    public class SharePointService : CloudService
    {
        public SharePointService()
            : base("SharePoint")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_SHAREPOINT;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFspt";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("SharePointDrawingImage") as DrawingImage;
        }
    }

    public class SugarSyncService : CloudService
    {
        public SugarSyncService()
            : base("SugarSync")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_SUGARSYNC;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFssync";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("SugarSyncDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class SwiftStackService : CloudService
    {
        public SwiftStackService()
            : base("SwiftStack")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_SWIFTSTACK;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFswift";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("SwiftStackDrawingImage") as DrawingImage;
        }
    }

    public class TeamsSharePointService : CloudService
    {
        public TeamsSharePointService()
            : base("Teams SharePoint")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_TEAMSSHAREPOINT;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFteamsspt";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("TeamsSharePointDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class WasabiService : CloudService
    {
        public WasabiService()
            : base("Wasabi")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_WASABI;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFwasabi";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("WasabiDrawingImage") as DrawingImage;
        }
    }

    [NoEmailAddress]
    public class WebDAVService : CloudService
    {
        public WebDAVService()
            : base("WebDAV")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_WEBDAV;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFWebDAV";
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("WebDAVDrawingImage") as DrawingImage;
        }
    }

    public class ZipShareService : CloudService
    {
        public class ZipShareEmptyAccount : EmptyAccount
        {
            public ZipShareEmptyAccount()
            {
                DisplayName = Properties.Resources.TEXT_ZIPSHARE_EMPTY_ACCOUNT;
            }
        }

        private ServiceItemBase _zipShareEmptyAccount = new ZipShareEmptyAccount();

        public ZipShareService()
            : base("ZipShare Cloud(Free)")
        {
            Spid = WzSvcProviderIDs.SPID_CLOUD_ZIPSHARE;
            RegistryPath = @"SOFTWARE\WinZip Computing\WinZip\WXF\WzWXFzshare";
            SelectedAccount = _zipShareEmptyAccount;
        }

        public override ServiceItemBase SelectedAccount
        {
            get => base.SelectedAccount;
            set => base.SelectedAccount = value == EmptyAccountItem ? _zipShareEmptyAccount : value;
        }

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("ZipShareDrawingImage") as DrawingImage;
        }
    }

    #endregion

    #region Helper Classes

    public class NoEmailAddressAttribute : Attribute
    {
    }

    #endregion
}