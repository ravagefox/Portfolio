using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Perfekt.Core.Cryptography
{
#pragma warning disable // Disables all warning contained in the current scope

    public class AesCipher : IDisposable
    {
        public byte[] Data { get; set; }

        public byte[] Key { get; }

        public Encoding Encoding { get; set; }


        private static readonly string hash = "SHA256";
        private static readonly string salt = "aselrias38490a32";
        private static readonly string vector = "8947az34awl34kjq";
        private static readonly int keysize = 256;


        public event EventHandler EncryptionComplete;
        public event EventHandler DecryptionComplete;

        private Aes cipher;
        private ICryptoTransform encryptor, decryptor;
        private byte[] keybytes;
        private byte[] vectorbytes;

        public AesCipher(byte[] symmetricKey)
        {
            this.Key = symmetricKey;
            this.cipher = Aes.Create();

            this.cipher.Mode = CipherMode.CBC;
            this.cipher.Padding = PaddingMode.PKCS7;
            this.Encoding = Encoding.UTF8;


            var saltbytes = this.Encoding.GetBytes(salt);
            var keystring = this.Encoding.GetString(this.Key);
            this.vectorbytes = this.Encoding.GetBytes(vector);

            var passwordBytes =
                new PasswordDeriveBytes(keystring, saltbytes, hash, iterations: 2);
            this.keybytes = passwordBytes.GetBytes(keysize / 8);
        }

        public byte[] Encrypt(byte[] inData)
        {
            this.Data = inData;
            this.Encrypt();

            return this.Data;
        }

        public void Encrypt()
        {
            if (this.Data == null)
            {
                throw new NullReferenceException(
                    "Data cannot be null.");
            }

            if (this.encryptor == null)
            {
                this.encryptor = this.CreateEncryptor();
            }

            using (var to = new MemoryStream())
            {
                using (var w = new CryptoStream(to, this.encryptor, CryptoStreamMode.Write))
                {
                    w.Write(this.Data, 0, this.Data.Length);
                    w.FlushFinalBlock();

                    this.Data = to.ToArray();
                }
            }
        }

        public async Task EncryptAsync()
        {
            if (this.Data == null)
            {
                throw new NullReferenceException(
                    "Data cannot be null.");
            }

            if (this.encryptor == null)
            {
                this.encryptor = this.CreateEncryptor();
            }

            using (var to = new MemoryStream())
            {
                using (var w = new CryptoStream(to, this.encryptor, CryptoStreamMode.Write))
                {
                    await w.WriteAsync(this.Data, 0, this.Data.Length);
                    w.FlushFinalBlock();

                    this.Data = new byte[to.Length];
                    _ = await to.ReadAsync(this.Data, 0, this.Data.Length);
                }
            }
        }

        public byte[] Decrypt(byte[] inData)
        {
            this.Data = inData;
            this.Decrypt();

            return this.Data;
        }

        public void Decrypt()
        {
            if (this.Data == null)
            {
                throw new NullReferenceException(
                    "Data cannot be null.");
            }

            if (this.decryptor == null)
            {
                this.decryptor = this.CreateDecryptor();
            }

            using (var from = new MemoryStream(this.Data))
            {
                using (var reader = new CryptoStream(from, this.decryptor, CryptoStreamMode.Read))
                {
                    this.Data = new byte[from.Length];
                    var size = reader.Read(this.Data, 0, this.Data.Length);
                }
            }
        }

        public async Task DecryptAsync()
        {
            if (this.Data == null)
            {
                throw new NullReferenceException(
                    "Data cannot be null.");
            }

            if (this.decryptor == null)
            {
                this.decryptor = this.CreateDecryptor();
            }


            using (var from = new MemoryStream(this.Data))
            {
                using (var reader = new CryptoStream(from, this.decryptor, CryptoStreamMode.Read))
                {
                    this.Data = new byte[from.Length];
                    _ = await reader.ReadAsync(this.Data, 0, this.Data.Length);
                }
            }
        }



        public void BeginEncrypt()
        {
            var action = new Action(this.Encrypt);
            _ = action.BeginInvoke(this.EndEncrypt, action);
        }

        private void EndEncrypt(IAsyncResult ar)
        {
            var action = ar.AsyncState as Action;
            action.EndInvoke(ar);

            EncryptionComplete?.Invoke(this, EventArgs.Empty);
        }


        public void BeginDecrypt()
        {
            var action = new Action(this.Decrypt);
            _ = action.BeginInvoke(this.EndDecrypt, action);
        }

        private void EndDecrypt(IAsyncResult ar)
        {
            var action = ar.AsyncState as Action;
            action.EndInvoke(ar);

            DecryptionComplete?.Invoke(this, EventArgs.Empty);
        }


        public void Dispose()
        {
            this.encryptor.Dispose();
            this.decryptor.Dispose();
            this.cipher.Dispose();
        }

        private ICryptoTransform CreateEncryptor()
        {
            return this.cipher.CreateEncryptor(this.keybytes, this.vectorbytes);
        }

        private ICryptoTransform CreateDecryptor()
        {
            return this.cipher.CreateDecryptor(this.keybytes, this.vectorbytes);
        }
    }
}
