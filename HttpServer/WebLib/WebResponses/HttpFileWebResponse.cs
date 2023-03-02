// Source: HttpFileWebResponse
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
using System.Net;
using WebLib.IO;

namespace WebLib.WebResponses
{
    /// <summary>
    /// Build a response that reads in a file and prepares it for sending.
    /// </summary>
    public sealed class HttpFileWebResponse : HttpWebResponse
    {
        /// <summary>
        /// Gets the <see cref="IO.WebConfiguration"/> that was used
        /// to load the server.
        /// </summary>
        public WebConfiguration WebConfiguration =>
            ServiceProvider.Instance.GetService<WebConfiguration>();

        /// <summary>
        /// Gets or sets the file request.
        /// </summary>
        public Uri RequestUri
        {
            get => this._requestUri;
            set
            {
                if (this._requestUri != value)
                {
                    this._requestUri = value;
                    this.FixMimeType();
                }
            }
        }

        private Uri _requestUri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HttpFileWebResponse(HttpListenerContext context) : base(context)
        {
        }


        private void FixMimeType()
        {
            var ext = Path.GetExtension(this._requestUri.AbsolutePath);
            this.ContentType = MimeLookup.GetContentType(ext);
        }


        /// <summary>
        /// When overriden in a derrived class, opens the file for reading.
        /// </summary>
        /// <returns></returns>
        protected override Stream GetResponseStream()
        {
            var serverManager = ServiceProvider.Instance.GetService<ServerManager>();
            var workingPath = serverManager.GetWorkingPath(this._requestUri.LocalPath);

            this.StatusCode = File.Exists(workingPath) ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            return File.Exists(workingPath) ? File.OpenRead(workingPath) : (Stream)new MemoryStream();
        }
    }
}
