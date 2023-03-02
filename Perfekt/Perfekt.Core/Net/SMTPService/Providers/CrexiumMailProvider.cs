using System.Net;

namespace Perfekt.Core.Net.SMTPService.Providers
{
    public sealed class CrexiumMailProvider : IEmailHostProvider
    {
        public string Name => nameof(CrexiumMailProvider);

        public string SmtpHostAddress => "smtp.crexium.com";

        public ushort SmtpPort => 587;

        public NetworkCredential Credentials { get; }

        public bool EnableSsl => true;

        public int SendLimit => 200;


        public CrexiumMailProvider(string useremail, string userpass)
        {
            Credentials = new NetworkCredential(useremail, userpass);
        }
    }
}
