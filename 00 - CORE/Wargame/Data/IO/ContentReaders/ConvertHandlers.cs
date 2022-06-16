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

using System;
using System.IO;
using Engine.Data;
using Engine.Data.Collections;
using Engine.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.IO.ContentReaders
{
    internal static partial class ConvertHandlers
    {
        public static GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public static bool IsDebug =>
#if DEBUG
            true;
#else 
            false;
#endif

        private static readonly CacheSet<Texture2D> textureCache = new CacheSet<Texture2D>();
        private static readonly CacheSet<Effect> effectCache = new CacheSet<Effect>();
        private static readonly CacheSet<Model> modelCache = new CacheSet<Model>();


        private static byte[] GetData(
            Stream stream,
            int bufferSize = 64 * 1000 * 1000)
        {
            var data = new byte[bufferSize];
            var oldPos = stream.Position;
            _ = stream.Seek(0, SeekOrigin.Begin);

            var size = stream.Read(data, 0, data.Length);
            Array.Resize(ref data, size);

            _ = stream.Seek(oldPos, SeekOrigin.Begin);
            return data;
        }


        internal static string FixExtension<TType>(string path)
        {
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                if (typeof(TType) == typeof(Effect))
                {
                    return path + (IsDebug ? ".hlsl" : ".cso");
                }

                if (typeof(TType) == typeof(Model))
                {
                    return path + ".m2x";
                }

                if (typeof(TType) == typeof(Texture2D))
                {
                    return path + (IsDebug ? ".png" : ".tx2");
                }

                if (typeof(TType) == typeof(UserInterface))
                {
                    return path + ".xml";
                }
            }

            return path;
        }
    }
}
