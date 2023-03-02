// Source: WebConfiguration
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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace WebLib.IO
{
    /// <summary>
    /// A configuration that stores any and all variables and values used
    /// by the <see cref="WebServer"/>.
    /// </summary>
    public sealed class WebConfiguration
    {
        /// <summary>
        /// Returns the default configuration if none is imported.
        /// </summary>
        public static readonly WebConfiguration Default = new WebConfiguration()
        {
            IsSslEnabled = false,
            WebPort = 80,
            Prefixes = new[]
                    {
                        "http://localhost:80/",
                    },
            WorkingDirectory =
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
        };


        /// <summary>
        /// Gets or sets the default SSL port used for secure
        /// connections.
        /// </summary>
        public ushort SslPort { get; set; }

        /// <summary>
        /// Gets or sets the default port when any non-ssl connections
        /// are made.
        /// </summary>
        public ushort WebPort { get; set; }

        /// <summary>
        /// Gets or sets the name of the certificate that will be used
        /// to bind the server with.
        /// </summary>
        public string CertificateName { get; set; }

        /// <summary>
        /// Gets or sets the location of any certificate that
        /// may be required to connect with.
        /// </summary>
        public StoreLocation CertificateStoreLocation { get; set; }

        /// <summary>
        /// Gets or sets the prefix list that would allow for connections.
        /// </summary>
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Gets or sets the virtual directory for when requesting 
        /// resources.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the relative url to redirect connections to.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the server will utilize SSL 
        /// connections.
        /// </summary>
        public bool IsSslEnabled { get; set; }
    }
}
