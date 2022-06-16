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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Assets;
using Engine.Assets.IO;
using Engine.Core;
using Engine.Data;
using Engine.Data.IO;
using Engine.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.IO.ContentReaders;

namespace Wargame.Data
{
    internal sealed class AssetManager : IStreamContainer, ILoader, IDisposable
    {
        public AppProfile Profile { get; }


        private ConvertManager converters;
        private ArchiveManager archives;
        private bool disposedValue;


        public AssetManager(AppProfile profile)
        {
            ServiceProvider.Instance.AddService<ILoader>(this);
            ServiceProvider.Instance.AddService<IStreamContainer>(this);

            this.Profile = profile;
            this.archives = new ArchiveManager(profile);

            this.converters = new ConvertManager();
            this.converters.Register(typeof(Texture2D), ConvertHandlers.ImportTexture2D);
            this.converters.Register(typeof(Effect), ConvertHandlers.ImportEffect);
            this.converters.Register(typeof(UserInterface), ConvertHandlers.ImportUserInterface);
            this.converters.Register(typeof(Model), ConvertHandlers.ImportModel);
        }

        public string[] EnumerateDirectory(params string[] paths)
        {
            var path = this.Profile.GetRelativePath(paths);
            var result = Enumerable.Empty<string>();

            result = this.archives.Enumerate(p => p.StartsWith(path));
            result = result.Concat(Directory.EnumerateFiles(path));
            result = result.Except(result.Where(f => Path.GetExtension(f).EndsWith("db")));

            return result.ToArray();
        }

        public T Load<T>(uint digest)
        {
            return this.archives.Digests.TryGetValue(digest, out var result)
                ? this.Load<T>(result)
                : throw new FileNotFoundException(
                $"The digest specified could not be mapped to file name. DIGEST: {digest}");
        }

        public T Load<T>(string assetPath)
        {
            var tPath = !assetPath.StartsWith(this.Profile.RootDirectory) ? Path.Combine(this.Profile.RootDirectory, assetPath) : assetPath;
            tPath = ConvertHandlers.FixExtension<T>(tPath);

            if (((IStreamContainer)this).CanOpenStream(tPath.ToLower()))
            {
                using (var stream = ((IStreamContainer)this).OpenStream(tPath.ToLower()))
                {
                    var args = new ConvertEventArgs(typeof(T), stream, tPath);
                    Console.WriteLine(tPath);

                    this.converters.Invoke(args);
                    stream.Close();

                    return (T)args.Result;
                }
            }

            return (T)(object)null;
        }

        bool IStreamContainer.CanOpenStream(string assetPath)
        {
            return File.Exists(assetPath) ||
                   this.archives.Any(mdx => mdx.Contains(assetPath));
        }

        Stream IStreamContainer.OpenStream(string assetPath)
        {
            if (!File.Exists(assetPath) && this.archives.Any())
            {
                var stream = this.archives.FirstOrDefault(s => s.Contains(assetPath));
                return new MemoryStream(stream.Read(assetPath));
            }

            return new MemoryStream(File.ReadAllBytes(assetPath));
        }


        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                this.archives.ToList().ForEach(stream => stream.Close());

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    this.archives.ToList().ForEach(stream => stream.Dispose());
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AssetManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
