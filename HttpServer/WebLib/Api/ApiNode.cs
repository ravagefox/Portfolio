// Source: ApiNode
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
using System.Text;
using WebLib.Collections;

namespace WebLib.Api
{
    /// <summary>
    /// Implementations for client accessible content is handled through the 
    /// <see cref="ApiContentNode"/> navigation class.
    /// </summary>
    public abstract class ApiContentNode
    {
        #region Properties
        /// <summary>
        /// Gets the arguments used to invoke this <see cref="ApiContentNode"/>.
        /// </summary>
        public HttpClientEventArgs Context { get; private set; }

        /// <summary>
        /// Gets the requesting encoding by the <see cref="HttpListenerContext"/>.
        /// </summary>
        public Encoding RequestEncoding => this.Context.Context.Request.ContentEncoding;

        /// <summary>
        /// When overriden in a derrived class, gets the html webpage file path that
        /// would be read and sent to the context.
        /// </summary>
        protected abstract string HtmlWebpage { get; }

        #endregion

        #region Fields
        /// <summary>
        /// An event for custom handling of api events.
        /// </summary>
        public event EventHandler<HttpClientEventArgs> HandleContext;


        private ApiLibrary apiLibrary;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ApiContentNode()
        {
            this.apiLibrary = new ApiLibrary(this);
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// When overriden in a derrived class, handles the context that would.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleContext(object sender, HttpClientEventArgs e)
        {
            HandleContext?.Invoke(sender, e);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Invokes the api method specified by the name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="urlEncodedData"></param>
        public void InvokeApi(string name, UrlEncodedData urlEncodedData)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.apiLibrary.Call(name, urlEncodedData);
            }
        }

        /// <summary>
        /// Invokes the operation of the <see cref="ApiContentNode"/> and executes its context.
        /// </summary>
        /// <param name="args"></param>
        public void Invoke(HttpClientEventArgs args)
        {
            this.Context = args;

            this.OnHandleContext(this, args);
        }

        /// <summary>
        /// Returns the webpage that will be sent to the context on GET request.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public byte[] GetWebpage()
        {
            return
                string.IsNullOrEmpty(this.HtmlWebpage) ?
                Array.Empty<byte>() :
                (!File.Exists(this.HtmlWebpage) ? throw new FileNotFoundException("The specified webpage cannot be found.") :
                File.ReadAllBytes(this.HtmlWebpage));
        }
        #endregion
    }
}
