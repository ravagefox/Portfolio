namespace Perfekt.Core
{

    public struct OAuth2Key
    {
        public string ClientSecret;
        public string ClientToken;

    }


    public sealed class Configuration
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Scheme { get; set; }

        public bool Enabled { get; set; }

        public string AssemblyPath { get; set; }

        public OAuth2Key Credentials { get; set; }


        public Configuration()
        {
            AssemblyPath = string.Empty;
            Enabled = true;

            Credentials = new OAuth2Key();
            Host = string.Empty;
            Scheme = string.Empty;
            Port = 0;
        }
    }
}
