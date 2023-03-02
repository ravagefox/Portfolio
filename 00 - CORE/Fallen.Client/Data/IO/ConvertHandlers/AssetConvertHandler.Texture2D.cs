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

namespace Client.Data.IO.ConvertHandlers
{
    public partial class AssetConvertHandler
    {
        public static void ImportTexture2D(ConvertEventArgs args)
        {
            var hashKey = ComputeHash(args);
            if (textureCache.Contains(hashKey))
            {
                args.Result = textureCache[hashKey];
                return;
            }

            var texture = BuildTexture(args);
            textureCache.Add(hashKey, texture);
            args.Result = texture;
        }

        private static Texture2D BuildTexture(ConvertEventArgs args)
        {
            var ext = Path.GetExtension(args.AssetPath);
            return ext.ToLower().EndsWith("tzx")
                ? (Texture2D)Engine.Graphics.Texture.Read(args.Input as Stream)
                : (Texture2D)Engine.Graphics.Texture.FromBmpStream(args.Input as Stream);
        }
    }
}
