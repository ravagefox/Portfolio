// Source: AssetConvertHandler
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
using Engine.Data.IO;
using Microsoft.Xna.Framework.Graphics;
using MIX;

namespace Client.Data.IO.ConvertHandlers
{
    public partial class AssetConvertHandler
    {
        public static void ImportModel(ConvertEventArgs e)
        {
            var hashKey = ComputeHash(e);
            if (modelCache.Contains(hashKey))
            {
                e.Result = modelCache[hashKey];
                return;
            }

            var mixFormat = new M2XFormat(e.Input as Stream);
            //var meshBuilder = new M2XMeshBuilder(mixFormat);

            //e.Result = meshBuilder.BuildModel();
            //modelCache.Add(hashKey, (Model)e.Result);
        }
    }
}
