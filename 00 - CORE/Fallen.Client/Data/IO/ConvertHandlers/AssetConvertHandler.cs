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

using System;
using System.IO;
using System.Security.Cryptography;
using Engine.Data;
using Engine.Data.Collections;
using Engine.Data.Cryptography;
using Engine.Data.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.IO.ConvertHandlers
{
    public partial class AssetConvertHandler
    {
        public static GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;


        private static readonly CacheSet<Texture2D> textureCache = new CacheSet<Texture2D>();
        private static readonly CacheSet<Model> modelCache = new CacheSet<Model>();
        private static readonly CacheSet<Effect> shaderCache = new CacheSet<Effect>();

        private static byte[] salt;



        internal static string GetExtension(Type type, string truePath)
        {
            var ext = Path.GetExtension(truePath);
            if (string.IsNullOrEmpty(ext))
            {
                if (type == typeof(Texture2D))
                {
                    return truePath + ".png";
                }

                if (type == typeof(Effect))
                {
                    return truePath + ".hlsl";
                }

                if (type == typeof(Model))
                {
                    return truePath + ".m2x";
                }
            }

            return truePath;
        }
        private static string ComputeHash(ConvertEventArgs args)
        {
            var result = string.Empty;
            using (var hashCalc = new HashCalculator<MD5>())
            {
                if (salt == null)
                {
                    hashCalc.GenerateSalt(51);
                    salt = hashCalc.Salt;
                }
                else { hashCalc.SetSalt(salt); }

                var data = ReadData(args.Input as Stream);
                data = hashCalc.Transform(data);

                result = hashCalc.Encoding.GetString(data);
            }

            return result;
        }

        private static byte[] ReadData(Stream inStream)
        {
            inStream.Position = 0;
            var data = new byte[inStream.Length];
            inStream.Read(data, 0, data.Length);

            inStream.Position = 0;
            return data;
        }
    }
}
