// Source: WebServer
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

namespace WebLib
{
    /// <summary>
    /// Provides a basic implementation for a Webserver interface.
    /// </summary>
    public class WebServer : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets whether the server is running.
        /// </summary>
        public bool IsRunning { get; private set; }
        #endregion

        #region Fields and Events
        /// <summary>
        /// An event for when the server is disposing.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;
        /// <summary>
        /// An event for when the server has closed.
        /// </summary>
        public event EventHandler<EventArgs> ServerClosed;

        /// <summary>
        /// An event for when the server has opened to connections.
        /// </summary>
        public event EventHandler<EventArgs> ServerOpened;

        /// <summary>
        /// An event for when the server has to handle a request made by
        /// the <see cref="HttpListenerContext"/>.
        /// </summary>
        public event EventHandler<HttpClientEventArgs> HandleContext;


        private HttpListener _listener;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public WebServer()
        {
            this._listener = new HttpListener();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified prefix to the server.
        /// </summary>
        /// <param name="url"></param>
        public void AddPrefix(string url)
        {
            if (!url.EndsWith("/")) { url += '/'; }
            this._listener.Prefixes.Add(url);
        }

        /// <summary>
        /// Begins hosting the server.
        /// </summary>
        public void Start()
        {
            if (!this.IsRunning)
            {
                try
                {
                    this.IsRunning = true;
                    this._listener.Start();
                    this.OnServerStarted(this, EventArgs.Empty);

                    this.BeginListenThread();
                }
                catch (HttpListenerException e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Stops hosting the server.
        /// </summary>
        public void Stop()
        {
            if (this.IsRunning)
            {
                try
                {
                    this._listener.Stop();
                    this.IsRunning = false;

                    this.OnServerClosed(this, EventArgs.Empty);
                }
                catch (HttpListenerException)
                {
                }
            }
        }
        #endregion

        #region Private Methods
        private void BeginListenThread()
        {
            this.ResumeListenThread();
        }

        private void ResumeListenThread()
        {
            if (!this.IsRunning) { return; }

            _ = this._listener.BeginGetContext(this.EndGetContext, null);
        }

        private void EndGetContext(IAsyncResult ar)
        {
            var context = this._listener.EndGetContext(ar);
            if (context != null)
            {
                this.OnHandleContext(
                    this,
                    new HttpClientEventArgs(context, DateTime.Now));
            }

            this.ResumeListenThread();
        }

        #endregion

        #region Virtual Methods
        /// <summary>
        /// When overriden in a derrived class, handles the context that 
        /// made a request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleContext(object sender, HttpClientEventArgs e)
        {
            HandleContext?.Invoke(sender, e);
        }

        /// <summary>
        /// When overriden in a derrived class, begins disposing of the current
        /// instance, and its resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            Disposing?.Invoke(sender, e);
        }

        /// <summary>
        /// When overriden in a derrived class, handles any events for when the 
        /// server has opened connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnServerStarted(object sender, EventArgs e)
        {
            ServerOpened?.Invoke(sender, e);
        }

        /// <summary>
        /// When overriden in a derrived class, handles any events for when the 
        /// server needs to stop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnServerClosed(object sender, EventArgs e)
        {
            ServerClosed?.Invoke(sender, e);
        }
        #endregion

        #region Dispose
        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.OnDisposing(this, EventArgs.Empty);
                }

                this.ServerClosed = null;
                this.Disposing = null;
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
