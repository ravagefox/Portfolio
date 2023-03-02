using System.Net.Mail;

namespace Perfekt.Core.Net.SMTPService
{
    public interface IEmailMessage
    {
        string Subject { get; }

        MailAddress Sender { get; }
        MailAddress Receipient { get; }

        string Body { get; }
        void SetBody(string body);

        IEnumerable<IAttachmentDetail> Attachments { get; }
    }
}
