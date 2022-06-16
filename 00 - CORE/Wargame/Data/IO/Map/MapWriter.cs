// Source: MapWriter
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Wargame.Data.IO.Map
{
    public sealed class MapWriter : IDisposable
    {
        public static readonly char[] MAGIC = new char[] { 'M', 'Z', 'X' };

        private BinaryWriter writer;


        public MapWriter(Stream outStream)
        {
            this.writer = new BinaryWriter(outStream, Encoding.UTF8);
            this.writer.Write(MAGIC);
        }

        public void Write(Guid id)
        {
            this.writer.Write(id.ToByteArray());
        }

        public void Write(float value)
        {
            this.writer.Write(value);
        }

        public void Write(bool value)
        {
            this.writer.Write(value ? (byte)0x1 : (byte)0x0);
        }

        public void Write(uint value) { this.writer.Write(value); }

        public void Write(int value) { this.writer.Write(value); }

        public void Write(Vector3 value)
        {
            this.writer.Write(value.X);
            this.writer.Write(value.Y);
            this.writer.Write(value.Z);
        }

        public void Write(Vector4 value)
        {
            this.writer.Write(value.X);
            this.writer.Write(value.Y);
            this.writer.Write(value.Z);
            this.writer.Write(value.W);
        }

        public void Write(Quaternion value)
        {
            this.Write(value.ToVector4());
        }

        public void Write(Color value)
        {
            this.Write(value.ToVector4());
        }

        public void Write(string value)
        {
            var len = value.Length > byte.MaxValue ? byte.MaxValue : value.Length;
            var chars = value.ToCharArray();
            if (len == byte.MaxValue)
            {
                Array.Resize(ref chars, byte.MaxValue);
            }

            this.writer.Write(len);
            this.writer.Write(chars);
        }

        public void Dispose()
        {
            this.writer?.Dispose();
        }
    }
}
