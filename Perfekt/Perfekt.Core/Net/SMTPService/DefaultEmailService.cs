using System.Net.Mail;
using System.Net.Mime;

namespace Perfekt.Core.Net.SMTPService
{

    public sealed class DefaultEmailService : IEmailManager
    {
        public IEmailHostProvider Host { get; }
        public string DisplayName { get; set; }
        public string SenderAddress { get; }


        private SmtpClient smtpClient;
        private List<MemoryStream> attachmentStreams;


        public DefaultEmailService(string senderAddress, IEmailHostProvider hostProvider)
        {
            this.smtpClient = new SmtpClient();
            this.Host = hostProvider;
            this.DisplayName = nameof(DefaultEmailService);
            this.attachmentStreams = new List<MemoryStream>();
            this.SenderAddress = senderAddress;
        }


        public SmtpClient GetClient()
        {
            if (this.smtpClient != null)
            {
                this.smtpClient?.Dispose();
            }

            this.smtpClient = new SmtpClient(this.Host.SmtpHostAddress, this.Host.SmtpPort);
            return this.smtpClient;
        }

        public MailMessage BuildMessage(IEmailMessage message)
        {
            var msg = new MailMessage(message.Sender, message.Receipient)
            {
                Subject = message.Subject,
                Body = message.Body,
            };

            this.CreateAttachmentStreams(msg, message);

            return msg;
        }

        private void CreateAttachmentStreams(MailMessage msg, IEmailMessage message)
        {
            this.DisposeStreams();

            foreach (var attachment in message.Attachments)
            {
                if (attachment.Data != null &&
                    attachment.Data.Length > 0)
                {
                    var stream = new MemoryStream(attachment.Data);
                    this.attachmentStreams.Add(stream);

                    var mailAttachment = new Attachment(stream, attachment.Name)
                    {
                        ContentType = new ContentType(attachment.ContentType),
                    };
                    msg.Attachments.Add(mailAttachment);
                }
            }
        }

        private void DisposeStreams()
        {
            this.attachmentStreams.ForEach(stream => stream.Close());
            this.attachmentStreams.Clear();
        }

        public MailAddress GetSender()
        {
            return new MailAddress(this.SenderAddress, this.DisplayName);
        }

        public void SendMessage(MailMessage msg)
        {
            Task.Run(async () =>
            {
                var client = this.GetClient();

                client.EnableSsl = this.Host.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = this.Host.Credentials;

                await client.SendMailAsync(msg);

                this.DisposeStreams();
            });
        }
    }
}
