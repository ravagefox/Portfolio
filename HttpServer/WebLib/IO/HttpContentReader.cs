// Source: HttpContentReader
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
using System.Net;
using System.Web;
using WebLib.Collections;

namespace WebLib.IO
{
    /// <summary>
    /// Opens a client request stream for reading.
    /// </summary>
    public sealed class HttpContentReader : IDisposable
    {
        #region Fields

        private StreamReader _reader;
        private Uri _requestUri;
        private MemoryStream _underlyingStream;
        private CookieCollection _cookies;

        #endregion

        #region Constructors

        private HttpContentReader()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens a new content reader from the specified
        /// <see cref="HttpListenerContext"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static HttpContentReader Open(HttpListenerContext e)
        {
            var reader = new HttpContentReader();
            reader._underlyingStream = CopyStream(e.Request.InputStream);
            reader._cookies = e.Request.Cookies;
            reader._reader = new StreamReader(reader._underlyingStream);
            reader._requestUri = e.Request.Url;

            return reader;
        }

        private static MemoryStream CopyStream(Stream inputStream)
        {
            var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            _ = ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        /// <summary>
        /// Returns the next available buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            return this._underlyingStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Returns the uri parameters that were specified.
        /// </summary>
        /// <returns></returns>
        public UrlEncodedData ReadUriParameters()
        {
            return string.IsNullOrEmpty(this._requestUri.Query) ||
                !this._requestUri.Query.StartsWith("?")
                ? new UrlEncodedData(this._requestUri, null)
                : this.ReadEncodedData(this._requestUri.Query.Substring(1));
        }

        /// <summary>
        /// Returns the form data that is embedded in the stream.
        /// </summary>
        /// <returns></returns>
        public UrlEncodedData ReadFormData()
        {
            var content = this.ReadToEnd();
            Console.WriteLine("CONTENT: " + content);

            return this.ReadEncodedData(content);
        }

        /// <summary>
        /// Returns the <see cref="Cookie"/> given by the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Cookie GetCookie(string name)
        {
            foreach (var cookie in this._cookies.OfType<Cookie>())
            {
                if (StringComparer.OrdinalIgnoreCase.Compare(cookie.Name, name) == 0)
                {
                    return cookie;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the next line that was read.
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            return this._reader.EndOfStream ? string.Empty :
                this._reader.ReadLine();
        }

        /// <summary>
        /// Returns the entire contents of the stream to string,
        /// regardless of its position.
        /// </summary>
        /// <returns></returns>
        public string ReadToEnd()
        {
            var oldPos = this._underlyingStream.Position;
            this._underlyingStream.Position = 0;

            var content = this._reader.ReadToEnd();
            this._underlyingStream.Position = oldPos;
            return content;
        }


        /// <summary>
        /// Disposes of the current instance and its resources.
        /// </summary>
        public void Dispose()
        {
            this._underlyingStream?.Dispose();
            this._reader?.Dispose();
        }

        #endregion

        #region Private Methods


        private UrlEncodedData ReadEncodedData(string content)
        {
            var result = new Dictionary<string, string>();
            var lines = content.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            lines.ToList()
                .ForEach(line =>
                {
                    var pair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    var value = pair.Length > 1 ? pair[1] : string.Empty;

                    result[pair[0].ToLower()] =
                    HttpUtility.HtmlDecode(HttpUtility.UrlDecode(value));
                });

            return new UrlEncodedData(this._requestUri, result);
        }


        #endregion
    }
}
