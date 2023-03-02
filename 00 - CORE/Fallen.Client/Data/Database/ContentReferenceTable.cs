// Source: ContentReferenceTable
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
using System.IO;
using Engine.Assets.IO.Database;
using Engine.Data;

namespace Client.Data.Database
{
    [DataTable(nameof(ContentReferenceTable))]
    public struct ContentReferenceTable : IReference, IRecord, IDbRef
    {
        [DataColumn(0)]
        public AssetId Id { get; set; }

        [DataColumn(1)]
        public int RefId { get; set; }

        [DataColumn(2)]
        public string Name { get; set; }

        [DataColumn(3)]
        public string TextureRelativePath { get; set; }


        public static ContentReferenceTable FromBlob(byte[] blob)
        {
            using (var dbxReader = new DbxReader(null, new MemoryStream(blob)))
            {
                var instance = dbxReader.ReadRecord();
                return Query.Create<ContentReferenceTable>(instance);
            }
        }

        public DataRecord GetRecord()
        {
            return Query.Create(this);
        }

        public byte[] ToArray()
        {
            using (var dbxWriter = new DbxWriter(null, new MemoryStream()))
            {
                dbxWriter.Write(this.GetRecord());
                return ((MemoryStream)dbxWriter.BaseStream).ToArray();
            }
        }
    }
}
