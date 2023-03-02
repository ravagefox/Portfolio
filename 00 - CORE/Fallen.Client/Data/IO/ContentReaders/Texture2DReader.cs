// Source: Texture2DReader
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

using System.Drawing;
using System.IO;
using System.Linq;
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics;
using Xna = Microsoft.Xna.Framework.Graphics;

namespace Client.Data.IO.ContentReaders
{
    public static partial class ConvertHandlers
    {
        public static void ImportTexture2D(ConvertEventArgs e)
        {
            var hash = new Hash(GetData(e.Input as Stream));
            if (textureCache.Contains(hash.ToString()))
            {
                e.Result = textureCache[hash.ToString()];
                return;
            }

            var ext = Path.GetExtension(e.AssetPath);
            if (ext.SequenceEqual(".tzx"))
            {
                e.Result = (Xna.Texture2D)Texture.Read(e.Input as Stream);
            }
            else
            {
                var bmp = (Bitmap)Image.FromStream(e.Input as Stream);
                e.Result = (Xna.Texture2D)Texture.Create(bmp);
            }


            textureCache.Add(hash.ToString(), (Xna.Texture2D)e.Result);
        }
    }
}
