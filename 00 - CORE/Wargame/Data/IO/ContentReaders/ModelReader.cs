// Source: ConvertHandlers
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
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics.Data.IO;
using Microsoft.Xna.Framework.Graphics;
using MIX;

namespace Wargame.Data.IO.ContentReaders
{
    internal static partial class ConvertHandlers
    {
        public static void ImportModel(ConvertEventArgs e)
        {
            var hash = new Hash(GetData(e.Input as Stream));
            if (modelCache.Contains(hash.ToString()))
            {
                e.Result = modelCache[hash.ToString()];
                return;
            }

            var format = MixFormat.Read(new MemoryStream(hash.Raw));
            var meshBuilder = new MixMeshBuilder(format);
            e.Result = meshBuilder.BuildModel();

            modelCache.Add(hash.ToString(), (Model)e.Result);
        }
    }
}
