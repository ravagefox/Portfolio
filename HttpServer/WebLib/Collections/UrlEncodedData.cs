// Source: UrlEncodedData
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebLib.Collections
{
    /// <summary>
    /// Extracts any url encoded parameters from a <see cref="Uri"/>.
    /// </summary>
    public sealed class UrlEncodedData : IEnumerable<string>
    {
        #region Properties
        /// <summary>
        /// Gets the <see cref="System.Uri"/> that was 
        /// used.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets whether there is a method name that will be executed.
        /// </summary>
        public bool HasMethodName => this._data.Any(d => StringComparer.OrdinalIgnoreCase.Compare(d.Key, MethodNameKey) == 0);
        #endregion

        #region Fields
        private Dictionary<string, string> _data;

        /// <summary>
        /// Returns the key that is auto bound to execute method
        /// names.
        /// </summary>
        public const string MethodNameKey = "Modal";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uri"></param>
        public UrlEncodedData(Uri uri, Dictionary<string, string> data)
        {
            this.Uri = uri;
            this._data = data;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns whether the value was found given by the name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out string value)
        {
            foreach (var _key in this._data.Keys)
            {
                if (StringComparer.OrdinalIgnoreCase.Compare(_key, name) == 0)
                {
                    value = this._data[_key];
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the name of the api method to execute.
        /// </summary>
        /// <returns></returns>
        public string GetApiMethod()
        {
            return this.HasMethodName ? this._data[MethodNameKey.ToLower()] : string.Empty;
        }

        /// <summary>
        /// Exposes the enumeration of the current instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            if (this._data == null) { yield break; }
            foreach (var key in this._data)
            {
                yield return key.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
