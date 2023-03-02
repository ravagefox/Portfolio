using System.Net.Mail;

namespace Perfekt.Core.Net.SMTPService
{
    public interface IEmailManager
    {
        IEmailHostProvider Host { get; }
        string DisplayName { get; }

        SmtpClient GetClient();
        MailMessage BuildMessage(IEmailMessage msg);
        MailAddress GetSender();
        void SendMessage(MailMessage msg);
    }
}
