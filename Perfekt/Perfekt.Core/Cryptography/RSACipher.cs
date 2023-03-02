using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Perfekt.Core.Cryptography
{
    public sealed class RsaCipher
    {
        public byte[] PublicKey { get; set; }

        public byte[] PrivateKey { get; set; }



        private RSACryptoServiceProvider provider;
        private Encoding e;

        public RsaCipher()
        {
            this.provider = new RSACryptoServiceProvider(1024);

            e = Encoding.UTF8;
            this.PrivateKey = e.GetBytes(this.provider.ToXmlString(true));
            this.PublicKey = e.GetBytes(this.provider.ToXmlString(false));
        }

        public byte[] Encrypt(byte[] data)
        {
            this.provider.FromXmlString(e.GetString(this.PublicKey));
            return this.provider.Encrypt(data, true);
        }
        public byte[] Decrypt(byte[] data)
        {
            this.provider.FromXmlString(e.GetString(this.PrivateKey));
            return this.provider.Decrypt(data, true);
        }
    }
}
