namespace SafeShare.WPFUI.Model.Services
{
    internal class CloudItem : ServiceItemBase
    {
        public CloudItem(CloudService service, string authId, string nickname)
            : base(nickname)
        {
            Service = service;
            AuthId = authId;
            Nickname = nickname;
        }

        public CloudService Service
        {
            get;
            private set;
        }

        public string AuthId
        {
            get;
            private set;
        }

        public string Nickname
        {
            get;
            private set;
        }
    }
}