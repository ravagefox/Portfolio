// Source: HttpContentWriter
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
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;

namespace WebLib.IO
{
    /// <summary>
    /// Provides a class which prepares data to be written to the 
    /// <see cref="HttpListenerContext"/>.
    /// </summary>
    public class HttpContentWriter : IDisposable
    {
        /// <summary>
        /// Gets the instance to write data to.
        /// </summary>
        public HttpListenerContext Context { get; }

        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/>
        /// for the <see cref="HttpListenerContext"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get => (HttpStatusCode)this.Context.Response.StatusCode;
            set => this.Context.Response.StatusCode = (int)value;
        }

        /// <summary>
        /// Gets or sets the desired <see cref="System.Net.Mime.ContentType"/>
        /// for the current stream.
        /// </summary>
        public ContentType ContentType { get; set; }


        [DllImport("Kernel32.dll")]
        private static extern void RtlZeroMemory(IntPtr dst, long length);


        private IntPtr _handle;
        private int _bytesWritten;
        private bool _isFlushed;
        private bool disposedValue;
        private const int MaxSize = 65535;


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HttpContentWriter(HttpListenerContext context)
        {
            this.Context = context;
            this._handle = Marshal.AllocHGlobal(MaxSize);
        }

        /// <summary>
        /// Sets the cookie value for the <see cref="HttpListenerContext"/>.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="content"></param>
        /// <param name="httpOnly"></param>
        /// <param name="expiry"></param>
        public void SetCookie(
            string cookieName,
            string content,
            bool httpOnly = false,
            long expiry = 0)
        {
            var cookie = new Cookie(cookieName, content)
            {
                Expires = DateTime.Now.ToUniversalTime() + (expiry > 0 ? TimeSpan.FromTicks(expiry) : TimeSpan.FromHours(24)),
                Domain = this.Context.Request.Url.Host,
                HttpOnly = httpOnly,
            };
            this.Context.Response.SetCookie(cookie);
        }

        /// <summary>
        /// Appends the new header value to the current 
        /// <see cref="HttpListenerContext"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AppendHeader(string name, string value)
        {
            this.Context.Response.AppendHeader(name, value);
        }

        /// <summary>
        /// Writes the buffer to memory.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureBuffer(buffer.Length, count);

            var offsetPtr = this._handle + this._bytesWritten + offset;
            Marshal.Copy(buffer, 0, offsetPtr, count);

            this._bytesWritten += count;
        }

        /// <summary>
        /// Writes the string content to memory.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="value"></param>
        public void Write(Encoding encoding, string value)
        {
            var bytes = encoding.GetBytes(value);
            this.Write(bytes, 0, bytes.Length);
        }

        private void EnsureBuffer(int length, int count)
        {
            var max = length > count ? length : count;
            var overflow = this._bytesWritten + max;

            if (overflow > MaxSize)
            {
                throw new OutOfMemoryException(
                    $"The allocated block of memory is assigned to {MaxSize} bytes " +
                    $"and the size is causing an overflow of {max} bytes.");
            }
        }

        /// <summary>
        /// Flushes the current buffer and begins writing to the 
        /// context.
        /// </summary>
        public void Flush()
        {
            if (this._isFlushed) { return; }

            try
            {
                var arr = new byte[this._bytesWritten];
                Marshal.Copy(this._handle, arr, 0, this._bytesWritten);

                this.Context.Response.OutputStream.Write(arr, 0, arr.Length);
                this._isFlushed = true;
            }
            catch
            {
            }
        }

        private void FlushAndCloseContext()
        {
            try
            {
                this.Context.Response.ContentLength64 = this._bytesWritten;
                this.Context.Response.ContentType = this.ContentType.MediaType;

                if (!this._isFlushed)
                {
                    this.Flush();
                }

                this.Context.Response.Close();
            }
            catch (ObjectDisposedException) { }
        }

        #region Dispose
        /// <summary>
        /// When overriden in a derrived class, disposes of the current instance
        /// and its resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.FlushAndCloseContext();
                }

                RtlZeroMemory(this._handle, this._bytesWritten);
                Marshal.FreeHGlobal(this._handle);

                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// <summary>
        /// Deconstructs the current instance.
        /// </summary>
        ~HttpContentWriter()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the current instance and its resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
