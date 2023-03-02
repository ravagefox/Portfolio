// Source: WmxStream
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
using Engine.Assets;
using Engine.Data;
using Engine.Graphics;

namespace Client.Data.IO.Map
{
    public sealed class WmxStream : IDisposable
    {
        #region Headers

        public struct WmxHeader
        {
            public char[] Magic;

            public uint DoodadCount;
            public uint TerrainCellCount;
            public uint MapId;
            public uint SkyboxId;
            public uint TerrainId;
        }

        public struct DoodadEntry
        {
            public uint ObjectType;
            public float X, Y, Z;
            public float ScaleX, ScaleY, ScaleZ;
            public float Yaw, Pitch, Roll;
            public uint ModelId;
        }

        #endregion

        #region Fields

        public readonly char[] WMX_MAGIC = "WMX2".ToCharArray();

        private BinaryReader _reader;
        private WmxHeader _header;

        #endregion

        public WmxStream(Stream inStream)
        {
            _reader = new BinaryReader(inStream);

            var mgc = _reader.ReadChars(WMX_MAGIC.Length);
            if (!mgc.SequenceEqual(WMX_MAGIC))
            {
                throw new InvalidDataException(
                    "The header appears to be corrupt or invalid.");
            }

            _header = ReadHeader();
        }

        private WmxHeader ReadHeader()
        {
            return new WmxHeader()
            {
                Magic = WMX_MAGIC,
                DoodadCount = _reader.ReadUInt32(),
                TerrainCellCount = _reader.ReadUInt32(),
                TerrainId = _reader.ReadUInt32(),
                MapId = _reader.ReadUInt32(),
                SkyboxId = _reader.ReadUInt32(),
            };
        }



        #region Public Methods

        public Terrain ReadTerrain()
        {
            var iLoader = ServiceProvider.Instance.GetService<ILoader>();

        }

        public void Dispose()
        {
            _reader?.Dispose();
        }

        #endregion
    }
}
