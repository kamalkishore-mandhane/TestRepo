using System;
using System.Windows;
using System.Windows.Media;

namespace SafeShare.WPFUI.Model.Services
{
    #region Email Service Base Class

    public class EmailService : ServiceBase
    {
        public EmailService(string displayName)
            : this(displayName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public EmailService(string displayName, string name, string login, string help, string encryption, string server, string port, string domains)
            : base(displayName)
        {
            Name = name;
            Login = login;
            Help = help;
            Encryption = encryption;
            Server = server;
            Port = port;
            Domains = domains;
        }

        public string Name
        {
            get;
            protected set;
        }

        public string Login
        {
            get;
            protected set;
        }

        public string Help
        {
            get;
            protected set;
        }

        public string Encryption
        {
            get;
            protected set;
        } = "None";

        public string Server
        {
            get;
            protected set;
        }

        public string Port
        {
            get;
            protected set;
        }

        public string Domains
        {
            get;
            protected set;
        }

        public virtual int DisplayOrder
        {
            get;
            protected set;
        } = 0;

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("CustomEmailServiceDrawingImage") as DrawingImage;
        }

        public static EmailService CreateServiceFromName(string name, string login, string help, string encryption, string server, string port, string domains)
        {
            if (string.Compare(name, OutlookService.XmlServiceName, true) == 0)
            {
                return new OutlookService(name, login, help, encryption, server, port, domains);
            }
            else if (string.Compare(name, GmailService.XmlServiceName, true) == 0)
            {
                return new GmailService(name, login, help, encryption, server, port, domains);
            }
            else if (string.Compare(name, YahooService.XmlServiceName, true) == 0)
            {
                return new YahooService(name, login, help, encryption, server, port, domains);
            }
            else if (string.Compare(name, HotmailService.XmlServiceName, true) == 0)
            {
                return new HotmailService(name, login, help, encryption, server, port, domains);
            }
            else
            {
                return new EmailService(name, name, login, help, encryption, server, port, domains);
            }
        }
    }

    #endregion

    #region Specific Email Service Classes

    [KnownEmailService]
    [OAuth2EmailService]
    public class OutlookService : EmailService
    {
        public OutlookService(string name, string login, string help, string encryption, string server, string port, string domains)
            : base("Microsoft Outlook", name, login, help, encryption, server, port, domains)
        {
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 1;

        public static string XmlServiceName => "Outlook.com";

        public static RecipientClient.Client Client
        {
            get;
            set;
        } = null;

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("OutlookDrawingImage") as DrawingImage;
        }
    }

    [KnownEmailService]
    [OAuth2EmailService]
    public class GmailService : EmailService
    {
        public GmailService(string name, string login, string help, string encryption, string server, string port, string domains)
            : base("Gmail", name, login, help, encryption, server, port, domains)
        {
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 2;

        public static string XmlServiceName => "Gmail";

        public static RecipientClient.Client Client
        {
            get;
            set;
        } = null;

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("GmailDrawingImage") as DrawingImage;
        }
    }

    [KnownEmailService]
    [OAuth2EmailService]
    public class YahooService : EmailService
    {
        public YahooService(string name, string login, string help, string encryption, string server, string port, string domains)
            : base("Yahoo!", name, login, help, encryption, server, port, domains)
        {
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 3;

        public static string XmlServiceName => "Yahoo!";

        public static RecipientClient.Client Client
        {
            get;
            set;
        } = null;

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("YahooDrawingImage") as DrawingImage;
        }
    }

    [KnownEmailService]
    public class HotmailService : EmailService
    {
        public HotmailService(string name, string login, string help, string encryption, string server, string port, string domains)
            : base("Hotmail", name, login, help, encryption, server, port, domains)
        {
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 4;

        public static string XmlServiceName => "Hotmail";

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("HotmailDrawingImage") as DrawingImage;
        }
    }

    public class CustomEmailService : EmailService
    {
        public CustomEmailService(string name, string login, string help, string encryption, string server, string port, string domains)
            : base(name, name, login, help, encryption, server, port, domains)
        {
            DisplayOrder = NextDisplayOrder + 1;
            NextDisplayOrder++;
        }

        public static int NextDisplayOrder
        {
            get;
            private set;
        } = 0;
    }

    #endregion

    #region Fake Email Service Classes

    [FakeEmailService]
    public class AdvancedSetup : EmailService
    {
        public AdvancedSetup()
            : base(Properties.Resources.TEXT_ADVANCED_SETUP)
        {
            SelectedAccount = new ServiceItemBase(Properties.Resources.TEXT_ADD_NEW_SERVICE);
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 5;

        protected override ImageSource LoadIcon()
        {
            return Application.Current.TryFindResource("AdvanceSetupDrawingImage") as DrawingImage;
        }
    }

    [FakeEmailService]
    public class UsersOwnProgram : EmailService
    {
        public UsersOwnProgram()
            : base(Properties.Resources.TEXT_USERS_OWN_PROGRAM)
        {
            SelectedAccount = new ServiceItemBase(Properties.Resources.TEXT_SYSTEM_SETTINGS);
        }

        public override int DisplayOrder => CustomEmailService.NextDisplayOrder + 6;
    }

    #endregion

    #region Helper Classes

    public class KnownEmailServiceAttribute : Attribute
    {
    }

    public class OAuth2EmailServiceAttribute : Attribute
    {
    }

    public class FakeEmailServiceAttribute : Attribute
    {
    }

    #endregion
}