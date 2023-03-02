using System.Net;

namespace Perfekt.Core.Net.SMTPService
{
    public interface IEmailHostProvider
    {
        string Name { get; }
        string SmtpHostAddress { get; }
        ushort SmtpPort { get; }

        NetworkCredential Credentials { get; }

        int SendLimit { get; }

        bool EnableSsl { get; }
    }
}
