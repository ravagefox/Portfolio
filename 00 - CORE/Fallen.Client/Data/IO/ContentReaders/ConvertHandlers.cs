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
using Engine.Data.Collections;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.IO.ContentReaders
{
    public static partial class ConvertHandlers
    {
        public static GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>().GraphicsDevice;

        public static bool IsDebug =>
#if DEBUG
            true;
#elif !DEBUG
            false;
#endif

        private static readonly CacheSet<Texture2D> textureCache = new CacheSet<Texture2D>();
        private static readonly CacheSet<Effect> effectCache = new CacheSet<Effect>();
        private static readonly CacheSet<SoundEffect> soundCache = new CacheSet<SoundEffect>();
        private static readonly CacheSet<Model> modelCache = new CacheSet<Model>();


        private static byte[] GetData(Stream dataStream)
        {
            var data = new byte[dataStream.Length];
            var oldPos = dataStream.Position;

            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.Read(data, 0, data.Length);

            dataStream.Seek(oldPos, SeekOrigin.Begin);
            return data;
        }

    }
}
