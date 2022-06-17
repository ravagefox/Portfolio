// Source: MapReader
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Engine.Core;
using Microsoft.Xna.Framework;

namespace Wargame.Data.IO.Map
{
    public sealed class MapReader : IDisposable
    {
        public List<Assembly> SearchAssemblies { get; }


        private BinaryReader reader;


        public MapReader(Stream instream)
        {
            this.SearchAssemblies = new List<Assembly>()
            {
                typeof(MapReader).Assembly,
            };

            var encoding = this.ReadEncoding(instream);
            this.reader = new BinaryReader(instream, encoding);

            this.ReadHeader();
        }

        public string ReadString()
        {
            var len = this.reader.ReadInt32();
            var chars = this.reader.ReadChars(len);
            return new string(chars);
        }
        public bool ReadBoolean() { return this.reader.ReadByte() != 0; }
        public float ReadSingle() { return this.reader.ReadSingle(); }
        public int ReadInt32() { return this.reader.ReadInt32(); }
        public uint ReadUInt32() { return this.reader.ReadUInt32(); }
        public Vector2 ReadVector2()
        {
            return new Vector2(this.ReadSingle(), this.ReadSingle());
        }
        public Vector3 ReadVector3()
        {
            return new Vector3(this.ReadVector2(), this.ReadSingle());
        }
        public Vector4 ReadVector4()
        {
            return new Vector4(this.ReadVector3(), this.ReadSingle());
        }
        public Quaternion ReadQuaternion()
        {
            return new Quaternion(this.ReadVector4());
        }

        public Color ReadColor()
        {
            return Color.FromNonPremultiplied(this.ReadVector4());
        }

        public Guid ReadGuid()
        {
            return new Guid(this.reader.ReadBytes(16));
        }

        public GameObject ReadGameObject()
        {
            var types = this.GetAssemblyTypes();
            var objType = this.ReadString();
            var type = types
                .FirstOrDefault(t => t.Name.SequenceEqual(objType));

            if (type == null) { return null; }

            var instance = (GameObject)Activator.CreateInstance(type);
            if (type.GetInterface(nameof(ISerializationObject)) != null)
            {
                ((ISerializationObject)instance).Deserialize(this);
            }

            var compCount = this.reader.ReadInt32();
            for (var i = 0; i < compCount; i++)
            {
                var component = this.ReadGameComponent();
                if (component != null)
                {
                    instance.AddComponent(component);
                }
            }

            return instance;
        }

        public GameObjectComponent ReadGameComponent()
        {
            var types = this.GetAssemblyTypes();
            var compName = this.ReadString();
            var compType = types
                .FirstOrDefault(t => t.Name.SequenceEqual(compName));
            if (compType != null)
            {
                var component = (GameObjectComponent)Activator.CreateInstance(compType);
                if (compType.GetInterface(nameof(ISerializationObject)) != null)
                {
                    ((ISerializationObject)component).Deserialize(this);
                }

                return component;
            }

            return null;
        }


        private IEnumerable<Type> GetAssemblyTypes()
        {
            if (this.SearchAssemblies.Count == 0) { return Enumerable.Empty<Type>(); }

            var typeList = new List<Type>();
            foreach (var asm in this.SearchAssemblies)
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                typeList.AddRange(types.Except(typeList));
            }

            return typeList;
        }

        private void ReadHeader()
        {
            if (!this.reader.ReadChars(MapWriter.MAGIC.Length).SequenceEqual(
                MapWriter.MAGIC))
            {
                throw new InvalidDataException(
                    "The header appears to be corrupt or invalid.");
            }
        }

        private Encoding ReadEncoding(Stream instream)
        {
            var bytes = new byte[1];
            instream.Read(bytes, 0, bytes.Length);

            return bytes[0] == 0x00 ? Encoding.UTF8 :
                   bytes[0] == 0x01 ? Encoding.Unicode :
                   bytes[0] == 0x02 ? Encoding.UTF8 :
                   bytes[0] == 0x03 ? Encoding.UTF7 :
                   Encoding.ASCII;
        }


        public void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}
