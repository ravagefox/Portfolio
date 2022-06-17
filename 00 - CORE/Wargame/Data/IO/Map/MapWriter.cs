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
using System.Linq;
using System.Text;
using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Wargame.Data.Gos.Components;

namespace Wargame.Data.IO.Map
{
    public sealed class MapWriter : IDisposable
    {
        public static readonly char[] MAGIC = new char[] { 'M', 'Z', 'X' };


        private BinaryWriter writer;


        public MapWriter(Stream innerStream, Encoding encoding)
        {
            this.WriteEncoding(innerStream, encoding);

            this.writer = new BinaryWriter(innerStream, encoding);
            this.writer.Write(MAGIC);
        }

        private void WriteEncoding(Stream innerStream, Encoding encoding)
        {
            var bytes = new byte[1];
            if (encoding == Encoding.UTF8)
            {
                bytes = new byte[1] { 0x00 };
            }
            if (encoding == Encoding.Unicode)
            {
                bytes = new byte[1] { 0x01 };
            }
            if (encoding == Encoding.UTF8)
            {
                bytes = new byte[1] { 0x02 };
            }
            if (encoding == Encoding.UTF7)
            {
                bytes = new byte[1] { 0x03 };
            }

            innerStream.Write(bytes, 0, bytes.Length);
        }


        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                this.writer.Write(0);
                return;
            }

            var len = value.Length;
            if (len > byte.MaxValue) { len = byte.MaxValue; }
            var chars = value.ToCharArray().Take(len).ToArray();

            this.writer.Write(chars.Length);
            this.writer.Write(chars);
        }
        public void Write(float single) { this.writer.Write(single); }
        public void Write(int value) { this.writer.Write(value); }
        public void Write(uint value) { this.writer.Write(value); }
        public void Write(Vector2 value) { this.writer.Write(value.X); this.writer.Write(value.Y); }
        public void Write(Vector3 value) { this.writer.Write(value.X); this.writer.Write(value.Y); this.writer.Write(value.Z); }
        public void Write(Vector4 value) { this.writer.Write(value.X); this.writer.Write(value.Y); this.writer.Write(value.Z); this.writer.Write(value.W); }
        public void Write(Quaternion value) { this.Write(value.ToVector4()); }
        public void Write(Color color) { this.Write(color.ToVector4()); }
        public void Write(AssetId id) { this.writer.Write(id.Id.ToByteArray()); }
        public void Write(GameObject gameObject)
        {
            this.Write(gameObject.GetType().Name);
            var components = gameObject.AllComponents
                .Where(comp => comp.GetType().GetInterface(nameof(ISerializationObject)) != null)
                .Where(comp => comp.GetType() != typeof(Transform));

            if (gameObject.GetType().GetInterface(nameof(ISerializationObject)) != null)
            {
                ((ISerializationObject)gameObject).Serialize(this);
            }

            this.writer.Write(components.Count());
            foreach (var comp in components)
            {
                this.Write(comp);
            }
        }

        public void Write(bool value) { this.writer.Write(value ? (byte)1 : (byte)0); }

        public void Write(GameObjectComponent component)
        {
            this.Write(component.GetType().Name);
            if (component.GetType().GetInterface(nameof(ISerializationObject)) != null)
            {
                ((ISerializationObject)component).Serialize(this);
            }
        }

        public void Dispose()
        {
            this.writer?.Dispose();
        }
    }
}
