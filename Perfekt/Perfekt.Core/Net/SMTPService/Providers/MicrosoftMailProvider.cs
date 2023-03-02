using System.Net;

namespace Perfekt.Core.Net.SMTPService.Providers
{
    public sealed class MicrosoftMailProvider : IEmailHostProvider
    {
        public string Name => nameof(MicrosoftMailProvider);

        public string SmtpHostAddress => "smtp-mail.outlook.com";

        public ushort SmtpPort => 587;

        public NetworkCredential Credentials { get; }

        public bool EnableSsl => true;

        public int SendLimit => 200;


        public MicrosoftMailProvider(string useremail, string userpass)
        {
            Credentials = new NetworkCredential(useremail, userpass);
        }
    }
}
