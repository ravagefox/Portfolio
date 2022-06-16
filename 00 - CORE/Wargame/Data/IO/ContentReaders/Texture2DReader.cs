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
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics;

namespace Wargame.Data.IO.ContentReaders
{
    using Xna = Microsoft.Xna.Framework.Graphics;

    internal static partial class ConvertHandlers
    {
        public static void ImportTexture2D(ConvertEventArgs e)
        {
            var hash = new Hash(GetData(e.Input as Stream));
            if (textureCache.Contains(hash.ToString()))
            {
                e.Result = textureCache[hash.ToString()];
                return;
            }

            if (IsDebug)
            {
                using (var bmp = (Bitmap)Image.FromStream(new MemoryStream(GetData(e.Input as Stream))))
                {
                    e.Result = (Xna.Texture2D)Texture.Create(bmp);
                }
            }
            else { e.Result = (Xna.Texture2D)Texture.Read(e.Input as Stream); }

            textureCache.Add(hash.ToString(), (Xna.Texture2D)e.Result);
        }
    }
}
