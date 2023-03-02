// Source: HttpWebResponse
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
using System.Net.Mime;
using System.Text;
using WebLib.IO;

namespace WebLib.WebResponses
{
    /// <summary>
    /// Provides an interface for when a custom web response needs
    /// to be requested, and responsed to.
    /// </summary>
    public abstract class HttpWebResponse : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the context that was used to create this 
        /// response.
        /// </summary>
        public HttpListenerContext Context { get; }

        /// <summary>
        /// Gets or sets the content type to accompany the headers.
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets the encoding used by the request.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Gets or sets the status code for the current instance.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        #endregion

        #region Fields and Events

        /// <summary>
        /// An event handler for when the current instance needs
        /// to dispose.
        /// </summary>
        public event EventHandler Disposing;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HttpWebResponse(HttpListenerContext context)
        {
            this.Context = context;
            this.ContentType = MimeLookup.Html;
            this.Encoding = context.Request.ContentEncoding;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// When overriden in a derrived class, creates and distributes the 
        /// stream of data that will be written to the context.
        /// </summary>
        /// <returns></returns>
        protected abstract Stream GetResponseStream();

        /// <summary>
        /// When overriden in a derrived class, disposes of the current instance
        /// and its resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            Disposing?.Invoke(sender, e);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Disposes of the current instance and its resources.
        /// </summary>
        public void Dispose()
        {
            this.OnDisposing(this, EventArgs.Empty);

            using (var rStream = this.GetResponseStream())
            {
                this.Context.Response.StatusCode = (int)this.StatusCode;
                this.Context.Response.ContentLength64 = rStream.Length;
                this.Context.Response.ContentType = this.ContentType.MediaType;
                this.Context.Response.ContentEncoding = this.Encoding;

                rStream.CopyTo(this.Context.Response.OutputStream, 2048);
            }

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
