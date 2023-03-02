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
using Engine.Assets.IO;
using Engine.Data.IO;
using Engine.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.IO.ConvertHandlers
{
    public partial class AssetConvertHandler
    {
        public static void ImportEffect(ConvertEventArgs e)
        {
            var hashKey = ComputeHash(e);
            if (shaderCache.Contains(hashKey))
            {
                e.Result = shaderCache[hashKey];
                return;
            }

            var filePath = e.AssetPath;
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(ext))
            {
                var expectedExtension = ".bin";
                if (DebugDraw.DebugMode) { expectedExtension = ".hlsl"; }

                filePath += expectedExtension;
            }

            var builder = new ShaderBuilder(filePath);
            var bytecode = builder.GetShaderBytecode(DebugDraw.DebugMode);
            shaderCache.Add(hashKey, new Effect(GraphicsDevice, bytecode));

            e.Result = shaderCache[hashKey];
        }
    }
}
