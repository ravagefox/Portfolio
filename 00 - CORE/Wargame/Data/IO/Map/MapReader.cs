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


        public MapReader(Stream mapStream)
        {
            this.SearchAssemblies = new List<Assembly>();
            this.reader = new BinaryReader(mapStream, Encoding.UTF8);

            if (!this.reader.ReadChars(MapWriter.MAGIC.Length).SequenceEqual(
                MapWriter.MAGIC))
            {
                throw new InvalidDataException(
                    "The header appears to be corrupt or invalid.");
            }
        }

        public Guid ReadGuid() { return new Guid(this.reader.ReadBytes(16)); }

        public uint ReadUInt32() { return this.reader.ReadUInt32(); }

        public int ReadInt32() { return this.reader.ReadInt32(); }

        public bool ReadBoolean() { return this.reader.ReadByte() != 0; }

        public float ReadSingle() { return this.reader.ReadSingle(); }

        public Vector3 ReadVector3()
        {
            return new Vector3(this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
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

        public string ReadString()
        {
            return this.ReadPascalString();
        }

        public GameObject ReadGameObject()
        {
            var goType = this.ReadString();
            foreach (var type in this.GetAssemblyTypes())
            {
                if (type.Name.SequenceEqual(goType))
                {
                    var instance = (GameObject)Activator.CreateInstance(type);
                    if (type.GetInterface(nameof(IObjectSerialization)) != null)
                    {
                        ((IObjectSerialization)instance).Deserialize(this);
                    }

                    var components = this.reader.ReadInt32();
                    for (var i = 0; i < components; i++)
                    {
                        var component = this.ReadGameComponent();
                        if (instance != null)
                        {
                            instance.AddComponent(component);
                        }
                    }

                    return instance;
                }
            }

            return null;
        }

        public GameObjectComponent ReadGameComponent()
        {
            var goType = this.ReadString();
            foreach (var type in this.GetAssemblyTypes())
            {
                if (type.Name.SequenceEqual(goType))
                {
                    var instance = (GameObjectComponent)Activator.CreateInstance(type);
                    if (type.GetInterface(nameof(IObjectSerialization)) != null)
                    {
                        ((IObjectSerialization)instance).Deserialize(this);
                    }

                    return instance;
                }
            }

            return null;
        }

        private IEnumerable<Type> GetAssemblyTypes()
        {
            if (this.SearchAssemblies.Count == 0) { return Enumerable.Empty<Type>(); }

            var result = new List<Type>();
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

                if (result.Count > 0)
                {
                    types = types.Except(result).ToArray();
                }

                result.AddRange(types);
            }

            return result;
        }

        private string ReadPascalString()
        {
            var len = this.reader.ReadInt32();
            if (len > 255) { len = 255; }

            return new string(this.reader.ReadChars(len));
        }


        public void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}
