using System.Net;
using System.Security;

namespace Perfekt.Core.Net.SMTPService.Providers
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class CustomMailProvider : IEmailHostProvider
    {
        public string Name { get; private set; }

        public string SmtpHostAddress { get; private set; }

        public ushort SmtpPort { get; private set; }

        public NetworkCredential Credentials { get; private set; }

        public int SendLimit { get; private set; }

        public bool EnableSsl { get; private set; }



        private CustomMailProvider()
        {
        }


        public static CustomMailProvider FromXml(Stream xmlStream)
        {
            CustomMailProvider m = new CustomMailProvider();
            xmlStream.Seek(0, SeekOrigin.Begin);

            using (var sr = new StreamReader(xmlStream))
            {
                var securityElement = SecurityElement.FromString(sr.ReadToEnd());
                if (securityElement == null)
                {
                    throw new InvalidDataException(
                        "The security element could not be parsed from the xml data stream.");
                }

                var name = securityElement.SearchForChildByTag(nameof(Name));
                var smtpHost = securityElement.SearchForChildByTag(nameof(SmtpHostAddress));
                var smtpPort = securityElement.SearchForChildByTag(nameof(SmtpPort));
                var credentials = securityElement.SearchForChildByTag(nameof(Credentials));
                var sendLimit = securityElement.SearchForChildByTag(nameof(SendLimit));
                var enableSsl = securityElement.SearchForChildByTag(nameof(EnableSsl));

                m = new CustomMailProvider()
                {
                    Name = GetValue(name, string.Empty),
                    SmtpHostAddress = GetValue(smtpHost, IPAddress.Loopback.ToString()),
                    SmtpPort = ushort.Parse(GetValue(smtpPort, "25")),
                    Credentials = GetNetCredentials(credentials),
                    SendLimit = int.Parse(GetValue(sendLimit, "200")),
                    EnableSsl = bool.Parse(GetValue(enableSsl, bool.FalseString)),
                };

                sr.Close();
            }

            return m;
        }

        private static NetworkCredential GetNetCredentials(SecurityElement? credentials)
        {
            if (credentials == null)
            {
                return new NetworkCredential(Environment.UserName, string.Empty);
            }

            var userName = GetValue(credentials.SearchForChildByTag("Username"), Environment.UserName);
            var password = GetValue(credentials.SearchForChildByTag("Password"), string.Empty);
            return new NetworkCredential(userName, password);
        }

        private static string GetValue(SecurityElement? name, string defaultValue)
        {
            if (name == null)
            {
                throw new NullReferenceException(nameof(name) + " cannot be null.");
            }

            if (!string.IsNullOrEmpty(name.Text)) { return name.Text; }

            return defaultValue;
        }
    }
}