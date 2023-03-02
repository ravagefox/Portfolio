// Source: ServerManager
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
using System.Reflection;
using System.Runtime.InteropServices;
using WebLib.IO;
using WebLib.IO.Serialization;
using WebLib.Security;

namespace WebLib
{
    /// <summary>
    /// Provides a management-like object which controls the server
    /// and its current operations.
    /// </summary>
    public sealed class ServerManager : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="Assembly.GetEntryAssembly"/>
        /// Application Id if it's present.
        /// </summary>
        public Guid AppId { get; private set; }


        private SslManager _sslManager;
        private WebServer _server;
        private bool _certificateBound;
        private WebConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configPath"></param>
        public ServerManager(string configPath)
        {
            this._sslManager = new SslManager();
            this._server = new WebServer();
            this._configuration = this.LoadConfiguration(configPath);

            ServiceProvider.Instance.AddService<WebConfiguration>(this._configuration);
            ServiceProvider.Instance.AddService<WebServer>(this._server);
            ServiceProvider.Instance.AddService<SslManager>(this._sslManager);
            ServiceProvider.Instance.AddService<ServerManager>(this);

            //this._configuration = this.LoadConfiguration(configPath);
        }

        /// <summary>
        /// Begins hosting the required services on the server.
        /// </summary>
        public void BeginHosting()
        {
            if (this._configuration.IsSslEnabled)
            {
                if (this._sslManager.FindCertificate(
                    this._configuration.CertificateName,
                    this._configuration.CertificateStoreLocation,
                    out var certificate))
                {
                    var appId = Assembly.GetEntryAssembly().GetCustomAttribute<GuidAttribute>();
                    if (appId != null)
                    {
                        var value = appId.Value;
                        this.AppId = Guid.Parse(value);

                        this._certificateBound = this._sslManager.BindSsl(
                            this.AppId,
                            this._configuration.SslPort,
                            certificate);
                    }
                }
            }

            foreach (var prefix in this._configuration.Prefixes)
            {
                this._server.AddPrefix(prefix);
                Console.WriteLine($"Hosting on {prefix}");
            }

            this._server.Start();
            if (this._server.IsRunning)
            {
                Console.WriteLine("Server is running");
            }
        }

        /// <summary>
        /// Returns the loaded <see cref="WebConfiguration"/> from source file path.
        /// </summary>
        /// <param name="xmlConfigPath"></param>
        /// <returns></returns>
        public WebConfiguration LoadConfiguration(string xmlConfigPath)
        {
            if (string.IsNullOrEmpty(xmlConfigPath) ||
                !File.Exists(xmlConfigPath))
            {
                this._configuration = WebConfiguration.Default;
            }

            using (var fr = File.OpenRead(xmlConfigPath))
            {
                this._configuration = GSerializer.Deserialize<WebConfiguration>(fr);
            }

            return this._configuration;
        }

        /// <summary>
        /// Disposes of the current instance and its resources.
        /// </summary>
        public void Dispose()
        {
            if (this._certificateBound)
            {
                this._sslManager.Unbind(this.AppId, this._configuration.SslPort);
            }

            this._server?.Dispose();
            this._sslManager?.Dispose();
        }

        /// <summary>
        /// Returns a local path that is relative to the 
        /// <see cref="Assembly.GetEntryAssembly"/>'s location.
        /// </summary>
        /// <param name="fileRelativePath"></param>
        /// <returns></returns>
        public string GetLocalPath(string fileRelativePath)
        {
            var asmLocation = Assembly.GetEntryAssembly().Location;
            var asmDirectory = Path.GetDirectoryName(asmLocation);

            return
                NormalizePath(asmDirectory) +
                NormalizePath(fileRelativePath);
        }

        /// <summary>
        /// Returns the virtual path corresponding relatively to the 
        /// specified working directory inside the <see cref="WebConfiguration"/>.
        /// </summary>
        /// <param name="fileRelativePath"></param>
        /// <returns></returns>
        public string GetWorkingPath(string fileRelativePath)
        {
            return
                NormalizePath(this._configuration.WorkingDirectory) +
                NormalizePath(fileRelativePath);
        }


        private static string NormalizePath(string path)
        {
            var fPath = path.Replace(
                Path.AltDirectorySeparatorChar, // '/'
                Path.DirectorySeparatorChar); // '\'
            if (fPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fPath = fPath.Substring(0, fPath.Length - 1);
            }

            return fPath;
        }
    }
}