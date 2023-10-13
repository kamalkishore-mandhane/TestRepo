namespace SafeShare.WPFUI.Model.Services
{
    public class EmailItem : ServiceItemBase
    {
        public EmailItem(EmailService service, string senderName, string senderAddress, string username, string passwordToken, string oauth, string refreshToken)
            : base(senderAddress)
        {
            Service = service;
            SenderName = senderName;
            SenderAddress = senderAddress;
            UserName = username;
            PasswordToken = passwordToken;
            Oauth = oauth;
            RefreshToken = refreshToken;
        }

        public EmailService Service
        {
            get;
            private set;
        }

        public string SenderName
        {
            get;
            private set;
        }

        public string SenderAddress
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string PasswordToken
        {
            get;
            private set;
        }

        public string Oauth
        {
            get;
            private set;
        }

        public string RefreshToken
        {
            get;
            private set;
        }
    }
}