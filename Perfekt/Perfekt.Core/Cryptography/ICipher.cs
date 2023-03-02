using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfekt.Core.Cryptography
{
    public interface ICipher
    {
        byte[] PublicKey { get; }
        byte[] PrivateKey { get; }

        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
