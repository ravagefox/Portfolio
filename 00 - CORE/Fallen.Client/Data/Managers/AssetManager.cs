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
using Client.Data.IO.ConvertHandlers;
using Engine.Assets;
using Engine.Assets.IO;
using Engine.Core;
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Managers
{
    public sealed class AssetManager : ManagerService, ILoader, IStreamContainer
    {
        public string RootDirectory { get; set; }

        private ArchiveManager archiveManager;
        private ConvertManager convertManager;

        public override void Initialize()
        {
            ServiceProvider.Instance.AddService<AssetManager>(this);
            ServiceProvider.Instance.AddService<IStreamContainer>(this);
            ServiceProvider.Instance.AddService<ILoader>(this);

            var profile = ServiceProvider.Instance.GetService<AppProfile>();

            this.RootDirectory = profile.RootDirectory;
            this.archiveManager = new ArchiveManager(profile);
            this.convertManager = new ConvertManager();

            this.convertManager.Register(typeof(UserInterface), AssetConvertHandler.ImportUserInterface);
            this.convertManager.Register(typeof(Effect), AssetConvertHandler.ImportEffect);
            this.convertManager.Register(typeof(Texture2D), AssetConvertHandler.ImportTexture2D);
            this.convertManager.Register(typeof(Model), AssetConvertHandler.ImportModel);
        }

        public bool CanOpenStream(string assetPath)
        {
            var truePath = this.FixPath(assetPath);
            return this.archiveManager.GetStream(truePath) != null ||
                   File.Exists(truePath);
        }

        public T Load<T>(string path)
        {
            var ext = Path.GetExtension(path);
            var truePath = this.FixPath(path);
            truePath = AssetConvertHandler.GetExtension(typeof(T), truePath);

            if (this.convertManager.IsRegistered(typeof(T)))
            {
                object result = null;
                if (this.OpenStream(truePath) is Stream stream)
                {
                    var e = new ConvertEventArgs(typeof(T), stream, truePath);

                    result = this.convertManager.Invoke(e);
                    stream.Close();
                }

                return (T)result;
            }

            throw new Exception(
                "The type specified cannot be loaded by the current instance.",
                new Exception("param: " + nameof(T)));
        }

        public Stream OpenStream(string assetPath)
        {
            var truePath = this.FixPath(assetPath);
            if (this.archiveManager.GetStream(truePath) is Stream stream)
            {
                return new MemoryStream(
                    ((MdxStream)stream).Read(truePath));
            }
            else
            {
                stream = File.OpenRead(truePath);
                return stream;
            }
        }

        private string FixPath(string inFile)
        {
            return !inFile.StartsWith(this.RootDirectory) ?
                   Path.Combine(this.RootDirectory, inFile) :
                   inFile;
        }

        protected override void Dispose(bool disposing)
        {
            this.archiveManager?.Dispose();

            this.convertManager = null;
            this.archiveManager = null;

            base.Dispose(disposing);
        }
    }
}
