using System.Net;

namespace Perfekt.Core.Net.SMTPService.Providers
{
    public sealed class GMailProvider : IEmailHostProvider
    {
        public string Name => nameof(GMailProvider);

        public string SmtpHostAddress => "smtp-relay.google.com";

        public ushort SmtpPort => 587;

        public bool EnableSsl => true;

        public int SendLimit => 2000;

        public NetworkCredential Credentials { get; }


        public GMailProvider(string useremail, string userpass)
        {
            this.Credentials = new NetworkCredential(useremail, userpass);
        }
    }
}
