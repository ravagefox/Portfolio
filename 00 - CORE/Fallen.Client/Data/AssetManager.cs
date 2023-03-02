// Source: AssetManager
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
using System.Linq;
using Client.Data.Converters;
using Engine.Assets;
using Engine.Assets.IO;
using Engine.Core;
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data
{
    public sealed class AssetManager : ILoader, IStreamContainer
    {
        public string RootDirectory { get; set; }


        private ArchiveManager archiveManager;
        private ConvertManager convertManager;


        public AssetManager(AppProfile profile)
        {
            this.archiveManager = new ArchiveManager(profile);
            this.convertManager = new ConvertManager();
            this.convertManager.Register(typeof(UserInterface), Converter.ImportUserInterface);
            this.convertManager.Register(typeof(Texture2D), Converter.ImportTexture2D);
            this.convertManager.Register(typeof(Effect), Converter.ImportEffect);
            this.convertManager.Register(typeof(Model), Converter.ImportModel);

            this.RootDirectory = profile.RootDirectory;

            ServiceProvider.Instance.AddService<IStreamContainer>(this);
            ServiceProvider.Instance.AddService<ILoader>(this);
        }


        public T Load<T>(string path)
        {
            object result = null;
            var truePath = this.FixPath(path);
            if (this.convertManager.IsRegistered(typeof(T)))
            {
                using (var stream = this.OpenStream(truePath))
                {
                    var cArgs = new ConvertEventArgs(typeof(T), stream, truePath);

                    result = this.convertManager.Invoke(cArgs);
                }
            }

            return (T)result;
        }

        public bool CanOpenStream(string assetPath)
        {
            var truePath = this.FixPath(assetPath);

            return this.archiveManager.GetFilesSharing(truePath).Any() ||
                   File.Exists(truePath);
        }

        public Stream OpenStream(string assetPath)
        {
            var truePath = this.FixPath(assetPath);

            if (!this.CanOpenStream(truePath))
            {
                throw new FileNotFoundException(
                    "File could not be found.",
                    new Exception(truePath));
            }

            // Find the given asset in the asset manager first.
            return this.archiveManager.GetStream(truePath) is MdxStream stream ?
                   new MemoryStream(stream.Read(truePath)) :
                   (Stream)File.OpenRead(truePath);
        }

        private string FixPath(string path)
        {
            return !path.StartsWith(this.RootDirectory) ?
                   Path.Combine(this.RootDirectory, path) :
                   path;
        }

        public void Unload()
        {
            this.archiveManager?.ToList().ForEach(mdx => mdx.Close());
            this.archiveManager = null;
            this.convertManager = null;
        }
    }
}
