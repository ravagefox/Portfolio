// Source: SslManager
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
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace WebLib.Security
{
    /// <summary>
    /// Provides basic SSL services to the current web platform.
    /// </summary>
    public sealed class SslManager : IDisposable
    {
        #region Fields and Events
        /// <summary>
        /// An event for when the manager is disposing.
        /// </summary>
        public event EventHandler Disposing;


        private Dictionary<string, X509Certificate2> _certificateLibrary;
        private bool disposedValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public SslManager()
        {
            this._certificateLibrary = new Dictionary<string, X509Certificate2>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns whether the certificate was successfully found.
        /// </summary>
        /// <param name="certificateName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public bool FindCertificate(
            string certificateName,
            StoreLocation storeLocation,
            out X509Certificate2 certificate)
        {
            certificate = null;

            using (var store = new X509Store(StoreName.My, storeLocation))
            {
                var certs = store.Certificates.Find(
                    X509FindType.FindBySubjectName,
                    certificateName,
                    true);

                if (certs.Count > 0)
                {
                    certificate = certs[0];
                }
            }

            return certificate != null;
        }

        /// <summary>
        /// Returns whether the certificate was successfully bound to the application.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sslPort"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public bool BindSsl(
            Guid appId,
            ushort sslPort,
            X509Certificate2 certificate)
        {
            if (!this._certificateLibrary.TryGetValue(appId.ToString(), out _))
            {
                this._certificateLibrary[appId.ToString()] = certificate;

                var appIdValue = "{" + appId.ToString() + "}";
                var hashString = certificate.GetCertHashString();

                var netshProcessInfo = new ProcessStartInfo()
                {
                    FileName = "netsh",
                    Arguments = $"http add sslcert ipport=0.0.0.0:{sslPort} certhash={hashString} appid={appIdValue}",
                };

                var netshProcess = Process.Start(netshProcessInfo);
                netshProcess.WaitForExit();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Unbinds the specified application.
        /// </summary>
        /// <param name="sslPort"></param>
        /// <param name="appId"></param>
        public void Unbind(Guid appId, ushort sslPort)
        {
            if (this._certificateLibrary.TryGetValue(appId.ToString(), out _))
            {
                var info = new ProcessStartInfo()
                {
                    FileName = "netsh",
                    Arguments = $"http delete sslcert ipport=0.0.0.0:{sslPort}",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                };
                var netshProcess = Process.Start(info);
                netshProcess.WaitForExit();

                _ = this._certificateLibrary.Remove(appId.ToString());
            }
        }
        #endregion

        #region Private Methods
        private void PurgeLibrary()
        {
            foreach (var pair in this._certificateLibrary)
            {
                this._certificateLibrary[pair.Key]?.Dispose();
            }

            this._certificateLibrary.Clear();
        }
        #endregion

        #region Dispose
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    Disposing?.Invoke(this, EventArgs.Empty);
                    this.PurgeLibrary();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes of the current instance and its resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
