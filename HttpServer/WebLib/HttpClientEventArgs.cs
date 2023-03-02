// Source: WebServerBase
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
using System.Net;
using System.Net.Http;

namespace WebLib
{
    /// <summary>
    /// Provides event arguments for when a <see cref="HttpListenerContext"/>
    /// has connected, and submitted a request.
    /// </summary>
    public sealed class HttpClientEventArgs
    {
        #region Properties
        /// <summary>
        /// Gets the connection time.
        /// </summary>
        public DateTime ConnectionTime { get; }

        /// <summary>
        /// Gets the context that connected.
        /// </summary>
        public HttpListenerContext Context { get; }

        /// <summary>
        /// Gets the method that was used by the request.
        /// </summary>
        public HttpMethod HttpMethod { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instancec.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="connectionTime"></param>
        public HttpClientEventArgs(HttpListenerContext context, DateTime connectionTime)
        {
            this.Context = context;
            this.ConnectionTime = connectionTime;

            this.HttpMethod = new HttpMethod(context.Request.HttpMethod);
        }
        #endregion
    }
}